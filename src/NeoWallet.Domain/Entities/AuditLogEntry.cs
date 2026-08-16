using System.Security.Cryptography;
using System.Text;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.Entities;

public sealed class AuditLogEntry : Entity<Guid>
{
    public const string GenesisHash = "0000000000000000000000000000000000000000000000000000000000000000";

    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string EventDataJson { get; private set; } = string.Empty;
    public string PreviousHash { get; private set; } = string.Empty;
    public string CurrentHash { get; private set; } = string.Empty;
    public long SequenceNumber { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    private AuditLogEntry()
    {
    }

    public static Result<AuditLogEntry> Create(
        Guid id,
        Guid aggregateId,
        string aggregateType,
        string eventType,
        string eventDataJson,
        string previousHash,
        long sequenceNumber,
        DateTime? timestampUtc = null)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<AuditLogEntry>(Error.Validation("AuditLog.EmptyId", "Audit log ID cannot be empty."));
        }

        if (aggregateId == Guid.Empty)
        {
            return Result.Failure<AuditLogEntry>(Error.Validation("AuditLog.EmptyAggregateId", "Aggregate ID cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(aggregateType))
        {
            return Result.Failure<AuditLogEntry>(Error.Validation("AuditLog.EmptyAggregateType", "Aggregate type cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            return Result.Failure<AuditLogEntry>(Error.Validation("AuditLog.EmptyEventType", "Event type cannot be empty."));
        }

        var prevHash = string.IsNullOrWhiteSpace(previousHash) ? GenesisHash : previousHash.Trim().ToLowerInvariant();
        var time = timestampUtc ?? DateTime.UtcNow;
        var currentHash = ComputeHash(prevHash, aggregateId, aggregateType, eventType, eventDataJson, sequenceNumber, time);

        var entry = new AuditLogEntry
        {
            Id = id,
            AggregateId = aggregateId,
            AggregateType = aggregateType.Trim(),
            EventType = eventType.Trim(),
            EventDataJson = eventDataJson ?? string.Empty,
            PreviousHash = prevHash,
            CurrentHash = currentHash,
            SequenceNumber = sequenceNumber,
            TimestampUtc = time
        };

        return Result.Success(entry);
    }

    public static string ComputeHash(
        string previousHash,
        Guid aggregateId,
        string aggregateType,
        string eventType,
        string eventDataJson,
        long sequenceNumber,
        DateTime timestampUtc)
    {
        var rawPayload = $"{previousHash}|{aggregateId}|{aggregateType}|{eventType}|{eventDataJson}|{sequenceNumber}|{timestampUtc:O}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawPayload));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public bool VerifyIntegrity(string expectedPreviousHash)
    {
        var normalizedExpectedPrev = string.IsNullOrWhiteSpace(expectedPreviousHash) ? GenesisHash : expectedPreviousHash.Trim().ToLowerInvariant();
        if (!string.Equals(PreviousHash, normalizedExpectedPrev, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var recomputedHash = ComputeHash(
            PreviousHash,
            AggregateId,
            AggregateType,
            EventType,
            EventDataJson,
            SequenceNumber,
            TimestampUtc);

        return string.Equals(CurrentHash, recomputedHash, StringComparison.OrdinalIgnoreCase);
    }
}
