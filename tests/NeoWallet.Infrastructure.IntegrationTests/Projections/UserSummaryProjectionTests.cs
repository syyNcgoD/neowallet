using FluentAssertions;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.Projections;

namespace NeoWallet.Infrastructure.IntegrationTests.Projections;

public sealed class UserSummaryProjectionTests
{
    private readonly OwnerId _userId = OwnerId.New();
    private readonly Email _email = Email.Create("user@domain.com").Value;
    private readonly PasswordHash _passwordHash = PasswordHash.Create("hash123").Value;

    [Fact]
    public void Create_FromUserCreated_ShouldInitializeSummary()
    {
        var @event = UserCreated.Create(_userId, _email, _passwordHash, UserRole.Admin);

        var summary = UserSummaryProjection.Create(@event);

        summary.Id.Should().Be(_userId.Value);
        summary.Email.Should().Be("user@domain.com");
        summary.PasswordHash.Should().Be("hash123");
        summary.Role.Should().Be(UserRole.Admin);
        summary.IsTwoFactorEnabled.Should().BeFalse();
        summary.TwoFactorSecret.Should().BeNull();
        summary.ActiveApiKeyHashes.Should().BeEmpty();
        summary.Version.Should().Be(0);
    }

    [Fact]
    public void Apply_RefreshTokenCreated_ShouldUpdateLastLoginAndVersion()
    {
        var summary = UserSummaryProjection.Create(UserCreated.Create(_userId, _email, _passwordHash, UserRole.Customer));
        var loginEvent = RefreshTokenCreated.Create(_userId, "tok", DateTime.UtcNow.AddDays(7), "1.1.1.1");

        UserSummaryProjection.Apply(loginEvent, summary);

        summary.LastLoginAtUtc.Should().Be(loginEvent.CreatedAtUtc);
        summary.Version.Should().Be(1);
    }

    [Fact]
    public void Apply_TwoFactorEnabledAndDisabled_ShouldUpdateStateAndVersion()
    {
        var summary = UserSummaryProjection.Create(UserCreated.Create(_userId, _email, _passwordHash, UserRole.Customer));
        var secret = TotpSecret.Generate();

        // Enable
        UserSummaryProjection.Apply(TwoFactorEnabled.Create(_userId, secret), summary);
        summary.IsTwoFactorEnabled.Should().BeTrue();
        summary.TwoFactorSecret.Should().Be(secret.Value);
        summary.Version.Should().Be(1);

        // Disable
        UserSummaryProjection.Apply(TwoFactorDisabled.Create(_userId), summary);
        summary.IsTwoFactorEnabled.Should().BeFalse();
        summary.TwoFactorSecret.Should().BeNull();
        summary.Version.Should().Be(2);
    }

    [Fact]
    public void Apply_ApiKeyCreatedAndRevoked_ShouldUpdateKeyListAndVersion()
    {
        var summary = UserSummaryProjection.Create(UserCreated.Create(_userId, _email, _passwordHash, UserRole.Merchant));
        var keyId = Guid.NewGuid();
        var key = ApiKey.Create(keyId, "Prod", "nw_live_123", "hash_abc").Value;

        // Created
        UserSummaryProjection.Apply(ApiKeyCreated.Create(_userId, key), summary);
        summary.ActiveApiKeyHashes.Should().Contain("hash_abc");
        summary.Version.Should().Be(1);

        // Revoked
        UserSummaryProjection.Apply(ApiKeyRevoked.Create(_userId, keyId, "Revoked"), summary);
        summary.Version.Should().Be(2);
    }
}
