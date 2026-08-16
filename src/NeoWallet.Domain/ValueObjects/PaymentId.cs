using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;

public sealed record PaymentId
{
    public Guid Value { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public PaymentId(Guid value)
    {
        Value = value;
    }

    public static PaymentId New() => new(Guid.NewGuid());

    public static PaymentId Empty => new(Guid.Empty);

    public static Result<PaymentId> From(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<PaymentId>(Error.Validation("PaymentId.Empty", "Payment ID cannot be empty."));
        }

        return Result.Success(new PaymentId(value));
    }

    public static implicit operator Guid(PaymentId id) => id.Value;

    public override string ToString() => Value.ToString();
}
