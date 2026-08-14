using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;
public sealed record WalletCreated(
    Guid EventId,
    WalletId WalletId,
    OwnerId OwnerId,
    Currency Currency,
    DateTime CreatedAtUtc) : IDomainEvent
{
    public Guid AggregateId => WalletId.Value;
    public DateTime OccurredOnUtc => CreatedAtUtc;

    public static WalletCreated Create(WalletId walletId, OwnerId ownerId, Currency currency, DateTime? createdAtUtc = null) =>
        new(Guid.NewGuid(), walletId, ownerId, currency, createdAtUtc ?? DateTime.UtcNow);
}
