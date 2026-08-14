using Marten;
using Marten.Exceptions;
using Microsoft.Extensions.Logging;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;

namespace NeoWallet.Infrastructure.Persistence.Repositories;
public class MartenAggregateRepository<TAggregate, TId> : IAggregateRepository<TAggregate, TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    protected readonly IDocumentSession Session;
    protected readonly ILogger Logger;

    public MartenAggregateRepository(
        IDocumentSession session,
        ILogger<MartenAggregateRepository<TAggregate, TId>> logger)
    {
        Session = session;
        Logger = logger;
    }

    public virtual async Task<Result<TAggregate>> LoadAsync(
        TId id,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        var guidId = ExtractGuid(id);
        if (guidId == Guid.Empty)
        {
            return Result.Failure<TAggregate>(Error.Validation("AggregateId.Empty", "Aggregate ID cannot be empty."));
        }

        try
        {
            var eventStream = await Session.Events.FetchStreamAsync(
                guidId,
                version: expectedVersion ?? 0,
                token: cancellationToken);

            if (eventStream is null || eventStream.Count == 0)
            {
                return Result.Failure<TAggregate>(Error.NotFound($"{typeof(TAggregate).Name}.NotFound", $"{typeof(TAggregate).Name} with ID '{id}' was not found."));
            }

            var domainEvents = eventStream
                .Select(e => e.Data)
                .OfType<IDomainEvent>()
                .ToList();

            var aggregate = (TAggregate)Activator.CreateInstance(typeof(TAggregate), nonPublic: true)!;
            aggregate.LoadFromHistory(domainEvents);

            if (expectedVersion.HasValue && aggregate.Version != expectedVersion.Value)
            {
                return Result.Failure<TAggregate>(DomainErrors.Concurrency.Conflict);
            }

            return Result.Success(aggregate);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load aggregate {AggregateType} with ID {AggregateId}", typeof(TAggregate).Name, id);
            return Result.Failure<TAggregate>(Error.Failure("Aggregate.LoadFailed", "Failed to load aggregate from event store."));
        }
    }

    public virtual async Task<Result> StoreAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default)
    {
        if (aggregate is null)
        {
            return Result.Failure(Error.Validation("Aggregate.Null", "Aggregate cannot be null."));
        }

        var uncommittedEvents = aggregate.UncommittedEvents.ToList();
        if (uncommittedEvents.Count == 0)
        {
            return Result.Success();
        }

        var guidId = ExtractGuid(aggregate.Id);

        try
        {
            var initialVersion = aggregate.Version - uncommittedEvents.Count;

            if (initialVersion == -1)
            {
                Session.Events.StartStream<TAggregate>(guidId, uncommittedEvents);
            }
            else
            {
                var expectedStreamVersion = initialVersion + 1;
                Session.Events.Append(guidId, expectedStreamVersion, uncommittedEvents);
            }

            await Session.SaveChangesAsync(cancellationToken);
            aggregate.ClearUncommittedEvents();

            return Result.Success();
        }
        catch (ExistingStreamIdCollisionException)
        {
            return Result.Failure(DomainErrors.Concurrency.Conflict);
        }
        catch (MartenCommandException ex) when (ex.InnerException?.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase) == true
                                            || ex.Message.Contains("concurrency", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(DomainErrors.Concurrency.Conflict);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to store events for aggregate {AggregateType} with ID {AggregateId}", typeof(TAggregate).Name, aggregate.Id);
            return Result.Failure(Error.Failure("Aggregate.StoreFailed", "Failed to persist aggregate events."));
        }
    }

    public virtual async Task<Result<bool>> ExistsAsync(
        TId id,
        CancellationToken cancellationToken = default)
    {
        var guidId = ExtractGuid(id);
        if (guidId == Guid.Empty)
        {
            return Result.Failure<bool>(Error.Validation("AggregateId.Empty", "Aggregate ID cannot be empty."));
        }

        try
        {
            var streamState = await Session.Events.FetchStreamStateAsync(guidId, cancellationToken);
            return Result.Success(streamState is not null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check aggregate stream existence for {AggregateId}", id);
            return Result.Failure<bool>(Error.Failure("Aggregate.ExistenceCheckFailed", "Failed to check aggregate stream existence."));
        }
    }

    private static Guid ExtractGuid(TId id)
    {
        return id switch
        {
            Guid guid => guid,
            IComparable => Guid.TryParse(id.ToString(), out var parsed) ? parsed : Guid.Empty,
            _ => Guid.Empty
        };
    }
}
