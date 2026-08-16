using NeoWallet.Domain.Common;
using NeoWallet.Domain.Entities;

namespace NeoWallet.Application.Common.Interfaces;

public interface IReconciliationService
{
    Task<Result<ReconciliationReport>> RunReconciliationAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}

public interface IDiscrepancyNotifier
{
    Task NotifyDiscrepancyAsync(
        ReconciliationReport report,
        CancellationToken cancellationToken = default);
}
