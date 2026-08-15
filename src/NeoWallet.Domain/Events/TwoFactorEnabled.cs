using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record TwoFactorEnabled(
    Guid EventId,
    OwnerId UserId,
    TotpSecret Secret,
    DateTime EnabledAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => EnabledAtUtc;

    public static TwoFactorEnabled Create(OwnerId userId, TotpSecret secret, DateTime? enabledAtUtc = null) =>
        new(Guid.NewGuid(), userId, secret, enabledAtUtc ?? DateTime.UtcNow);
}
