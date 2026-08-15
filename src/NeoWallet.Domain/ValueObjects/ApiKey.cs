using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;

public sealed record ApiKey
{
    public Guid Id { get; }
    public string Name { get; }
    public string Prefix { get; }
    public string KeyHash { get; }
    public IReadOnlyList<string> Permissions { get; }
    public DateTime CreatedAtUtc { get; }
    public DateTime? ExpiresAtUtc { get; }
    public DateTime? RevokedAtUtc { get; }
    public string? RevokeReason { get; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsExpired => ExpiresAtUtc.HasValue && DateTime.UtcNow >= ExpiresAtUtc.Value;
    public bool IsActive => !IsRevoked && !IsExpired;

    private ApiKey(
        Guid id,
        string name,
        string prefix,
        string keyHash,
        IReadOnlyList<string> permissions,
        DateTime createdAtUtc,
        DateTime? expiresAtUtc,
        DateTime? revokedAtUtc,
        string? revokeReason)
    {
        Id = id;
        Name = name;
        Prefix = prefix;
        KeyHash = keyHash;
        Permissions = permissions;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        RevokedAtUtc = revokedAtUtc;
        RevokeReason = revokeReason;
    }

    public static Result<ApiKey> Create(
        Guid id,
        string name,
        string prefix,
        string keyHash,
        IReadOnlyList<string>? permissions = null,
        DateTime? createdAtUtc = null,
        DateTime? expiresAtUtc = null)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<ApiKey>(Error.Validation("ApiKey.EmptyId", "API key ID cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ApiKey>(Error.Validation("ApiKey.EmptyName", "API key name cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Result.Failure<ApiKey>(Error.Validation("ApiKey.EmptyPrefix", "API key prefix cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(keyHash))
        {
            return Result.Failure<ApiKey>(Error.Validation("ApiKey.EmptyKeyHash", "API key hash cannot be empty."));
        }

        return Result.Success(new ApiKey(
            id,
            name.Trim(),
            prefix.Trim(),
            keyHash.Trim(),
            permissions ?? Array.Empty<string>(),
            createdAtUtc ?? DateTime.UtcNow,
            expiresAtUtc,
            null,
            null));
    }

    public ApiKey Revoke(string reason, DateTime? revokedAtUtc = null)
    {
        return new ApiKey(
            Id,
            Name,
            Prefix,
            KeyHash,
            Permissions,
            CreatedAtUtc,
            ExpiresAtUtc,
            revokedAtUtc ?? DateTime.UtcNow,
            reason);
    }
}
