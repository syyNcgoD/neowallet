namespace NeoWallet.Application.Features.Sagas.Transfer.Contracts;

public sealed record InitiateTransferSagaCommand(
    Guid SagaId,
    Guid SourceWalletId,
    Guid TargetWalletId,
    decimal Amount,
    string Currency,
    string? Reference = null);

public sealed record DeductSourceWalletCommand(
    Guid SagaId,
    Guid SourceWalletId,
    decimal Amount,
    string Currency,
    string? Reference);

public sealed record CreditTargetWalletCommand(
    Guid SagaId,
    Guid TargetWalletId,
    decimal Amount,
    string Currency,
    string? Reference);

public sealed record TransferCompensateSourceCommand(
    Guid SagaId,
    Guid SourceWalletId,
    decimal Amount,
    string Currency,
    string Reason);

public sealed record TransferSourceDeductedEvent(
    Guid SagaId,
    Guid SourceWalletId,
    Guid TargetWalletId,
    decimal Amount,
    string Currency);

public sealed record TransferSourceFailedEvent(
    Guid SagaId,
    string Reason);

public sealed record TransferTargetCreditedEvent(
    Guid SagaId,
    Guid TargetWalletId,
    decimal Amount,
    string Currency);

public sealed record TransferTargetFailedEvent(
    Guid SagaId,
    string Reason);

public sealed record TransferCompensatedEvent(
    Guid SagaId);
