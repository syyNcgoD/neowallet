using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;

/// <summary>
/// Strongly-typed identifier for the owner (user/account) of a wallet.
/// </summary>
public readonly record struct OwnerId : IComparable<OwnerId>
{
    public Guid Value { get; }

    public static OwnerId Empty => new(Guid.Empty);

    private OwnerId(Guid value)
    {
        Value = value;
    }

    public static OwnerId New() => new(Guid.NewGuid());

    public static Result<OwnerId> From(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<OwnerId>(Error.Validation("OwnerId.Empty", "Owner ID cannot be empty."));
        }

        return Result.Success(new OwnerId(value));
    }

    public static Result<OwnerId> From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            return Result.Failure<OwnerId>(Error.Validation("OwnerId.InvalidFormat", "Owner ID must be a valid GUID string."));
        }

        return From(guid);
    }

    public static implicit operator Guid(OwnerId ownerId) => ownerId.Value;

    public int CompareTo(OwnerId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
