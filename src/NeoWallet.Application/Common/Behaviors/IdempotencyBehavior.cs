using MediatR;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Common.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IIdempotencyStore? _idempotencyStore;

    public IdempotencyBehavior(IIdempotencyStore? idempotencyStore = null)
    {
        _idempotencyStore = idempotencyStore;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_idempotencyStore is null || request is not IIdempotentCommand idempotentCommand || string.IsNullOrWhiteSpace(idempotentCommand.IdempotencyKey))
        {
            return await next();
        }

        var key = idempotentCommand.IdempotencyKey.Trim();

        var cachedResult = await _idempotencyStore.GetResultAsync<TResponse>(key, cancellationToken);
        if (cachedResult is not null)
        {
            return cachedResult;
        }

        var response = await next();

        if (response.IsSuccess)
        {
            await _idempotencyStore.StoreResultAsync(key, response, TimeSpan.FromHours(24), cancellationToken);
        }

        return response;
    }
}
