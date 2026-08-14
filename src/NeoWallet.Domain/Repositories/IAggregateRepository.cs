using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.Repositories;
public interface IAggregateRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<Result<TAggregate>> LoadAsync(
        TId id,
        long? expectedVersion = null,
        CancellationToken cancellationToken = default);
    Task<Result> StoreAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> ExistsAsync(
        TId id,
        CancellationToken cancellationToken = default);
}
