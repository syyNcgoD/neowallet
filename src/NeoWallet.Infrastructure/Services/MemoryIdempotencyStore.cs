using System.Collections.Concurrent;
using NeoWallet.Application.Common.Interfaces;

namespace NeoWallet.Infrastructure.Services;

public sealed class MemoryIdempotencyStore : IIdempotencyStore
{
    private sealed record CacheEntry(object Result, DateTime ExpiresAtUtc);
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow <= entry.ExpiresAtUtc)
            {
                return Task.FromResult(true);
            }
            _store.TryRemove(key, out _);
        }
        return Task.FromResult(false);
    }

    public Task<TResult?> GetResultAsync<TResult>(string key, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow <= entry.ExpiresAtUtc)
            {
                return Task.FromResult((TResult?)entry.Result);
            }
            _store.TryRemove(key, out _);
        }
        return Task.FromResult<TResult?>(default);
    }

    public Task StoreResultAsync<TResult>(
        string key,
        TResult result,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        if (result is not null)
        {
            var expiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(24));
            _store[key] = new CacheEntry(result, expiresAt);
        }
        return Task.CompletedTask;
    }
}
