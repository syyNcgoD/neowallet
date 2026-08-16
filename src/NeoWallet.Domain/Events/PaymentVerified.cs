using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record PaymentVerified(
    Guid EventId,
    PaymentId PaymentId,
    string ExternalTransactionId,
    DateTime VerifiedAtUtc) : IDomainEvent
{
    public Guid AggregateId => PaymentId.Value;
    public DateTime OccurredOnUtc => VerifiedAtUtc;

    public static PaymentVerified Create(
        PaymentId paymentId,
        string externalTransactionId,
        DateTime? verifiedAtUtc = null) =>
        new(Guid.NewGuid(), paymentId, externalTransactionId, verifiedAtUtc ?? DateTime.UtcNow);
}
