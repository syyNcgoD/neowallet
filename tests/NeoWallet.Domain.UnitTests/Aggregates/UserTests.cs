using FluentAssertions;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.Aggregates;

public sealed class UserTests
{
    private readonly OwnerId _userId = OwnerId.New();
    private readonly Email _email = Email.Create("user@neowallet.com").Value;
    private readonly PasswordHash _passwordHash = PasswordHash.Create("hash_value_123").Value;

    [Fact]
    public void Create_WithValidParameters_ShouldInitializeUserAndEmitUserCreatedEvent()
    {
        var result = User.Create(_userId, _email, _passwordHash, UserRole.Customer);

        result.IsSuccess.Should().BeTrue();
        var user = result.Value;
        user.Id.Should().Be(_userId);
        user.Email.Should().Be(_email);
        user.PasswordHash.Should().Be(_passwordHash);
        user.Role.Should().Be(UserRole.Customer);
        user.IsTwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();
        user.RefreshTokens.Should().BeEmpty();
        user.ApiKeys.Should().BeEmpty();

        user.UncommittedEvents.Should().HaveCount(1);
        var @event = user.UncommittedEvents.Single().Should().BeOfType<UserCreated>().Subject;
        @event.UserId.Should().Be(_userId);
        @event.Email.Should().Be(_email);
    }

