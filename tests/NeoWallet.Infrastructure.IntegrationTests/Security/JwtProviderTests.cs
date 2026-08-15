using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.Authentication;
using NeoWallet.Infrastructure.Authentication.Options;

namespace NeoWallet.Infrastructure.IntegrationTests.Security;

public sealed class JwtProviderTests
{
    private readonly JwtProvider _sut;
    private readonly JwtSettings _settings = new()
    {
        Issuer = "NeoWallet.Test",
        Audience = "NeoWallet.TestApi",
        SecretKey = "super_long_and_extremely_secure_key_for_testing_purposes_only!",
        AccessTokenExpirationMinutes = 15,
        RefreshTokenExpirationDays = 7
    };

    public JwtProviderTests()
    {
        _sut = new JwtProvider(Options.Create(_settings));
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeStandardClaims()
    {
        var user = User.Create(
            OwnerId.New(),
            Email.Create("user@domain.com").Value,
            PasswordHash.Create("hash").Value,
            UserRole.Merchant).Value;

        var token = _sut.GenerateAccessToken(user, twoFactorVerified: true);

        token.Should().NotBeNullOrWhiteSpace();

        var principal = _sut.ValidateToken(token);
        principal.Should().NotBeNull();
        (principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? principal.FindFirst("sub")?.Value).Should().Be(user.Id.Value.ToString());
        (principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? principal.FindFirst("email")?.Value).Should().Be("user@domain.com");
        (principal.FindFirst(ClaimTypes.Role)?.Value ?? principal.FindFirst("role")?.Value).Should().Be("Merchant");
        principal.FindFirst("two_factor_verified")!.Value.Should().Be("true");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceHighEntropyString()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);
        token1.Length.Should().BeGreaterThan(50);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("invalid.jwt.token")]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull(string? token)
    {
        var principal = _sut.ValidateToken(token!);
        principal.Should().BeNull();
    }
}
