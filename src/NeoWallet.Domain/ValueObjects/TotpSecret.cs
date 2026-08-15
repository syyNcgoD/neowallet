using System.Security.Cryptography;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;

public sealed record TotpSecret
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public string Value { get; }

    private TotpSecret(string value)
    {
        Value = value;
    }

    public static Result<TotpSecret> Create(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return Result.Failure<TotpSecret>(Error.Validation("TotpSecret.Empty", "TOTP secret cannot be empty."));
        }

        var normalized = secret.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");

        foreach (var c in normalized)
        {
            if (!Base32Alphabet.Contains(c))
            {
                return Result.Failure<TotpSecret>(Error.Validation("TotpSecret.InvalidBase32", "Secret must be a valid Base32 string."));
            }
        }

        if (normalized.Length < 16)
        {
            return Result.Failure<TotpSecret>(Error.Validation("TotpSecret.TooShort", "Secret must be at least 16 Base32 characters."));
        }

        return Result.Success(new TotpSecret(normalized));
    }

    public static TotpSecret Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        var base32 = ToBase32(bytes);
        return new TotpSecret(base32);
    }

    public string GenerateQrCodeUri(string issuer, string accountName)
    {
        Guard.AgainstNullOrWhiteSpace(issuer, nameof(issuer));
        Guard.AgainstNullOrWhiteSpace(accountName, nameof(accountName));

        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedAccount = Uri.EscapeDataString(accountName);

        return $"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={Value}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    private static string ToBase32(byte[] data)
    {
        var result = new System.Text.StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0;
        int bitsLeft = 0;

        foreach (byte b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                bitsLeft -= 5;
                result.Append(Base32Alphabet[(buffer >> bitsLeft) & 31]);
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
        }

        return result.ToString();
    }

    public override string ToString() => "********";
}
