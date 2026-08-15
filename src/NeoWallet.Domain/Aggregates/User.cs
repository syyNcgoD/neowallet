using NeoWallet.Domain.Common;
using NeoWallet.Domain.Entities;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Aggregates;

public sealed class User : AggregateRoot<OwnerId>
{
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<ApiKey> _apiKeys = [];

    public Email Email { get; private set; } = default!;
    public PasswordHash PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsTwoFactorEnabled { get; private set; }
    public TotpSecret? TwoFactorSecret { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public IReadOnlyCollection<ApiKey> ApiKeys => _apiKeys.AsReadOnly();

    private User()
    {
    }

    public static Result<User> Create(
        OwnerId id,
        Email email,
        PasswordHash passwordHash,
        UserRole role)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<User>(Error.Validation("User.EmptyId", "User ID cannot be empty."));
        }

        if (email is null)
        {
            return Result.Failure<User>(Error.Validation("User.NullEmail", "Email cannot be null."));
        }

        if (passwordHash is null)
        {
            return Result.Failure<User>(Error.Validation("User.NullPasswordHash", "Password hash cannot be null."));
        }

        var user = new User();
        var @event = UserCreated.Create(id, email, passwordHash, role);
        user.RaiseEvent(@event);

        return Result.Success(user);
    }

    public Result<RefreshToken> AddRefreshToken(
        string token,
        DateTime expiresAtUtc,
        string createdByIp)
    {
        var tokenResult = RefreshToken.Create(token, expiresAtUtc, createdByIp);
        if (tokenResult.IsFailure)
        {
            return tokenResult;
        }

        var @event = RefreshTokenCreated.Create(Id, token, expiresAtUtc, createdByIp);
        RaiseEvent(@event);

        return Result.Success(tokenResult.Value);
    }

    public Result<RefreshToken> RotateRefreshToken(
        string oldToken,
        string newToken,
        DateTime newExpiresAtUtc,
        string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(oldToken))
        {
            return Result.Failure<RefreshToken>(DomainErrors.Identity.InvalidRefreshToken);
        }

        var existingToken = _refreshTokens.FirstOrDefault(t => t.Token == oldToken);
        if (existingToken is null)
        {
            return Result.Failure<RefreshToken>(DomainErrors.Identity.InvalidRefreshToken);
        }

        // Security reuse detection: If the token was already revoked, someone is trying to reuse a stolen token!
        if (existingToken.IsRevoked)
        {
            var revokeAllEvent = AllRefreshTokensRevoked.Create(Id, "Token reuse detected during rotation");
            RaiseEvent(revokeAllEvent);

            return Result.Failure<RefreshToken>(DomainErrors.Identity.RefreshTokenReused);
        }

        if (existingToken.IsExpired)
        {
            return Result.Failure<RefreshToken>(DomainErrors.Identity.RefreshTokenExpired);
        }

        // Revoke the old token
        var revokeEvent = RefreshTokenRevoked.Create(Id, oldToken, ipAddress, "Rotated to new token", newToken);
        RaiseEvent(revokeEvent);

        // Add the new token
        var newTokenResult = RefreshToken.Create(newToken, newExpiresAtUtc, ipAddress);
        if (newTokenResult.IsFailure)
        {
            return newTokenResult;
        }

        var createEvent = RefreshTokenCreated.Create(Id, newToken, newExpiresAtUtc, ipAddress);
        RaiseEvent(createEvent);

        return Result.Success(newTokenResult.Value);
    }

    public Result RevokeRefreshToken(string token, string ipAddress, string reason)
    {
        var existingToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (existingToken is null)
        {
            return Result.Failure(DomainErrors.Identity.InvalidRefreshToken);
        }

        if (existingToken.IsRevoked)
        {
            return Result.Failure(DomainErrors.Identity.RefreshTokenRevoked);
        }

        var @event = RefreshTokenRevoked.Create(Id, token, ipAddress, reason);
        RaiseEvent(@event);

        return Result.Success();
    }

    public Result EnableTwoFactor(TotpSecret secret)
    {
        if (secret is null)
        {
            return Result.Failure(Error.Validation("User.NullTotpSecret", "TOTP secret cannot be null."));
        }

        if (IsTwoFactorEnabled)
        {
            return Result.Failure(DomainErrors.Identity.TwoFactorAlreadyEnabled);
        }

        var @event = TwoFactorEnabled.Create(Id, secret);
        RaiseEvent(@event);

        return Result.Success();
    }

    public Result DisableTwoFactor()
    {
        if (!IsTwoFactorEnabled)
        {
            return Result.Failure(DomainErrors.Identity.TwoFactorNotEnabled);
        }

        var @event = TwoFactorDisabled.Create(Id);
        RaiseEvent(@event);

        return Result.Success();
    }

    public Result<ApiKey> AddApiKey(
        Guid apiKeyId,
        string name,
        string prefix,
        string keyHash,
        IReadOnlyList<string>? permissions = null,
        DateTime? expiresAtUtc = null)
    {
        var apiKeyResult = ApiKey.Create(apiKeyId, name, prefix, keyHash, permissions, DateTime.UtcNow, expiresAtUtc);
        if (apiKeyResult.IsFailure)
        {
            return apiKeyResult;
        }

        var @event = ApiKeyCreated.Create(Id, apiKeyResult.Value);
        RaiseEvent(@event);

        return Result.Success(apiKeyResult.Value);
    }

    public Result RevokeApiKey(Guid apiKeyId, string reason)
    {
        var existingKey = _apiKeys.FirstOrDefault(k => k.Id == apiKeyId);
        if (existingKey is null)
        {
            return Result.Failure(DomainErrors.Identity.ApiKeyNotFound);
        }

        if (existingKey.IsRevoked)
        {
            return Result.Failure(DomainErrors.Identity.ApiKeyAlreadyRevoked);
        }

        var @event = ApiKeyRevoked.Create(Id, apiKeyId, reason);
        RaiseEvent(@event);

        return Result.Success();
    }

    protected override void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case UserCreated e:
                Id = e.UserId;
                Email = e.Email;
                PasswordHash = e.PasswordHash;
                Role = e.Role;
                IsTwoFactorEnabled = false;
                CreatedAtUtc = e.CreatedAtUtc;
                break;

            case RefreshTokenCreated e:
                var token = RefreshToken.Create(e.Token, e.ExpiresAtUtc, e.CreatedByIp, e.CreatedAtUtc).Value;
                _refreshTokens.Add(token);
                LastLoginAtUtc = e.CreatedAtUtc;
                break;

            case RefreshTokenRevoked e:
                var targetToken = _refreshTokens.FirstOrDefault(t => t.Token == e.Token);
                targetToken?.Revoke(e.RevokedByIp, e.Reason, e.ReplacedByToken, e.RevokedAtUtc);
                break;

            case AllRefreshTokensRevoked e:
                foreach (var t in _refreshTokens.Where(t => !t.IsRevoked))
                {
                    t.Revoke("System", e.Reason, null, e.RevokedAtUtc);
                }
                break;

            case TwoFactorEnabled e:
                IsTwoFactorEnabled = true;
                TwoFactorSecret = e.Secret;
                break;

            case TwoFactorDisabled:
                IsTwoFactorEnabled = false;
                TwoFactorSecret = null;
                break;

            case ApiKeyCreated e:
                _apiKeys.Add(e.ApiKey);
                break;

            case ApiKeyRevoked e:
                var keyIndex = _apiKeys.FindIndex(k => k.Id == e.ApiKeyId);
                if (keyIndex >= 0)
                {
                    _apiKeys[keyIndex] = _apiKeys[keyIndex].Revoke(e.Reason, e.RevokedAtUtc);
                }
                break;
        }
    }
}
