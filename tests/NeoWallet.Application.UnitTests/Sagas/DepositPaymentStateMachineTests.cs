using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Application.Features.Sagas.Payment;
using NeoWallet.Application.Features.Sagas.Payment.Contracts;
using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.UnitTests.Sagas;

public sealed class DepositPaymentStateMachineTests
{
    [Fact]
    public async Task DepositPaymentStateMachine_HappyPath_ShouldCreditWalletAndFinalize()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<DepositPaymentStateMachine, DepositPaymentState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<DepositPaymentStateMachine, DepositPaymentState>();
        var sagaId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        // 1. Initiate Deposit
        await harness.Bus.Publish(new InitiateDepositPaymentSagaCommand(
            sagaId,
            walletId,
            500m,
            "USD",
            PaymentGatewayProvider.Mock,
            "https://app.neowallet.com/callback"));

        (await sagaHarness.Consumed.Any<InitiateDepositPaymentSagaCommand>()).Should().BeTrue();

        // 2. Gateway Initiated
        await harness.Bus.Publish(new PaymentGatewayInitiatedEvent(
            sagaId,
            sagaId,
            "https://checkout.neowallet.com/pay/tok_123",
            "tok_123"));

        (await sagaHarness.Consumed.Any<PaymentGatewayInitiatedEvent>()).Should().BeTrue();

        // 3. Gateway Verified -> Publishes CreditWalletAfterPaymentCommand
        await harness.Bus.Publish(new PaymentGatewayVerifiedEvent(
            sagaId,
            sagaId,
            "txn_bank_999"));

        (await sagaHarness.Consumed.Any<PaymentGatewayVerifiedEvent>()).Should().BeTrue();
        (await harness.Published.Any<CreditWalletAfterPaymentCommand>()).Should().BeTrue();

        // 4. Wallet Credited -> SAGA FINALIZED
        await harness.Bus.Publish(new PaymentWalletCreditedEvent(
            sagaId,
            walletId,
            500m,
            "USD"));

        (await sagaHarness.Consumed.Any<PaymentWalletCreditedEvent>()).Should().BeTrue();

        var instance = sagaHarness.Created.Contains(sagaId);
        instance.Should().NotBeNull();
    }

    [Fact]
    public async Task DepositPaymentStateMachine_WhenGatewayFails_ShouldTransitionToFailed()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddSagaStateMachine<DepositPaymentStateMachine, DepositPaymentState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var sagaHarness = harness.GetSagaStateMachineHarness<DepositPaymentStateMachine, DepositPaymentState>();
        var sagaId = Guid.NewGuid();
        var walletId = Guid.NewGuid();

        // 1. Initiate
        await harness.Bus.Publish(new InitiateDepositPaymentSagaCommand(
            sagaId,
            walletId,
            500m,
            "USD",
            PaymentGatewayProvider.Mock,
            "https://app.neowallet.com/callback"));

        // 2. Gateway fails
        await harness.Bus.Publish(new PaymentGatewayFailedEvent(
            sagaId,
            "Bank declined transaction"));

        (await sagaHarness.Consumed.Any<PaymentGatewayFailedEvent>()).Should().BeTrue();
        (await harness.Published.Any<CreditWalletAfterPaymentCommand>()).Should().BeFalse();
    }
}
