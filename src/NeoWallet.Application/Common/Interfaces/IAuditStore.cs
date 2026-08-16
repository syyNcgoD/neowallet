using NeoWallet.Domain.Common;
using NeoWallet.Domain.Entities;

namespace NeoWallet.Application.Common.Interfaces;

public interface IAuditStore
{
    Task<Result> AppendAsync(
        Guid aggregateId,
        string aggregateType,
        string eventType,
        string eventDataJson,
        CancellationToken cancellationToken = default);

    Task<Result<bool>> VerifyChainIntegrityAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AuditLogEntry>>> GetAuditTrailAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default);
}