    [Fact]
    public void Create_WithInvalidParameters_ShouldReturnValidationFailure()
    {
        var r1 = User.Create(OwnerId.Empty, _email, _passwordHash, UserRole.Customer);
        var r2 = User.Create(_userId, null!, _passwordHash, UserRole.Customer);
        var r3 = User.Create(_userId, _email, null!, UserRole.Customer);

        r1.IsFailure.Should().BeTrue();
        r2.IsFailure.Should().BeTrue();
        r3.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AddRefreshToken_WithValidToken_ShouldStoreTokenAndEmitEvent()
    {
        var user = User.Create(_userId, _email, _passwordHash, UserRole.Customer).Value;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var result = user.AddRefreshToken("token_xyz", expiresAt, "127.0.0.1");

        result.IsSuccess.Should().BeTrue();
        user.RefreshTokens.Should().HaveCount(1);
        var token = user.RefreshTokens.Single();
        token.Token.Should().Be("token_xyz");
        token.IsActive.Should().BeTrue();

        var @event = user.UncommittedEvents.Last().Should().BeOfType<RefreshTokenCreated>().Subject;
        @event.Token.Should().Be("token_xyz");
    }

    [Fact]
    public void RotateRefreshToken_WhenValid_ShouldRevokeOldAndAddNewToken()
    {
        var user = User.Create(_userId, _email, _passwordHash, UserRole.Customer).Value;
        user.AddRefreshToken("old_token", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        var newExpiry = DateTime.UtcNow.AddDays(7);
        var rotateResult = user.RotateRefreshToken("old_token", "new_token", newExpiry, "192.168.1.1");

        rotateResult.IsSuccess.Should().BeTrue();
        user.RefreshTokens.Should().HaveCount(2);

        var oldToken = user.RefreshTokens.First(t => t.Token == "old_token");
        var newToken = user.RefreshTokens.First(t => t.Token == "new_token");

        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be("new_token");
        newToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RotateRefreshToken_WhenReusingRevokedToken_ShouldRevokeAllTokensAndReturnRefreshTokenReused()
    {
        // Arrange
        var user = User.Create(_userId, _email, _passwordHash, UserRole.Customer).Value;
        user.AddRefreshToken("token_1", DateTime.UtcNow.AddDays(7), "127.0.0.1");
        user.AddRefreshToken("token_2", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        // Rotate token_1 -> token_3 (revokes token_1)
        user.RotateRefreshToken("token_1", "token_3", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        // Act: Hacker tries to use revoked token_1 again (Token Theft!)
        var reuseResult = user.RotateRefreshToken("token_1", "token_hacker", DateTime.UtcNow.AddDays(7), "10.0.0.1");

        // Assert
        reuseResult.IsFailure.Should().BeTrue();
        reuseResult.Error.Code.Should().Be(DomainErrors.Identity.RefreshTokenReused.Code);

        // Security check: ALL tokens must now be revoked!
        user.RefreshTokens.Should().OnlyContain(t => t.IsRevoked);
        user.UncommittedEvents.Should().ContainSingle(e => e is AllRefreshTokensRevoked);
    }

    [Fact]
    public void RotateRefreshToken_WithNonExistentToken_ShouldReturnInvalidRefreshToken()
    {
        var user = User.Create(_userId, _email, _passwordHash, UserRole.Customer).Value;

        var result = user.RotateRefreshToken("non_existent", "new_token", DateTime.UtcNow.AddDays(7), "127.0.0.1");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.Identity.InvalidRefreshToken.Code);
    }

    [Fact]
    public void TwoFactor_Lifecycle_ShouldWorkAccurately()
    {
        var user = User.Create(_userId, _email, _passwordHash, UserRole.Customer).Value;
        var secret = TotpSecret.Generate();

        // 1. Enable 2FA
        var enableResult = user.EnableTwoFactor(secret);
        enableResult.IsSuccess.Should().BeTrue();
        user.IsTwoFactorEnabled.Should().BeTrue();
        user.TwoFactorSecret.Should().Be(secret);

        // 2. Duplicate Enable should fail
        var duplicateEnable = user.EnableTwoFactor(secret);
        duplicateEnable.IsFailure.Should().BeTrue();
        duplicateEnable.Error.Code.Should().Be(DomainErrors.Identity.TwoFactorAlreadyEnabled.Code);

        // 3. Disable 2FA
        var disableResult = user.DisableTwoFactor();
        disableResult.IsSuccess.Should().BeTrue();
        user.IsTwoFactorEnabled.Should().BeFalse();
        user.TwoFactorSecret.Should().BeNull();

        // 4. Duplicate Disable should fail
        var duplicateDisable = user.DisableTwoFactor();
        duplicateDisable.IsFailure.Should().BeTrue();
        duplicateDisable.Error.Code.Should().Be(DomainErrors.Identity.TwoFactorNotEnabled.Code);
    }

    [Fact]
    public void ApiKey_Lifecycle_ShouldWorkAccurately()
    {
        var user = User.Create(_userId, _email, _passwordHash, UserRole.Merchant).Value;
        var apiKeyId = Guid.NewGuid();

        // 1. Add API Key
        var addResult = user.AddApiKey(apiKeyId, "Production Server", "nw_live_a1b2", "hash_sha256", ["wallets:read", "wallets:transfer"]);
        addResult.IsSuccess.Should().BeTrue();
        user.ApiKeys.Should().HaveCount(1);
        var key = user.ApiKeys.Single();
        key.Name.Should().Be("Production Server");
        key.IsActive.Should().BeTrue();
        key.Permissions.Should().Contain("wallets:transfer");

        // 2. Revoke API Key
        var revokeResult = user.RevokeApiKey(apiKeyId, "Compromised key");
        revokeResult.IsSuccess.Should().BeTrue();
        key = user.ApiKeys.Single();
        key.IsRevoked.Should().BeTrue();
        key.RevokeReason.Should().Be("Compromised key");

        // 3. Duplicate Revoke should fail
        var duplicateRevoke = user.RevokeApiKey(apiKeyId, "Again");
        duplicateRevoke.IsFailure.Should().BeTrue();
        duplicateRevoke.Error.Code.Should().Be(DomainErrors.Identity.ApiKeyAlreadyRevoked.Code);
    }

    [Fact]
    public void EventSourcing_LoadFromHistory_ShouldReconstructUserState()
    {
        var userId = OwnerId.New();
        var email = Email.Create("admin@neowallet.com").Value;
        var passwordHash = PasswordHash.Create("hashed_pass").Value;
        var secret = TotpSecret.Generate();
        var apiKeyId = Guid.NewGuid();
        var apiKey = ApiKey.Create(apiKeyId, "Key", "nw_live_pfx", "hash123").Value;

        var history = new List<IDomainEvent>
        {
            UserCreated.Create(userId, email, passwordHash, UserRole.Admin),
            RefreshTokenCreated.Create(userId, "tok_1", DateTime.UtcNow.AddDays(7), "1.1.1.1"),
            RefreshTokenCreated.Create(userId, "tok_2", DateTime.UtcNow.AddDays(7), "1.1.1.1"),
            RefreshTokenRevoked.Create(userId, "tok_1", "1.1.1.1", "Rotated", "tok_2"),
            TwoFactorEnabled.Create(userId, secret),
            ApiKeyCreated.Create(userId, apiKey)
        };

        var rehydrated = (User)Activator.CreateInstance(typeof(User), nonPublic: true)!;
        rehydrated.LoadFromHistory(history);

        rehydrated.Id.Should().Be(userId);
        rehydrated.Email.Should().Be(email);
        rehydrated.Role.Should().Be(UserRole.Admin);
        rehydrated.IsTwoFactorEnabled.Should().BeTrue();
        rehydrated.TwoFactorSecret.Should().Be(secret);
        rehydrated.RefreshTokens.Should().HaveCount(2);
        rehydrated.RefreshTokens.First(t => t.Token == "tok_1").IsRevoked.Should().BeTrue();
        rehydrated.RefreshTokens.First(t => t.Token == "tok_2").IsActive.Should().BeTrue();
        rehydrated.ApiKeys.Should().HaveCount(1);
        rehydrated.UncommittedEvents.Should().BeEmpty();
    }
}
