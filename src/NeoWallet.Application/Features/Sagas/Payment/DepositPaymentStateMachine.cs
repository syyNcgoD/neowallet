using MassTransit;
using NeoWallet.Application.Features.Sagas.Payment.Contracts;

namespace NeoWallet.Application.Features.Sagas.Payment;

public sealed class DepositPaymentStateMachine : MassTransitStateMachine<DepositPaymentState>
{
    public State WaitingForPayment { get; private set; } = default!;
    public State CreditingWallet { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    public Event<InitiateDepositPaymentSagaCommand> DepositInitiated { get; private set; } = default!;
    public Event<PaymentGatewayInitiatedEvent> GatewayInitiated { get; private set; } = default!;
    public Event<PaymentGatewayVerifiedEvent> GatewayVerified { get; private set; } = default!;
    public Event<PaymentGatewayFailedEvent> GatewayFailed { get; private set; } = default!;
    public Event<PaymentWalletCreditedEvent> WalletCredited { get; private set; } = default!;

    public DepositPaymentStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => DepositInitiated, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => GatewayInitiated, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => GatewayVerified, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => GatewayFailed, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => WalletCredited, x => x.CorrelateById(m => m.Message.SagaId));

        Initially(
            When(DepositInitiated)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.SagaId;
                    context.Saga.WalletId = context.Message.WalletId;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.Currency = context.Message.Currency;
                    context.Saga.Provider = context.Message.Provider;
                    context.Saga.StartedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(WaitingForPayment));

        During(WaitingForPayment,
            When(GatewayInitiated)
                .Then(context =>
                {
                    context.Saga.PaymentUrl = context.Message.PaymentUrl;
                    context.Saga.PaymentToken = context.Message.PaymentToken;
                }),

            When(GatewayVerified)
                .Then(context =>
                {
                    context.Saga.ExternalTransactionId = context.Message.ExternalTransactionId;
                })
                .Publish(context => new CreditWalletAfterPaymentCommand(
                    context.Saga.CorrelationId,
                    context.Saga.WalletId,
                    context.Saga.Amount,
                    context.Saga.Currency,
                    $"PAYMENT-{context.Message.ExternalTransactionId}"))
                .TransitionTo(CreditingWallet),

            When(GatewayFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(Failed));

        During(CreditingWallet,
            When(WalletCredited)
                .Then(context =>
                {
                    context.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
