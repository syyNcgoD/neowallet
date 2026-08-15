using Marten;
using Microsoft.Extensions.Logging;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.Persistence.Repositories;

public sealed class MartenUserRepository : MartenAggregateRepository<User, OwnerId>, IUserRepository
{
    public MartenUserRepository(
        IDocumentSession session,
        ILogger<MartenUserRepository> logger)
        : base(session, logger)
    {
    }

    public async Task<Result<User>> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        if (email is null)
        {
            return Result.Failure<User>(Error.Validation("Email.Null", "Email cannot be null."));
        }

        try
        {
            var summary = await Session.Query<UserSummary>()
                .FirstOrDefaultAsync(u => u.Email == email.Value, cancellationToken);

            if (summary is null)
            {
                return Result.Failure<User>(DomainErrors.Identity.UserNotFound);
            }

            var userIdResult = OwnerId.From(summary.Id);
            if (userIdResult.IsFailure)
            {
                return Result.Failure<User>(userIdResult.Error);
            }

            return await LoadAsync(userIdResult.Value, null, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to find user by email {Email}", email.Value);
            return Result.Failure<User>(Error.Failure("User.LookupFailed", "Failed to lookup user by email."));
        }
    }

    public async Task<Result<User>> GetByApiKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
        {
            return Result.Failure<User>(DomainErrors.Identity.ApiKeyNotFound);
        }

        try
        {
            var summary = await Session.Query<UserSummary>()
                .FirstOrDefaultAsync(u => u.ActiveApiKeyHashes.Contains(keyHash), cancellationToken);

            if (summary is null)
            {
                return Result.Failure<User>(DomainErrors.Identity.ApiKeyNotFound);
            }

            var userIdResult = OwnerId.From(summary.Id);
            if (userIdResult.IsFailure)
            {
                return Result.Failure<User>(userIdResult.Error);
            }

            return await LoadAsync(userIdResult.Value, null, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to lookup user by API key hash");
            return Result.Failure<User>(Error.Failure("User.ApiKeyLookupFailed", "Failed to lookup user by API key."));
        }
    }

    public async Task<Result<bool>> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default)
    {
        if (email is null)
        {
            return Result.Failure<bool>(Error.Validation("Email.Null", "Email cannot be null."));
        }

        try
        {
            var exists = await Session.Query<UserSummary>()
                .AnyAsync(u => u.Email == email.Value, cancellationToken);

            return Result.Success(!exists);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check email uniqueness for {Email}", email.Value);
            return Result.Failure<bool>(Error.Failure("User.EmailCheckFailed", "Failed to check email uniqueness."));
        }
    }
}
