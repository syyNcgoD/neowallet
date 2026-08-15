using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record RefreshTokenCreated(
    Guid EventId,
    OwnerId UserId,
    string Token,
    DateTime ExpiresAtUtc,
    string CreatedByIp,
    DateTime CreatedAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => CreatedAtUtc;

    public static RefreshTokenCreated Create(
        OwnerId userId,
        string token,
        DateTime expiresAtUtc,
        string createdByIp,
        DateTime? createdAtUtc = null) =>
        new(Guid.NewGuid(), userId, token, expiresAtUtc, createdByIp, createdAtUtc ?? DateTime.UtcNow);
}
