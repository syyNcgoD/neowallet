namespace NeoWallet.Application.Common.Interfaces;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<TResult?> GetResultAsync<TResult>(string key, CancellationToken cancellationToken = default);
    Task StoreResultAsync<TResult>(string key, TResult result, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}
