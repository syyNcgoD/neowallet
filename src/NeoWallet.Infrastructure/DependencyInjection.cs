using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.Features.Sagas.Payment;
using NeoWallet.Application.Features.Sagas.Payment.Consumers;
using NeoWallet.Application.Features.Sagas.Transfer;
using NeoWallet.Application.Features.Sagas.Transfer.Consumers;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.Repositories;
using NeoWallet.Infrastructure.Authentication;
using NeoWallet.Infrastructure.Authentication.Options;
using NeoWallet.Infrastructure.Gateways;
using NeoWallet.Infrastructure.Persistence.Options;
using NeoWallet.Infrastructure.Persistence.Repositories;
using NeoWallet.Infrastructure.Projections;
using NeoWallet.Infrastructure.ReadModels;
using NeoWallet.Infrastructure.Services;

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
            martenSettings.ConnectionString = configuration["Marten:ConnectionString"]
                ?? configuration.GetConnectionString("Postgres")
                ?? configuration["DATABASE_URL"]
                ?? configuration["DATABASE_PUBLIC_URL"]
                ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
                ?? "Host=localhost;Port=5432;Database=neowallet;Username=postgres;Password=postgres;";
        }

        services.Configure<MartenSettings>(opts =>
        {
            opts.ConnectionString = martenSettings.ConnectionString;
            opts.SchemaName = martenSettings.SchemaName;
            opts.AutoCreateSchemaObjects = true;
        });

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddMarten((StoreOptions options) =>
        {
            options.Connection(martenSettings.ConnectionString);
            options.DatabaseSchemaName = martenSettings.SchemaName;
            options.AutoCreateSchemaObjects = JasperFx.CodeGeneration.AutoCreate.All;

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

            // Register Payment Domain Event Types
            options.Events.AddEventType(typeof(PaymentInitiated));
            options.Events.AddEventType(typeof(PaymentVerified));
            options.Events.AddEventType(typeof(PaymentSettled));
            options.Events.AddEventType(typeof(PaymentFailed));

            // Register Real-time Inline CQRS Projections
            options.Projections.Add<WalletSummaryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            options.Projections.Add<TransactionHistoryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);
            options.Projections.Add<UserSummaryProjection>(JasperFx.Events.Projections.ProjectionLifecycle.Inline);

            // Document Schema configurations & indexes
            options.Schema.For<WalletSummary>().Identity(x => x.Id).Index(x => x.OwnerId);
            options.Schema.For<TransactionHistory>().Identity(x => x.Id).Index(x => x.WalletId);
            options.Schema.For<UserSummary>().Identity(x => x.Id).Index(x => x.Email);
            options.Schema.For<NeoWallet.Domain.Entities.AuditLogEntry>().Identity(x => x.Id).Index(x => x.AggregateId).Index(x => x.SequenceNumber);
        }).UseLightweightSessions();

        // Authentication & Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddSingleton<ITotpProvider, TotpProvider>();
        services.AddSingleton<IApiKeyService, ApiKeyService>();

        // Domain Services & Read Services
        services.AddScoped<NeoWallet.Domain.Services.ITransferService, NeoWallet.Domain.Services.TransferService>();
        services.AddScoped<IWalletReadService, MartenWalletReadService>();
        services.AddSingleton<IIdempotencyStore, MemoryIdempotencyStore>();
        services.AddSingleton<IPaymentGateway, MockPaymentGateway>();

        // Audit & Reconciliation Services
        services.AddScoped<IAuditStore, NeoWallet.Infrastructure.Audit.MartenAuditStore>();
        services.AddScoped<IReconciliationService, NeoWallet.Infrastructure.Reconciliation.ReconciliationService>();
        services.AddSingleton<IDiscrepancyNotifier, NeoWallet.Infrastructure.Notifications.LoggingDiscrepancyNotifier>();

        // Repositories
        services.AddScoped<IWalletRepository, MartenWalletRepository>();
        services.AddScoped<IUserRepository, MartenUserRepository>();
        services.AddScoped(typeof(IAggregateRepository<,>), typeof(MartenAggregateRepository<,>));

        // MassTransit Saga State Machines and Consumers
        services.AddMassTransit(x =>
        {
            x.AddConsumer<DeductSourceWalletConsumer>();
            x.AddConsumer<CreditTargetWalletConsumer>();
            x.AddConsumer<CompensateSourceWalletConsumer>();
            x.AddConsumer<InitiateGatewayPaymentConsumer>();
            x.AddConsumer<CreditWalletAfterPaymentConsumer>();

            x.AddSagaStateMachine<TransferStateMachine, TransferState>()
                .InMemoryRepository();

            x.AddSagaStateMachine<DepositPaymentStateMachine, DepositPaymentState>()
                .InMemoryRepository();

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
