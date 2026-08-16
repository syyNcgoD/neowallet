using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Application.Features.Sagas.Transfer;
using NeoWallet.Application.Features.Sagas.Transfer.Contracts;

namespace NeoWallet.Application.UnitTests.Sagas;

public sealed class TransferStateMachineTests
{
    [Fact]
    public async Task TransferStateMachine_HappyPath_ShouldTransitionToFinalized()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<TransferStateMachine, TransferState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<TransferStateMachine, TransferState>();
        var sagaId = Guid.NewGuid();
        var sourceWalletId = Guid.NewGuid();
        var targetWalletId = Guid.NewGuid();

        // 1. Send Initiate
        await harness.Bus.Publish(new InitiateTransferSagaCommand(
            sagaId,
            sourceWalletId,
            targetWalletId,
            200m,
            "USD",
            "P2P Transfer"));

        (await sagaHarness.Consumed.Any<InitiateTransferSagaCommand>()).Should().BeTrue();
        (await harness.Published.Any<DeductSourceWalletCommand>()).Should().BeTrue();

        // 2. Publish Source Deducted
        await harness.Bus.Publish(new TransferSourceDeductedEvent(
            sagaId,
            sourceWalletId,
            targetWalletId,
            200m,
            "USD"));

        (await sagaHarness.Consumed.Any<TransferSourceDeductedEvent>()).Should().BeTrue();
        (await harness.Published.Any<CreditTargetWalletCommand>()).Should().BeTrue();

        // 3. Publish Target Credited -> SAGA FINALIZED!
        await harness.Bus.Publish(new TransferTargetCreditedEvent(
            sagaId,
            targetWalletId,
            200m,
            "USD"));

        (await sagaHarness.Consumed.Any<TransferTargetCreditedEvent>()).Should().BeTrue();

        var instance = sagaHarness.Created.Contains(sagaId);
        instance.Should().NotBeNull();
    }

    [Fact]
    public async Task TransferStateMachine_WhenTargetCreditFails_ShouldPublishCompensationCommand()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<TransferStateMachine, TransferState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<TransferStateMachine, TransferState>();
        var sagaId = Guid.NewGuid();
        var sourceWalletId = Guid.NewGuid();
        var targetWalletId = Guid.NewGuid();

        // 1. Initiate
        await harness.Bus.Publish(new InitiateTransferSagaCommand(
            sagaId,
            sourceWalletId,
            targetWalletId,
            100m,
            "USD"));

        // 2. Source deducted
        await harness.Bus.Publish(new TransferSourceDeductedEvent(
            sagaId,
            sourceWalletId,
            targetWalletId,
            100m,
            "USD"));

        // 3. Target credit fails (e.g. Target Wallet is Locked!)
        await harness.Bus.Publish(new TransferTargetFailedEvent(
            sagaId,
            "Destination wallet is locked"));

        (await sagaHarness.Consumed.Any<TransferTargetFailedEvent>()).Should().BeTrue();

        // Check that Compensation (Refund) was published!
        (await harness.Published.Any<TransferCompensateSourceCommand>()).Should().BeTrue();

        // 4. Compensation complete
        await harness.Bus.Publish(new TransferCompensatedEvent(sagaId));
        (await sagaHarness.Consumed.Any<TransferCompensatedEvent>()).Should().BeTrue();
    }

    [Fact]
    public async Task TransferStateMachine_WhenSourceDeductFails_ShouldTransitionToFailed()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<TransferStateMachine, TransferState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<TransferStateMachine, TransferState>();
        var sagaId = Guid.NewGuid();

        // 1. Initiate
        await harness.Bus.Publish(new InitiateTransferSagaCommand(
            sagaId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            5000m,
            "USD"));

        // 2. Source deduction fails (Insufficient balance)
        await harness.Bus.Publish(new TransferSourceFailedEvent(
            sagaId,
            "Insufficient funds"));

        (await sagaHarness.Consumed.Any<TransferSourceFailedEvent>()).Should().BeTrue();
        (await harness.Published.Any<CreditTargetWalletCommand>()).Should().BeFalse();
        (await harness.Published.Any<TransferCompensateSourceCommand>()).Should().BeFalse();
    }
}
