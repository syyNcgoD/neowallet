using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.Features.Sagas.Payment.Contracts;

public sealed record InitiateDepositPaymentSagaCommand(
    Guid SagaId,
    Guid WalletId,
    decimal Amount,
    string Currency,
    PaymentGatewayProvider Provider,
    string ReturnUrl);

public sealed record PaymentGatewayInitiatedEvent(
    Guid SagaId,
    Guid PaymentId,
    string PaymentUrl,
    string PaymentToken);

public sealed record PaymentGatewayVerifyCommand(
    Guid SagaId,
    Guid PaymentId,
    string PaymentToken);

public sealed record PaymentGatewayVerifiedEvent(
    Guid SagaId,
    Guid PaymentId,
    string ExternalTransactionId);

public sealed record PaymentGatewayFailedEvent(
    Guid SagaId,
    string Reason);

public sealed record CreditWalletAfterPaymentCommand(
    Guid SagaId,
    Guid WalletId,
    decimal Amount,
    string Currency,
    string Reference);

public sealed record PaymentWalletCreditedEvent(
    Guid SagaId,
    Guid WalletId,
    decimal Amount,
    string Currency);
