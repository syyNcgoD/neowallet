using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.Repositories;

/// <summary>
/// Generic repository contract for Event Sourced Aggregate Roots.
/// </summary>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public interface IAggregateRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Loads an aggregate root from its historical event stream, verifying expected version if supplied.
    /// </summary>
    Task<Result<TAggregate>> LoadAsync(
        TId id,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists uncommitted domain events to the append-only event stream using optimistic concurrency control.
    /// </summary>
    Task<Result> StoreAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether an event stream exists for the given aggregate identifier.
    /// </summary>
    Task<Result<bool>> ExistsAsync(
        TId id,
        CancellationToken cancellationToken = default);
}
