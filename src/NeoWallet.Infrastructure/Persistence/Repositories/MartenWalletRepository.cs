using Marten;
using Marten.Exceptions;
using Microsoft.Extensions.Logging;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Infrastructure.Persistence.Repositories;
public sealed class MartenWalletRepository : IWalletRepository
{
    private readonly IDocumentSession _session;
    private readonly ILogger<MartenWalletRepository> _logger;

    public MartenWalletRepository(
        IDocumentSession session,
        ILogger<MartenWalletRepository> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<Result<Wallet>> LoadAsync(
        WalletId id,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<Wallet>(Error.Validation("WalletId.Empty", "Wallet ID cannot be empty."));
        }

        try
        {
            var eventStream = expectedVersion.HasValue && expectedVersion.Value > 0
                ? await _session.Events.FetchStreamAsync(id.Value, version: expectedVersion.Value + 1, token: cancellationToken)
                : await _session.Events.FetchStreamAsync(id.Value, token: cancellationToken);

            if (eventStream is null || eventStream.Count == 0)
            {
                return Result.Failure<Wallet>(DomainErrors.Wallet.NotFound(id));
            }

            var domainEvents = eventStream
                .Select(e => e.Data)
                .OfType<IDomainEvent>()
                .ToList();

            var wallet = (Wallet)Activator.CreateInstance(typeof(Wallet), nonPublic: true)!;
            wallet.LoadFromHistory(domainEvents);

            if (expectedVersion.HasValue && wallet.Version != expectedVersion.Value)
            {
                _logger.LogWarning(
                    "Concurrency conflict detected while loading wallet {WalletId}. Expected version {ExpectedVersion}, actual {ActualVersion}",
                    id.Value,
                    expectedVersion.Value,
                    wallet.Version);

                return Result.Failure<Wallet>(DomainErrors.Concurrency.Conflict);
            }

            return Result.Success(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading wallet stream for {WalletId}", id.Value);
            return Result.Failure<Wallet>(Error.Failure("Wallet.LoadFailed", $"Failed to load wallet: {ex.Message} ({ex.InnerException?.Message})"));
        }
    }

    public async Task<Result> StoreAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        if (wallet is null)
        {
            return Result.Failure(Error.Validation("Wallet.Null", "Wallet cannot be null."));
        }

        var uncommittedEvents = wallet.UncommittedEvents.ToList();
        if (uncommittedEvents.Count == 0)
        {
            return Result.Success();
        }

        try
        {
            // If the aggregate is newly created, its pre-commit version was -1
            var initialVersion = wallet.Version - uncommittedEvents.Count;

            if (initialVersion == -1)
            {
                _session.Events.StartStream<Wallet>(wallet.Id.Value, uncommittedEvents);
            }
            else
            {
                // In Marten, expected starting version is 1-based index of the first new event
                var expectedStartingVersion = initialVersion + 2;
                _session.Events.Append(wallet.Id.Value, expectedStartingVersion, uncommittedEvents);
            }

            await _session.SaveChangesAsync(cancellationToken);
            wallet.ClearUncommittedEvents();

            _logger.LogInformation(
                "Successfully stored {EventCount} events for wallet {WalletId}. Current version: {Version}",
                uncommittedEvents.Count,
                wallet.Id.Value,
                wallet.Version);

            return Result.Success();
        }
        catch (ExistingStreamIdCollisionException ex)
        {
            _logger.LogWarning(
                ex,
                "Stream collision occurred: Wallet {WalletId} already exists.",
                wallet.Id.Value);

            return Result.Failure(DomainErrors.Concurrency.Conflict);
        }
        catch (Exception ex) when (
            (ex is MartenException && (
                ex.Message.Contains("version", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("expected", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("collision", StringComparison.OrdinalIgnoreCase))) ||
            ex.Message.Contains("version", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("expected", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) ||
            ex.InnerException?.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) == true ||
            ex.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
            ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(
                ex,
                "Concurrency exception occurred while saving wallet {WalletId}.",
                wallet.Id.Value);

            return Result.Failure(DomainErrors.Concurrency.Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure storing events for wallet {WalletId}", wallet.Id.Value);
            return Result.Failure(Error.Failure("Wallet.StoreFailed", $"Failed to persist wallet events: {ex.Message} ({ex.InnerException?.Message})"));
        }
    }

    public async Task<Result<bool>> ExistsAsync(
        WalletId id,
        CancellationToken cancellationToken = default)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<bool>(Error.Validation("WalletId.Empty", "Wallet ID cannot be empty."));
        }

        try
        {
            var streamState = await _session.Events.FetchStreamStateAsync(id.Value, cancellationToken);
            return Result.Success(streamState is not null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check stream existence for wallet {WalletId}", id.Value);
            return Result.Failure<bool>(Error.Failure("Wallet.ExistenceCheckFailed", "Failed to check wallet stream existence."));
        }
    }

    public async Task<Result<IReadOnlyList<IDomainEvent>>> GetEventStreamAsync(
        WalletId id,
        long fromVersion = 0,
        CancellationToken cancellationToken = default)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<IDomainEvent>>(Error.Validation("WalletId.Empty", "Wallet ID cannot be empty."));
        }

        try
        {
            var eventStream = await _session.Events.FetchStreamAsync(
                id.Value,
                fromVersion: fromVersion,
                token: cancellationToken);

            if (eventStream is null || eventStream.Count == 0)
            {
                return Result.Failure<IReadOnlyList<IDomainEvent>>(DomainErrors.Wallet.NotFound(id));
            }

            var events = eventStream
                .Select(e => e.Data)
                .OfType<IDomainEvent>()
                .ToList()
                .AsReadOnly();

            return Result.Success<IReadOnlyList<IDomainEvent>>(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve raw event stream for wallet {WalletId}", id.Value);
            return Result.Failure<IReadOnlyList<IDomainEvent>>(Error.Failure("Wallet.StreamRetrievalFailed", "Failed to retrieve event stream."));
        }
    }
}
