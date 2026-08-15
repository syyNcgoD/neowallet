using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.Repositories;
using NeoWallet.Infrastructure.Authentication;
using NeoWallet.Infrastructure.Authentication.Options;
using NeoWallet.Infrastructure.Persistence.Options;
using NeoWallet.Infrastructure.Persistence.Repositories;
using NeoWallet.Infrastructure.Projections;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNeoWalletInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Marten configuration
        var martenSettings = new MartenSettings();
        configuration.GetSection(MartenSettings.SectionName).Bind(martenSettings);

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

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddMarten((StoreOptions options) =>
        {
            options.Connection(martenSettings.ConnectionString);
            options.DatabaseSchemaName = martenSettings.SchemaName;

            // Register Wallet Domain Event Types
            options.Events.AddEventType(typeof(WalletCreated));
            options.Events.AddEventType(typeof(MoneyDeposited));
            options.Events.AddEventType(typeof(MoneyWithdrawn));
            options.Events.AddEventType(typeof(MoneyTransferredOut));
            options.Events.AddEventType(typeof(MoneyTransferredIn));
            options.Events.AddEventType(typeof(WalletLocked));
            options.Events.AddEventType(typeof(WalletUnlocked));

            // Register User / Identity Domain Event Types
            options.Events.AddEventType(typeof(UserCreated));
            options.Events.AddEventType(typeof(RefreshTokenCreated));
            options.Events.AddEventType(typeof(RefreshTokenRevoked));
            options.Events.AddEventType(typeof(AllRefreshTokensRevoked));
            options.Events.AddEventType(typeof(TwoFactorEnabled));
            options.Events.AddEventType(typeof(TwoFactorDisabled));
            options.Events.AddEventType(typeof(ApiKeyCreated));
            options.Events.AddEventType(typeof(ApiKeyRevoked));

            // Register Real-time Inline CQRS Projections
            options.Projections.Add<WalletSummaryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            options.Projections.Add<TransactionHistoryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            options.Projections.Add<UserSummaryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);

            // Aggregate Snapshotting
            options.Projections.Snapshot<Wallet>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<User>(SnapshotLifecycle.Inline);

            // Document Schema configurations & indexes
            options.Schema.For<WalletSummary>().Identity(x => x.Id).Index(x => x.OwnerId);
            options.Schema.For<TransactionHistory>().Identity(x => x.Id).Index(x => x.WalletId);
            options.Schema.For<UserSummary>().Identity(x => x.Id).Index(x => x.Email);
        }).UseLightweightSessions();

        // Authentication & Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddSingleton<ITotpProvider, TotpProvider>();
        services.AddSingleton<IApiKeyService, ApiKeyService>();

        // Repositories
        services.AddScoped<IWalletRepository, MartenWalletRepository>();
        services.AddScoped<IUserRepository, MartenUserRepository>();
        services.AddScoped(typeof(IAggregateRepository<,>), typeof(MartenAggregateRepository<,>));

        return services;
    }
}
