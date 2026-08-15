using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken()
    {
    }

    public static Result<RefreshToken> Create(
        string token,
        DateTime expiresAtUtc,
        string createdByIp,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure<RefreshToken>(Error.Validation("RefreshToken.Empty", "Token string cannot be empty."));
        }

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            return Result.Failure<RefreshToken>(Error.Validation("RefreshToken.InvalidExpiry", "Expiry date must be in the future."));
        }

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            CreatedByIp = createdByIp ?? string.Empty
        };

        return Result.Success(refreshToken);
    }

    public Result Revoke(
        string ipAddress,
        string reason,
        string? replacedByToken = null,
        DateTime? revokedAtUtc = null)
    {
        if (IsRevoked)
        {
            return Result.Failure(Error.Conflict("RefreshToken.AlreadyRevoked", "Refresh token has already been revoked."));
        }

        RevokedAtUtc = revokedAtUtc ?? DateTime.UtcNow;
        RevokedByIp = ipAddress;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;

        return Result.Success();
    }
}
