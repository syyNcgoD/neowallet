namespace NeoWallet.Infrastructure.Authentication.Options;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "NeoWallet.Auth";
    public string Audience { get; set; } = "NeoWallet.Api";
    public string SecretKey { get; set; } = "super_secure_secret_key_for_neowallet_system_must_be_at_least_32_bytes_long!";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
