using Marten;
using Microsoft.Extensions.Logging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Entities;
using NeoWallet.Domain.Errors;

namespace NeoWallet.Infrastructure.Audit;

public sealed class MartenAuditStore : IAuditStore
{
    private readonly IDocumentSession _session;
    private readonly ILogger<MartenAuditStore> _logger;

    public MartenAuditStore(
        IDocumentSession session,
        ILogger<MartenAuditStore> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<Result> AppendAsync(
        Guid aggregateId,
        string aggregateType,
        string eventType,
        string eventDataJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var lastEntry = await _session.Query<AuditLogEntry>()
                .OrderByDescending(x => x.SequenceNumber)
                .FirstOrDefaultAsync(cancellationToken);

            var nextSeq = (lastEntry?.SequenceNumber ?? 0) + 1;
            var prevHash = lastEntry?.CurrentHash ?? AuditLogEntry.GenesisHash;

            var entryResult = AuditLogEntry.Create(
                Guid.NewGuid(),
                aggregateId,
                aggregateType,
                eventType,
                eventDataJson,
                prevHash,
                nextSeq,
                DateTime.UtcNow);

            if (entryResult.IsFailure)
            {
                return entryResult;
            }

            _session.Store(entryResult.Value);
            await _session.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append audit log entry for aggregate {AggregateId}", aggregateId);
            return Result.Failure(Error.Failure("AuditLog.AppendFailed", "Failed to store audit log entry."));
        }
    }

    public async Task<Result<bool>> VerifyChainIntegrityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await _session.Query<AuditLogEntry>()
                .OrderBy(x => x.SequenceNumber)
                .ToListAsync(cancellationToken);

            var expectedPrevHash = AuditLogEntry.GenesisHash;

            foreach (var entry in entries)
            {
                if (!entry.VerifyIntegrity(expectedPrevHash))
                {
                    _logger.LogCritical(
                        "SECURITY ALERT: Audit log chain integrity violation detected at Sequence {SeqNumber}, Entry ID {EntryId}!",
                        entry.SequenceNumber,
                        entry.Id);

                    return Result.Failure<bool>(DomainErrors.Audit.HashChainTampered);
                }

                expectedPrevHash = entry.CurrentHash;
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while verifying audit log hash chain");
            return Result.Failure<bool>(Error.Failure("AuditLog.VerificationError", "Failed to verify audit log chain."));
        }
    }

    public async Task<Result<IReadOnlyList<AuditLogEntry>>> GetAuditTrailAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await _session.Query<AuditLogEntry>()
                .Where(x => x.AggregateId == aggregateId)
                .OrderByDescending(x => x.TimestampUtc)
                .ToListAsync(cancellationToken);

            return Result.Success<IReadOnlyList<AuditLogEntry>>(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit trail for aggregate {AggregateId}", aggregateId);
            return Result.Failure<IReadOnlyList<AuditLogEntry>>(Error.Failure("AuditLog.QueryFailed", "Failed to query audit trail."));
        }
    }
}
