using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record PaymentFailed(
    Guid EventId,
    PaymentId PaymentId,
    string Reason,
    DateTime FailedAtUtc) : IDomainEvent
{
    public Guid AggregateId => PaymentId.Value;
    public DateTime OccurredOnUtc => FailedAtUtc;

    public static PaymentFailed Create(
        PaymentId paymentId,
        string reason,
        DateTime? failedAtUtc = null) =>
        new(Guid.NewGuid(), paymentId, reason, failedAtUtc ?? DateTime.UtcNow);
}
