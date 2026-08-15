using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record ApiKeyRevoked(
    Guid EventId,
    OwnerId UserId,
    Guid ApiKeyId,
    string Reason,
    DateTime RevokedAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => RevokedAtUtc;

    public static ApiKeyRevoked Create(OwnerId userId, Guid apiKeyId, string reason, DateTime? revokedAtUtc = null) =>
        new(Guid.NewGuid(), userId, apiKeyId, reason, revokedAtUtc ?? DateTime.UtcNow);
}
