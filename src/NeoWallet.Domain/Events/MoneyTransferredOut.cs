using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;
public sealed record MoneyTransferredOut(
    Guid EventId,
    WalletId SourceWalletId,
    WalletId TargetWalletId,
    TransactionId TransactionId,
    Money Amount,
    Money BalanceAfter,
    string? Reference,
    DateTime TransferredAtUtc) : IDomainEvent
{
    public Guid AggregateId => SourceWalletId.Value;
    public DateTime OccurredOnUtc => TransferredAtUtc;

    public static MoneyTransferredOut Create(
        WalletId sourceWalletId,
        WalletId targetWalletId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        string? reference,
        DateTime? transferredAtUtc = null) =>
        new(
            Guid.NewGuid(),
            sourceWalletId,
            targetWalletId,
            transactionId,
            amount,
            balanceAfter,
            reference,
            transferredAtUtc ?? DateTime.UtcNow);
}
