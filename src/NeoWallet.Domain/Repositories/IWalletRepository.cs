using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Repositories;

/// <summary>
/// Specialized repository interface for the Wallet aggregate root.
/// </summary>
public interface IWalletRepository : IAggregateRepository<Wallet, WalletId>
{
    /// <summary>
    /// Retrieves raw historical domain events for audit and replay purposes.
    /// </summary>
    Task<Result<IReadOnlyList<IDomainEvent>>> GetEventStreamAsync(
        WalletId id,
        long fromVersion = 0,
        CancellationToken cancellationToken = default);
}
