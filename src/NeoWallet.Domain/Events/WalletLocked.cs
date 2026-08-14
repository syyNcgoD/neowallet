using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;
public sealed record WalletLocked(
    Guid EventId,
    WalletId WalletId,
    string Reason,
    DateTime LockedAtUtc) : IDomainEvent
{
    public Guid AggregateId => WalletId.Value;
    public DateTime OccurredOnUtc => LockedAtUtc;

    public static WalletLocked Create(WalletId walletId, string reason, DateTime? lockedAtUtc = null) =>
        new(Guid.NewGuid(), walletId, reason, lockedAtUtc ?? DateTime.UtcNow);
}
