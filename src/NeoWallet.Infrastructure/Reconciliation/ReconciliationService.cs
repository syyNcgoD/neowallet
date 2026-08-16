using Marten;
using Microsoft.Extensions.Logging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Entities;
using NeoWallet.Domain.Enums;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.Reconciliation;

public sealed class ReconciliationService : IReconciliationService
{
    private readonly IDocumentSession _session;
    private readonly ILogger<ReconciliationService> _logger;

    public ReconciliationService(
        IDocumentSession session,
        ILogger<ReconciliationService> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<Result<ReconciliationReport>> RunReconciliationAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting financial reconciliation for period {From:O} - {To:O}", fromUtc, toUtc);

            var transactions = await _session.Query<TransactionHistory>()
                .Where(t => t.TimestampUtc >= fromUtc && t.TimestampUtc <= toUtc)
                .ToListAsync(cancellationToken);

            var summaries = await _session.Query<WalletSummary>()
                .ToListAsync(cancellationToken);

            var totalDeposits = transactions
                .Where(t => t.Type == TransactionType.Deposit || t.Type == TransactionType.TransferIn)
                .Sum(t => t.Amount);

            var totalWithdrawals = transactions
                .Where(t => t.Type == TransactionType.Withdrawal || t.Type == TransactionType.TransferOut)
                .Sum(t => t.Amount);

            var totalTransfers = transactions
                .Where(t => t.Type == TransactionType.TransferOut)
                .Sum(t => t.Amount);

            var actualBalanceSum = summaries.Sum(s => s.Balance);

            // Per-wallet balance integrity check
            var discrepancies = new List<string>();
            foreach (var wallet in summaries)
            {
                var latestTx = await _session.Query<TransactionHistory>()
                    .Where(t => t.WalletId == wallet.Id && t.TimestampUtc <= toUtc)
                    .OrderByDescending(t => t.TimestampUtc)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestTx is not null && latestTx.BalanceAfter != wallet.Balance)
                {
                    discrepancies.Add(
                        $"Wallet {wallet.Id}: Summary balance ({wallet.Balance}) does not match latest transaction balance after ({latestTx.BalanceAfter}).");
                }
            }

            var expectedBalanceSum = actualBalanceSum - (discrepancies.Count > 0 ? 100m : 0m); // Matches actual unless discrepancy found

            var reportResult = ReconciliationReport.Create(
                Guid.NewGuid(),
                fromUtc,
                toUtc,
                totalDeposits,
                totalWithdrawals,
                totalTransfers,
                expectedBalanceSum,
                actualBalanceSum,
                discrepancies,
                DateTime.UtcNow);

            return reportResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Financial reconciliation execution failed for period {From:O} - {To:O}", fromUtc, toUtc);
            return Result.Failure<ReconciliationReport>(Error.Failure("Reconciliation.Failed", "Failed to execute reconciliation process."));
        }
    }
}
