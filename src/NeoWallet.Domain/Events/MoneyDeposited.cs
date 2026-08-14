using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;
public sealed record MoneyDeposited(
    Guid EventId,
    WalletId WalletId,
    TransactionId TransactionId,
    Money Amount,
    Money BalanceAfter,
    string? Reference,
    string? Description,
    DateTime DepositedAtUtc) : IDomainEvent
{
    public Guid AggregateId => WalletId.Value;
    public DateTime OccurredOnUtc => DepositedAtUtc;

    public static MoneyDeposited Create(
        WalletId walletId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        string? reference,
        string? description,
        DateTime? depositedAtUtc = null) =>
        new(
            Guid.NewGuid(),
            walletId,
            transactionId,
            amount,
            balanceAfter,
            reference,
            description,
            depositedAtUtc ?? DateTime.UtcNow);
}
