using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;
public readonly record struct WalletId : IComparable<WalletId>
{
    public Guid Value { get; }

    public static WalletId Empty => new(Guid.Empty);

    [System.Text.Json.Serialization.JsonConstructor]
    public WalletId(Guid value)
    {
        Value = value;
    }

    public static WalletId New() => new(Guid.NewGuid());

    public static Result<WalletId> From(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Result.Failure<WalletId>(Error.Validation("WalletId.Empty", "Wallet ID cannot be empty."));
        }

        return Result.Success(new WalletId(value));
    }

    public static Result<WalletId> From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            return Result.Failure<WalletId>(Error.Validation("WalletId.InvalidFormat", "Wallet ID must be a valid GUID string."));
        }

        return From(guid);
    }

    public static implicit operator Guid(WalletId walletId) => walletId.Value;

    public int CompareTo(WalletId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}
