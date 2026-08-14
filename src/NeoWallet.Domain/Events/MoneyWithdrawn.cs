using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;
public sealed record MoneyWithdrawn(
    Guid EventId,
    WalletId WalletId,
    TransactionId TransactionId,
    Money Amount,
    Money BalanceAfter,
    string? Reference,
    string? Description,
    DateTime WithdrawnAtUtc) : IDomainEvent
{
    public Guid AggregateId => WalletId.Value;
    public DateTime OccurredOnUtc => WithdrawnAtUtc;

    public static MoneyWithdrawn Create(
        WalletId walletId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        string? reference,
        string? description,
        DateTime? withdrawnAtUtc = null) =>
        new(
            Guid.NewGuid(),
            walletId,
            transactionId,
            amount,
            balanceAfter,
            reference,
            description,
            withdrawnAtUtc ?? DateTime.UtcNow);
}
