using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Events;

public sealed record PaymentInitiated(
    Guid EventId,
    PaymentId PaymentId,
    WalletId WalletId,
    Money Amount,
    PaymentGatewayProvider Provider,
    string ExternalReference,
    string ReturnUrl,
    DateTime CreatedAtUtc) : IDomainEvent
{
    public Guid AggregateId => PaymentId.Value;
    public DateTime OccurredOnUtc => CreatedAtUtc;

    public static PaymentInitiated Create(
        PaymentId paymentId,
        WalletId walletId,
        Money amount,
        PaymentGatewayProvider provider,
        string externalReference,
        string returnUrl,
        DateTime? createdAtUtc = null) =>
        new(Guid.NewGuid(), paymentId, walletId, amount, provider, externalReference, returnUrl, createdAtUtc ?? DateTime.UtcNow);
}
