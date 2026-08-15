using Marten.Events.Aggregation;
using NeoWallet.Domain.Events;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.Projections;

public sealed class UserSummaryProjection : SingleStreamProjection<UserSummary, Guid>
{
    public static UserSummary Create(UserCreated @event)
    {
        return new UserSummary
        {
            Id = @event.UserId.Value,
            Email = @event.Email.Value,
            PasswordHash = @event.PasswordHash.Value,
            Role = @event.Role,
            IsTwoFactorEnabled = false,
            TwoFactorSecret = null,
            ActiveApiKeyHashes = [],
            Version = 0,
            CreatedAtUtc = @event.CreatedAtUtc
        };
    }

    public static void Apply(RefreshTokenCreated @event, UserSummary current)
    {
        current.LastLoginAtUtc = @event.CreatedAtUtc;
        current.Version++;
    }

    public static void Apply(TwoFactorEnabled @event, UserSummary current)
    {
        current.IsTwoFactorEnabled = true;
        current.TwoFactorSecret = @event.Secret.Value;
        current.Version++;
    }

    public static void Apply(TwoFactorDisabled @event, UserSummary current)
    {
        current.IsTwoFactorEnabled = false;
        current.TwoFactorSecret = null;
        current.Version++;
    }

    public static void Apply(ApiKeyCreated @event, UserSummary current)
    {
        if (!current.ActiveApiKeyHashes.Contains(@event.ApiKey.KeyHash))
        {
            current.ActiveApiKeyHashes.Add(@event.ApiKey.KeyHash);
        }
        current.Version++;
    }

    public static void Apply(ApiKeyRevoked @event, UserSummary current)
    {
        current.Version++;
    }
}
