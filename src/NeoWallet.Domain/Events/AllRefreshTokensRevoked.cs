using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record AllRefreshTokensRevoked(
    Guid EventId,
    OwnerId UserId,
    string Reason,
    DateTime RevokedAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => RevokedAtUtc;

    public static AllRefreshTokensRevoked Create(
        OwnerId userId,
        string reason,
        DateTime? revokedAtUtc = null) =>
        new(Guid.NewGuid(), userId, reason, revokedAtUtc ?? DateTime.UtcNow);
}
