using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record RefreshTokenRevoked(
    Guid EventId,
    OwnerId UserId,
    string Token,
    string RevokedByIp,
    string Reason,
    string? ReplacedByToken,
    DateTime RevokedAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => RevokedAtUtc;

    public static RefreshTokenRevoked Create(
        OwnerId userId,
        string token,
        string revokedByIp,
        string reason,
        string? replacedByToken = null,
        DateTime? revokedAtUtc = null) =>
        new(Guid.NewGuid(), userId, token, revokedByIp, reason, replacedByToken, revokedAtUtc ?? DateTime.UtcNow);
}
