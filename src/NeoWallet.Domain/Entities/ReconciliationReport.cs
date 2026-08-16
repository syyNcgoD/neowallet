using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.Entities;

public sealed class ReconciliationReport : Entity<Guid>
{
    private readonly List<string> _discrepancies = [];

    public DateTime PeriodStartUtc { get; private set; }
    public DateTime PeriodEndUtc { get; private set; }
    public decimal TotalDeposits { get; private set; }
    public decimal TotalWithdrawals { get; private set; }
    public decimal TotalTransfers { get; private set; }
    public decimal ExpectedBalanceSum { get; private set; }
    public decimal ActualBalanceSum { get; private set; }
    public decimal DiscrepancyAmount { get; private set; }
    public bool HasDiscrepancy => DiscrepancyAmount != 0 || _discrepancies.Count != 0;
    public IReadOnlyList<string> Discrepancies => _discrepancies.AsReadOnly();
    public DateTime GeneratedAtUtc { get; private set; }

    private ReconciliationReport()
    {
    }

    public static Result<ReconciliationReport> Create(
        Guid id,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        decimal totalDeposits,
        decimal totalWithdrawals,
        decimal totalTransfers,
        decimal expectedBalanceSum,
        decimal actualBalanceSum,
        IEnumerable<string>? discrepancies = null,
        DateTime? generatedAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<ReconciliationReport>(Error.Validation("Reconciliation.EmptyId", "Report ID cannot be empty."));
        }

        if (periodStartUtc > periodEndUtc)
        {
            return Result.Failure<ReconciliationReport>(Error.Validation("Reconciliation.InvalidPeriod", "Start date cannot be after end date."));
        }

        var discrepancyAmount = actualBalanceSum - expectedBalanceSum;
        var report = new ReconciliationReport
        {
            Id = id,
            PeriodStartUtc = periodStartUtc,
            PeriodEndUtc = periodEndUtc,
            TotalDeposits = totalDeposits,
            TotalWithdrawals = totalWithdrawals,
            TotalTransfers = totalTransfers,
            ExpectedBalanceSum = expectedBalanceSum,
            ActualBalanceSum = actualBalanceSum,
            DiscrepancyAmount = discrepancyAmount,
            GeneratedAtUtc = generatedAtUtc ?? DateTime.UtcNow
        };

        if (discrepancies is not null)
        {
            report._discrepancies.AddRange(discrepancies);
        }

        if (discrepancyAmount != 0 && report._discrepancies.Count == 0)
        {
            report._discrepancies.Add($"Balance mismatch: Expected sum is {expectedBalanceSum:N2}, but actual sum is {actualBalanceSum:N2} (Delta: {discrepancyAmount:N2}).");
        }

        return Result.Success(report);
    }
}
