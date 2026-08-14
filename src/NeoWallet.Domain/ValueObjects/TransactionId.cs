using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;
public readonly record struct TransactionId : IComparable<TransactionId>
{
    public Guid Value { get; }

    public static TransactionId Empty => new(Guid.Empty);

    private TransactionId(Guid value)
    {
        Value = value;
    }

    public static TransactionId New() => new(Guid.NewGuid());

    public static Result<TransactionId> From(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<TransactionId>(Error.Validation("TransactionId.Empty", "Transaction ID cannot be empty."));
        }

        return Result.Success(new TransactionId(value));
    }

    public static Result<TransactionId> From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            return Result.Failure<TransactionId>(Error.Validation("TransactionId.InvalidFormat", "Transaction ID must be a valid GUID string."));
        }

        return From(guid);
    }

    public static implicit operator Guid(TransactionId transactionId) => transactionId.Value;

    public int CompareTo(TransactionId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
