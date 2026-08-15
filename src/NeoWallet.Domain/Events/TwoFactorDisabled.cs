using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record TwoFactorDisabled(
    Guid EventId,
    OwnerId UserId,
    DateTime DisabledAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => DisabledAtUtc;

    public static TwoFactorDisabled Create(OwnerId userId, DateTime? disabledAtUtc = null) =>
        new(Guid.NewGuid(), userId, disabledAtUtc ?? DateTime.UtcNow);
}
