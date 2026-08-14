using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Repositories;
public interface IWalletRepository : IAggregateRepository<Wallet, WalletId>
{
    Task<Result<IReadOnlyList<IDomainEvent>>> GetEventStreamAsync(
        WalletId id,
        long fromVersion = 0,
        CancellationToken cancellationToken = default);
}
