namespace NeoWallet.Infrastructure.Authentication;

public interface IApiKeyService
{
    (string PlainTextKey, string Prefix, string KeyHash) GenerateApiKey(string environment = "live");
    string HashApiKey(string plainTextKey);
    string? ExtractPrefix(string plainTextKey);
}
