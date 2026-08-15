using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.DTOs.Wallet;

public sealed record WalletDto(
    Guid Id,
    Guid OwnerId,
    decimal Balance,
    string Currency,
    WalletStatus Status,
    long Version,
    DateTime CreatedAtUtc);

public sealed record WalletSummaryDto(
    Guid Id,
    Guid OwnerId,
    decimal Balance,
    string Currency,
    WalletStatus Status,
    long Version,
    DateTime? LastModifiedAtUtc);

public sealed record TransactionHistoryDto(
    Guid Id,
    Guid WalletId,
    TransactionType Type,
    decimal Amount,
    string Currency,
    decimal BalanceAfter,
    string? Reference,
    string? Description,
    Guid? RelatedWalletId,
    DateTime TimestampUtc);
