using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Infrastructure.Authentication.Options;

namespace NeoWallet.Infrastructure.Authentication;

public sealed class JwtProvider : IJwtProvider
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _securityKey;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtProvider(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        _tokenHandler.InboundClaimTypeMap.Clear();
        _tokenHandler.OutboundClaimTypeMap.Clear();
    }

    public string GenerateAccessToken(User user, bool twoFactorVerified = false)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("two_factor_enabled", user.IsTwoFactorEnabled.ToString().ToLowerInvariant()),
            new("two_factor_verified", (!user.IsTwoFactorEnabled || twoFactorVerified).ToString().ToLowerInvariant())
        };

        var signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            SigningCredentials = signingCredentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _securityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return validatedToken is JwtSecurityToken ? principal : null;
        }
        catch
        {
            return null;
        }
    }
}
