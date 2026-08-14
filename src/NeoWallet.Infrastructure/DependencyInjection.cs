using Marten;
using Marten.Events;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.Repositories;
using NeoWallet.Infrastructure.Persistence.Options;
using NeoWallet.Infrastructure.Persistence.Repositories;
using NeoWallet.Infrastructure.Projections;
using NeoWallet.Infrastructure.ReadModels;
using Weasel.Core;

namespace NeoWallet.Infrastructure;

/// <summary>
/// Service collection extension methods for registering infrastructure dependencies.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddNeoWalletInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var martenSettings = new MartenSettings();
        configuration.GetSection(MartenSettings.SectionName).Bind(martenSettings);

        // Allow connection string from root "ConnectionStrings:Postgres" or Marten section
        if (string.IsNullOrWhiteSpace(martenSettings.ConnectionString))
        {
            martenSettings.ConnectionString = configuration.GetConnectionString("Postgres")
                ?? "Host=localhost;Port=5432;Database=neowallet;Username=postgres;Password=postgres;";
        }

        services.Configure<MartenSettings>(opts =>
        {
            opts.ConnectionString = martenSettings.ConnectionString;
            opts.SchemaName = martenSettings.SchemaName;
            opts.AutoCreateSchemaObjects = martenSettings.AutoCreateSchemaObjects;
        });

        services.AddMarten((StoreOptions options) =>
        {
            options.Connection(martenSettings.ConnectionString);
            options.DatabaseSchemaName = martenSettings.SchemaName;

            // Register known domain event types
            options.Events.AddEventType(typeof(WalletCreated));
            options.Events.AddEventType(typeof(MoneyDeposited));
            options.Events.AddEventType(typeof(MoneyWithdrawn));
            options.Events.AddEventType(typeof(MoneyTransferredOut));
            options.Events.AddEventType(typeof(MoneyTransferredIn));
            options.Events.AddEventType(typeof(WalletLocked));
            options.Events.AddEventType(typeof(WalletUnlocked));

            // Register Real-time Inline CQRS Projections
            options.Projections.Add<WalletSummaryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            options.Projections.Add<TransactionHistoryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);

            // Aggregate Snapshotting
            options.Projections.Snapshot<Wallet>(Marten.Events.Projections.SnapshotLifecycle.Inline);

            // Document Schema configurations & indexes
            options.Schema.For<WalletSummary>().Identity(x => x.Id).Index(x => x.OwnerId);
            options.Schema.For<TransactionHistory>().Identity(x => x.Id).Index(x => x.WalletId);
        }).UseLightweightSessions();

        // Repositories
        services.AddScoped<IWalletRepository, MartenWalletRepository>();
        services.AddScoped(typeof(IAggregateRepository<,>), typeof(MartenAggregateRepository<,>));

        return services;
    }
}
