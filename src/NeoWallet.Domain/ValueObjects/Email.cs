using System.Text.RegularExpressions;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;

public sealed partial record Email : IComparable<Email>
{
    private static readonly Regex EmailRegex = ValidEmailRegex();

    public string Value { get; }

    [System.Text.Json.Serialization.JsonConstructor]
    public Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(Error.Validation("Email.Empty", "Email address cannot be empty."));
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > 256)
        {
            return Result.Failure<Email>(Error.Validation("Email.TooLong", "Email address cannot exceed 256 characters."));
        }

        if (!EmailRegex.IsMatch(normalizedEmail))
        {
            return Result.Failure<Email>(Error.Validation("Email.InvalidFormat", "Email address format is invalid."));
        }

        return Result.Success(new Email(normalizedEmail));
    }

    public static implicit operator string(Email email) => email.Value;

    public int CompareTo(Email? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex ValidEmailRegex();
}
