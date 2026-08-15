using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record UserCreated(
    Guid EventId,
    OwnerId UserId,
    Email Email,
    PasswordHash PasswordHash,
    UserRole Role,
    DateTime CreatedAtUtc) : IDomainEvent
{
    public Guid AggregateId => UserId.Value;
    public DateTime OccurredOnUtc => CreatedAtUtc;

    public static UserCreated Create(OwnerId userId, Email email, PasswordHash passwordHash, UserRole role, DateTime? createdAtUtc = null) =>
        new(Guid.NewGuid(), userId, email, passwordHash, role, createdAtUtc ?? DateTime.UtcNow);
}
