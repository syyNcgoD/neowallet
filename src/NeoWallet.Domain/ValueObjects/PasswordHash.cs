using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;

public sealed record PasswordHash
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static Result<PasswordHash> Create(string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            return Result.Failure<PasswordHash>(Error.Validation("PasswordHash.Empty", "Password hash cannot be empty."));
        }

        return Result.Success(new PasswordHash(hash));
    }

    public static implicit operator string(PasswordHash hash) => hash.Value;

    public override string ToString() => "********";
}
