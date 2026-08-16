using MassTransit;
using NeoWallet.Application.Features.Sagas.Transfer.Contracts;

namespace NeoWallet.Application.Features.Sagas.Transfer;

public sealed class TransferStateMachine : MassTransitStateMachine<TransferState>
{
    public State DeductingSource { get; private set; } = default!;
    public State CreditingTarget { get; private set; } = default!;
    public State Compensating { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    public Event<InitiateTransferSagaCommand> TransferInitiated { get; private set; } = default!;
    public Event<TransferSourceDeductedEvent> SourceDeducted { get; private set; } = default!;
    public Event<TransferSourceFailedEvent> SourceFailed { get; private set; } = default!;
    public Event<TransferTargetCreditedEvent> TargetCredited { get; private set; } = default!;
    public Event<TransferTargetFailedEvent> TargetFailed { get; private set; } = default!;
    public Event<TransferCompensatedEvent> Compensated { get; private set; } = default!;

    public TransferStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => TransferInitiated, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => SourceDeducted, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => SourceFailed, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => TargetCredited, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => TargetFailed, x => x.CorrelateById(m => m.Message.SagaId));
        Event(() => Compensated, x => x.CorrelateById(m => m.Message.SagaId));

        Initially(
            When(TransferInitiated)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.SagaId;
                    context.Saga.SourceWalletId = context.Message.SourceWalletId;
                    context.Saga.TargetWalletId = context.Message.TargetWalletId;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.Currency = context.Message.Currency;
                    context.Saga.Reference = context.Message.Reference;
                    context.Saga.StartedAtUtc = DateTime.UtcNow;
                })
                .Publish(context => new DeductSourceWalletCommand(
                    context.Saga.CorrelationId,
                    context.Saga.SourceWalletId,
                    context.Saga.Amount,
                    context.Saga.Currency,
                    context.Saga.Reference))
                .TransitionTo(DeductingSource));

        During(DeductingSource,
            When(SourceDeducted)
                .Publish(context => new CreditTargetWalletCommand(
                    context.Saga.CorrelationId,
                    context.Saga.TargetWalletId,
                    context.Saga.Amount,
                    context.Saga.Currency,
                    context.Saga.Reference))
                .TransitionTo(CreditingTarget),

            When(SourceFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(Failed));

        During(CreditingTarget,
            When(TargetCredited)
                .Then(context =>
                {
                    context.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .Finalize(),

            When(TargetFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .Publish(context => new TransferCompensateSourceCommand(
                    context.Saga.CorrelationId,
                    context.Saga.SourceWalletId,
                    context.Saga.Amount,
                    context.Saga.Currency,
                    $"Compensation for failed transfer: {context.Message.Reason}"))
                .TransitionTo(Compensating));

        During(Compensating,
            When(Compensated)
                .Then(context =>
                {
                    context.Saga.CompletedAtUtc = DateTime.UtcNow;
                })
                .TransitionTo(Failed));

        SetCompletedWhenFinalized();
    }
}
