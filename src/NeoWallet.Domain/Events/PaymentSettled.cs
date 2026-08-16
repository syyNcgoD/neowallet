using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record PaymentSettled(
    Guid EventId,
    PaymentId PaymentId,
    WalletId WalletId,
    Money Amount,
    DateTime SettledAtUtc) : IDomainEvent
{
    public Guid AggregateId => PaymentId.Value;
    public DateTime OccurredOnUtc => SettledAtUtc;

    public static PaymentSettled Create(
        PaymentId paymentId,
        WalletId walletId,
        Money amount,
        DateTime? settledAtUtc = null) =>
        new(Guid.NewGuid(), paymentId, walletId, amount, settledAtUtc ?? DateTime.UtcNow);
}
