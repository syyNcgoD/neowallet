using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;
public sealed record MoneyTransferredIn(
    Guid EventId,
    WalletId TargetWalletId,
    WalletId SourceWalletId,
    TransactionId TransactionId,
    Money Amount,
    Money BalanceAfter,
    string? Reference,
    DateTime TransferredAtUtc) : IDomainEvent
{
    public Guid AggregateId => TargetWalletId.Value;
    public DateTime OccurredOnUtc => TransferredAtUtc;

    public static MoneyTransferredIn Create(
        WalletId targetWalletId,
        WalletId sourceWalletId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        string? reference,
        DateTime? transferredAtUtc = null) =>
        new(
            Guid.NewGuid(),
            targetWalletId,
            sourceWalletId,
            transactionId,
            amount,
            balanceAfter,
            reference,
            transferredAtUtc ?? DateTime.UtcNow);
}
