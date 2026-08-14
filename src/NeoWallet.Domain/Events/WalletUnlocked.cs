using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

/// <summary>
/// Domain event emitted when a locked wallet is unlocked back to active status.
/// </summary>
public sealed record WalletUnlocked(
    Guid EventId,
    WalletId WalletId,
    string Reason,
    DateTime UnlockedAtUtc) : IDomainEvent
{
    public Guid AggregateId => WalletId.Value;
    public DateTime OccurredOnUtc => UnlockedAtUtc;

    public static WalletUnlocked Create(WalletId walletId, string reason, DateTime? unlockedAtUtc = null) =>
        new(Guid.NewGuid(), walletId, reason, unlockedAtUtc ?? DateTime.UtcNow);
}
