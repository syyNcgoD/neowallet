using FluentAssertions;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.Features.Audit.Queries.GetAuditTrail;
using NeoWallet.Application.Features.Audit.Queries.VerifyAuditChain;
using NeoWallet.Application.Features.Reconciliation.Commands.RunReconciliation;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Entities;
using NSubstitute;

namespace NeoWallet.Application.UnitTests.Features;

public sealed class AuditAndReconciliationTests
{
    private readonly IAuditStore _auditStore = Substitute.For<IAuditStore>();
    private readonly IReconciliationService _reconciliationService = Substitute.For<IReconciliationService>();
    private readonly IDiscrepancyNotifier _discrepancyNotifier = Substitute.For<IDiscrepancyNotifier>();

    [Fact]
    public async Task VerifyAuditChainQueryHandler_ShouldInvokeAuditStore()
    {
        _auditStore.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(true)));

        var handler = new VerifyAuditChainQueryHandler(_auditStore);
        var result = await handler.Handle(new VerifyAuditChainQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        await _auditStore.Received(1).VerifyChainIntegrityAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAuditTrailQueryHandler_ShouldReturnMappedDtos()
    {
        var aggregateId = Guid.NewGuid();
        var entry = AuditLogEntry.Create(
            Guid.NewGuid(),
            aggregateId,
            "Wallet",
            "MoneyDeposited",
            "{}",
            AuditLogEntry.GenesisHash,
            1,
            DateTime.UtcNow).Value;

        _auditStore.GetAuditTrailAsync(aggregateId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<AuditLogEntry>>([entry])));

        var handler = new GetAuditTrailQueryHandler(_auditStore);
        var result = await handler.Handle(new GetAuditTrailQuery(aggregateId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Single().AggregateId.Should().Be(aggregateId);
    }

    [Fact]
    public async Task RunReconciliationCommandHandler_WhenDiscrepancyFound_ShouldNotifyAndReturnDto()
    {
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;

        var reportWithDiscrepancy = ReconciliationReport.Create(
            Guid.NewGuid(),
            from,
            to,
            1000m,
            200m,
            100m,
            800m,
            750m).Value; // 50 discrepancy!

        _reconciliationService.RunReconciliationAsync(from, to, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(reportWithDiscrepancy)));

        var handler = new RunReconciliationCommandHandler(_reconciliationService, _discrepancyNotifier);
        var result = await handler.Handle(new RunReconciliationCommand(from, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasDiscrepancy.Should().BeTrue();
        result.Value.DiscrepancyAmount.Should().Be(-50m);

        // Verify alert was dispatched!
        await _discrepancyNotifier.Received(1).NotifyDiscrepancyAsync(reportWithDiscrepancy, Arg.Any<CancellationToken>());
    }
}
