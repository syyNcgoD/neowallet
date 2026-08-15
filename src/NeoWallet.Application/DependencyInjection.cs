using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Application.Common.Behaviors;

namespace NeoWallet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNeoWalletApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
