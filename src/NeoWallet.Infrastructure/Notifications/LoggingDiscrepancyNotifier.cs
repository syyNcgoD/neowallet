using Microsoft.Extensions.Logging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Entities;

namespace NeoWallet.Infrastructure.Notifications;

public sealed class LoggingDiscrepancyNotifier : IDiscrepancyNotifier
{
    private readonly ILogger<LoggingDiscrepancyNotifier> _logger;

    public LoggingDiscrepancyNotifier(ILogger<LoggingDiscrepancyNotifier> logger)
    {
        _logger = logger;
    }

    public Task NotifyDiscrepancyAsync(ReconciliationReport report, CancellationToken cancellationToken = default)
    {
        _logger.LogCritical(
            "CRITICAL FINANCIAL ALERT: Discrepancy detected in reconciliation report {ReportId}! Period: {Start:O} - {End:O}, Expected Sum: {Expected}, Actual Sum: {Actual}, Discrepancy: {Discrepancy}, Details: {@Discrepancies}",
            report.Id,
            report.PeriodStartUtc,
            report.PeriodEndUtc,
            report.ExpectedBalanceSum,
            report.ActualBalanceSum,
            report.DiscrepancyAmount,
            report.Discrepancies);

        return Task.CompletedTask;
    }
}
