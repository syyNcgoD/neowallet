using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Repositories;

public interface IUserRepository : IAggregateRepository<User, OwnerId>
{
    Task<Result<User>> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<Result<User>> GetByApiKeyHashAsync(string keyHash, CancellationToken cancellationToken = default);
    Task<Result<bool>> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);
}
