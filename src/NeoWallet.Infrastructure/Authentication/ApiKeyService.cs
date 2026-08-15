using System.Security.Cryptography;
using System.Text;

namespace NeoWallet.Infrastructure.Authentication;

public sealed class ApiKeyService : IApiKeyService
{
    private const string KeyPrefixTag = "nw";

    public (string PlainTextKey, string Prefix, string KeyHash) GenerateApiKey(string environment = "live")
    {
        var env = string.IsNullOrWhiteSpace(environment) ? "live" : environment.Trim().ToLowerInvariant();
        var prefixBytes = RandomNumberGenerator.GetBytes(4);
        var secretBytes = RandomNumberGenerator.GetBytes(24);

        var shortPrefix = Convert.ToHexString(prefixBytes).ToLowerInvariant();
        var secret = Convert.ToHexString(secretBytes).ToLowerInvariant();

        var fullPrefix = $"{KeyPrefixTag}_{env}_{shortPrefix}";
        var plainTextKey = $"{fullPrefix}_{secret}";
        var hash = HashApiKey(plainTextKey);

        return (plainTextKey, fullPrefix, hash);
    }

    public string HashApiKey(string plainTextKey)
    {
        if (string.IsNullOrWhiteSpace(plainTextKey))
        {
            throw new ArgumentException("Plain text key cannot be null or empty.", nameof(plainTextKey));
        }

        var keyBytes = Encoding.UTF8.GetBytes(plainTextKey.Trim());
        var hashBytes = SHA256.HashData(keyBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public string? ExtractPrefix(string plainTextKey)
    {
        if (string.IsNullOrWhiteSpace(plainTextKey))
        {
            return null;
        }

        var parts = plainTextKey.Trim().Split('_');
        if (parts.Length < 4 || parts[0] != KeyPrefixTag)
        {
            return null;
        }

        return $"{parts[0]}_{parts[1]}_{parts[2]}";
    }
}
