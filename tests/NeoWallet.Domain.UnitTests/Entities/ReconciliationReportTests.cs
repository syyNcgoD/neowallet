using FluentAssertions;
using NeoWallet.Domain.Entities;

namespace NeoWallet.Domain.UnitTests.Entities;

public sealed class ReconciliationReportTests
{
    [Fact]
    public void Create_WhenBalancesMatch_ShouldHaveZeroDiscrepancy()
    {
        var reportId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var result = ReconciliationReport.Create(
            reportId,
            from,
            to,
            totalDeposits: 1000m,
            totalWithdrawals: 400m,
            totalTransfers: 200m,
            expectedBalanceSum: 600m,
            actualBalanceSum: 600m);

        result.IsSuccess.Should().BeTrue();
        var report = result.Value;
        report.Id.Should().Be(reportId);
        report.DiscrepancyAmount.Should().Be(0m);
        report.HasDiscrepancy.Should().BeFalse();
        report.Discrepancies.Should().BeEmpty();
    }

    [Fact]
    public void Create_WhenBalancesMismatch_ShouldDetectDiscrepancyAndIncludeDetails()
    {
        var reportId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var result = ReconciliationReport.Create(
            reportId,
            from,
            to,
            totalDeposits: 1000m,
            totalWithdrawals: 400m,
            totalTransfers: 200m,
            expectedBalanceSum: 600m,
            actualBalanceSum: 550m); // 50 dollars missing!

        result.IsSuccess.Should().BeTrue();
        var report = result.Value;
        report.DiscrepancyAmount.Should().Be(-50m);
        report.HasDiscrepancy.Should().BeTrue();
        report.Discrepancies.Should().NotBeEmpty();
        report.Discrepancies.First().Should().Contain("Balance mismatch");
    }

    [Fact]
    public void Create_WhenPeriodIsInvalid_ShouldReturnValidationFailure()
    {
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(-1); // from > to (invalid!)

        var result = ReconciliationReport.Create(
            Guid.NewGuid(),
            from,
            to,
            100m,
            50m,
            0m,
            50m,
            50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reconciliation.InvalidPeriod");
    }
}
