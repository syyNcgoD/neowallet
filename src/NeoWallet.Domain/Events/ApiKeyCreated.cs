using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record ApiKeyCreated(
    Guid EventId,
    OwnerId UserId,
    ApiKey ApiKey,
    DateTime CreatedAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => CreatedAtUtc;

    public static ApiKeyCreated Create(OwnerId userId, ApiKey apiKey, DateTime? createdAtUtc = null) =>
        new(Guid.NewGuid(), userId, apiKey, createdAtUtc ?? DateTime.UtcNow);
}
