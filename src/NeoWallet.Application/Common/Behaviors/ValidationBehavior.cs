using FluentValidation;
using MediatR;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var firstFailure = failures[0];
            var error = Error.Validation(
                firstFailure.ErrorCode ?? "Validation.Error",
                firstFailure.ErrorMessage);

            return CreateValidationResult<TResponse>(error);
        }

        return await next();
    }

    private static TResponse CreateValidationResult<TResult>(Error error)
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResponse)!;
        }

        var resultType = typeof(TResult);
        var genericType = resultType.GenericTypeArguments[0];
        var failureMethod = typeof(Result)
            .GetMethods()
            .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod && m.GetParameters().Length == 1)
            .MakeGenericMethod(genericType);

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
