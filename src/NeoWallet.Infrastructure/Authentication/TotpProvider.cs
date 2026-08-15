using System.Buffers.Binary;
using System.Security.Cryptography;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Infrastructure.Authentication;

public sealed class TotpProvider : ITotpProvider
{
    private const int TimeStepSeconds = 30;
    private const int Digits = 6;
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public string GenerateCode(TotpSecret secret, DateTime? timestampUtc = null)
    {
        Guard.AgainstNull(secret, nameof(secret));

        var time = timestampUtc ?? DateTime.UtcNow;
        var unixTimestamp = new DateTimeOffset(time).ToUnixTimeSeconds();
        var timeStep = unixTimestamp / TimeStepSeconds;

        return ComputeTotp(secret.Value, timeStep);
    }

    public bool VerifyCode(TotpSecret secret, string code, int toleranceSteps = 1, DateTime? timestampUtc = null)
    {
        if (secret is null || string.IsNullOrWhiteSpace(code) || code.Trim().Length != Digits)
        {
            return false;
        }

        var normalizedCode = code.Trim();
        var time = timestampUtc ?? DateTime.UtcNow;
        var unixTimestamp = new DateTimeOffset(time).ToUnixTimeSeconds();
        var currentTimeStep = unixTimestamp / TimeStepSeconds;

        for (int i = -toleranceSteps; i <= toleranceSteps; i++)
        {
            var calculatedCode = ComputeTotp(secret.Value, currentTimeStep + i);
            if (CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(calculatedCode),
                    System.Text.Encoding.UTF8.GetBytes(normalizedCode)))
            {
                return true;
            }
        }

        return false;
    }

    private static string ComputeTotp(string base32Secret, long timeStep)
    {
        byte[] key = FromBase32(base32Secret);
        byte[] counter = new byte[8];
        BinaryPrimitives.WriteInt64BigEndian(counter, timeStep);

        using var hmac = new HMACSHA1(key);
        byte[] hash = hmac.ComputeHash(counter);

        int offset = hash[^1] & 0x0F;
        int binaryCode = (hash[offset] & 0x7F) << 24
                       | (hash[offset + 1] & 0xFF) << 16
                       | (hash[offset + 2] & 0xFF) << 8
                       | (hash[offset + 3] & 0xFF);

        int otp = binaryCode % (int)Math.Pow(10, Digits);
        return otp.ToString(new string('0', Digits));
    }

    private static byte[] FromBase32(string base32)
    {
        var clean = base32.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        var output = new List<byte>();
        int buffer = 0;
        int bitsLeft = 0;

        foreach (char c in clean)
        {
            int val = Base32Alphabet.IndexOf(c);
            if (val < 0) continue;

            buffer = (buffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                output.Add((byte)((buffer >> bitsLeft) & 0xFF));
            }
        }

        return output.ToArray();
    }
}
