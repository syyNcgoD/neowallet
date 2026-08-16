using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.DTOs.Payment;

public sealed record PaymentInitiateRequestDto(
    Guid PaymentId,
    Guid WalletId,
    decimal Amount,
    string Currency,
    PaymentGatewayProvider Provider,
    string Description,
    string ReturnUrl);

public sealed record PaymentInitiateResultDto(
    Guid PaymentId,
    string PaymentUrl,
    string PaymentToken,
    PaymentStatus Status);

public sealed record PaymentVerifyRequestDto(
    Guid PaymentId,
    string PaymentToken,
    string? Authority = null);

public sealed record PaymentVerifyResultDto(
    Guid PaymentId,
    bool IsSuccess,
    string ExternalTransactionId,
    string? FailureReason = null);

public sealed record PaymentDto(
    Guid Id,
    Guid WalletId,
    decimal Amount,
    string Currency,
    PaymentGatewayProvider Provider,
    PaymentStatus Status,
    string ExternalReference,
    string? ExternalTransactionId,
    DateTime CreatedAtUtc,
    DateTime? SettledAtUtc);
