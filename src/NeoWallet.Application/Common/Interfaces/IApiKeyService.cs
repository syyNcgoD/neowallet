namespace NeoWallet.Application.Common.Interfaces;

public interface IApiKeyService
{
    (string PlainTextKey, string Prefix, string KeyHash) GenerateApiKey(string environment = "live");
    string HashApiKey(string plainTextKey);
    string? ExtractPrefix(string plainTextKey);
}
