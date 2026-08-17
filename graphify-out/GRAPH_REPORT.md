# Graph Report - .  (2026-08-17)

## Corpus Check
- 420 files · ~143,384 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2363 nodes · 5786 edges · 132 communities (104 shown, 28 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 156 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Marten Event Sourcing & Projections
- REST API Endpoints & DTOs
- Authentication & Security (JWT/2FA)
- MassTransit Distributed Sagas
- Dashboard & Interactive Wallet Cards
- Marten Event Sourcing & Projections
- MassTransit Distributed Sagas
- Dashboard & Interactive Wallet Cards
- REST API Endpoints & DTOs
- Dashboard & Interactive Wallet Cards
- REST API Endpoints & DTOs
- Authentication & Security (JWT/2FA)
- Audit Hash Chaining & Reconciliation
- Marten Event Sourcing & Projections
- REST API Endpoints & DTOs
- Dashboard & Interactive Wallet Cards
- Marten Event Sourcing & Projections
- Authentication & Security (JWT/2FA)
- Marten Event Sourcing & Projections
- Domain Model & Invariants
- Dashboard & Interactive Wallet Cards
- Marten Event Sourcing & Projections
- Authentication & Security (JWT/2FA)
- Marten Event Sourcing & Projections
- Shadcn UI Components & Primitives
- Marten Event Sourcing & Projections
- REST API Endpoints & DTOs
- Domain Model & Invariants
- Domain Model & Invariants
- Frontend React Query & Client Hooks
- REST API Endpoints & DTOs
- Marten Event Sourcing & Projections
- Dashboard & Interactive Wallet Cards
- 3D Interactive Globe Visualization
- Domain Model & Invariants
- REST API Endpoints & DTOs
- Dashboard & Interactive Wallet Cards
- Dashboard & Interactive Wallet Cards
- Domain Model & Invariants
- 3D Interactive Globe Visualization
- Shadcn UI Components & Primitives
- Marten Event Sourcing & Projections
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- Authentication & Security (JWT/2FA)
- Authentication & Security (JWT/2FA)
- Domain Model & Invariants
- Audit Hash Chaining & Reconciliation
- Marten Event Sourcing & Projections
- Authentication & Security (JWT/2FA)
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- MassTransit Distributed Sagas
- Authentication & Security (JWT/2FA)
- REST API Endpoints & DTOs
- MassTransit Distributed Sagas
- Dashboard & Interactive Wallet Cards
- Domain Model & Invariants
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- Marten Event Sourcing & Projections
- REST API Endpoints & DTOs
- Audit Hash Chaining & Reconciliation
- Audit Hash Chaining & Reconciliation
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- Domain Model & Invariants
- NeoWallet.Api.Extensions Module
- REST API Endpoints & DTOs
- Shadcn UI Components & Primitives
- REST API Endpoints & DTOs
- MassTransit Distributed Sagas
- Authentication & Security (JWT/2FA)
- REST API Endpoints & DTOs
- Marten Event Sourcing & Projections
- Shadcn UI Components & Primitives
- MassTransit Distributed Sagas
- MassTransit Distributed Sagas
- Unit & Integration Test Suites
- Marten Event Sourcing & Projections
- Audit Hash Chaining & Reconciliation
- Audit Hash Chaining & Reconciliation
- Unit & Integration Test Suites
- Shadcn UI Components & Primitives
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- Unit & Integration Test Suites
- REST API Endpoints & DTOs
- REST API Endpoints & DTOs
- SignalR Real-time Event Streaming
- Unit & Integration Test Suites
- Unit & Integration Test Suites
- Unit & Integration Test Suites
- Domain Model & Invariants
- SignalR Real-time Event Streaming
- Transactions & Ledger History UI
- REST API Endpoints & DTOs
- Domain Model & Invariants
- REST API Endpoints & DTOs
- MassTransit Distributed Sagas
- NeoWallet.Application Module
- SignalR Real-time Event Streaming
- MassTransit Distributed Sagas
- Frontend React Query & Client Hooks
- Shadcn UI Components & Primitives
- Authentication & Security (JWT/2FA)
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Authentication & Security (JWT/2FA)
- SignalR Real-time Event Streaming
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- 3D Interactive Globe Visualization
- 3D Interactive Globe Visualization
- Analytics & Financial Charts
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Shadcn UI Components & Primitives
- Frontend React Query & Client Hooks
- Frontend React Query & Client Hooks
- Unit & Integration Test Suites

## God Nodes (most connected - your core abstractions)
1. `cn()` - 235 edges
2. `Result` - 112 edges
3. `NeoWallet.Domain.Common` - 101 edges
4. `NeoWallet.Domain.ValueObjects` - 78 edges
5. `NeoWallet.Application.Common.Interfaces` - 42 edges
6. `NeoWallet.Domain.Enums` - 41 edges
7. `Button()` - 38 edges
8. `Card()` - 35 edges
9. `CardContent()` - 35 edges
10. `WalletId` - 34 edges

## Surprising Connections (you probably didn't know these)
- `CustomWebApplicationFactory` --references--> `Program`  [EXTRACTED]
  tests/NeoWallet.Api.IntegrationTests/Common/CustomWebApplicationFactory.cs → src/NeoWallet.Api/Program.cs
- `IdentityCommandHandlersTests` --references--> `IApiKeyService`  [EXTRACTED]
  tests/NeoWallet.Application.UnitTests/Features/Identity/IdentityCommandHandlersTests.cs → src/NeoWallet.Application/Common/Interfaces/IApiKeyService.cs
- `CustomWebApplicationFactory` --references--> `IAuditStore`  [EXTRACTED]
  tests/NeoWallet.Api.IntegrationTests/Common/CustomWebApplicationFactory.cs → src/NeoWallet.Application/Common/Interfaces/IAuditStore.cs
- `AuditAndReconciliationTests` --references--> `IAuditStore`  [EXTRACTED]
  tests/NeoWallet.Application.UnitTests/Features/AuditAndReconciliationTests.cs → src/NeoWallet.Application/Common/Interfaces/IAuditStore.cs
- `IdentityCommandHandlersTests` --references--> `IJwtProvider`  [EXTRACTED]
  tests/NeoWallet.Application.UnitTests/Features/Identity/IdentityCommandHandlersTests.cs → src/NeoWallet.Application/Common/Interfaces/IJwtProvider.cs

## Import Cycles
- None detected.

## Communities (132 total, 28 thin omitted)

### Community 0 - "Marten Event Sourcing & Projections"
Cohesion: 0.07
Nodes (49): AiInsights(), container, item, SUBCATEGORY_COLORS, chartConfig, MonthComparison(), CYCLE, nextStatus() (+41 more)

### Community 1 - "REST API Endpoints & DTOs"
Cohesion: 0.06
Nodes (51): ControllerBase, CreateApiKeyRequest, CreateWalletRequest, DepositRequest, Disable2FARequest, HttpDelete, IMediator, LoginRequest (+43 more)

### Community 2 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.06
Nodes (49): containerVariants, itemVariants, SignInPage(), containerVariants, itemVariants, SignUpPage(), Checkbox(), Command() (+41 more)

### Community 3 - "MassTransit Distributed Sagas"
Cohesion: 0.05
Nodes (46): IBus, InitiatePaymentRequest, MassTransitStateMachine, SagaStateMachineInstance, CancellationToken, HttpPost, IActionResult, Task (+38 more)

### Community 4 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.05
Nodes (34): chartConfig, FinancialOverview(), monthIndex, EmptyState(), EmptyStateProps, EmptyStateVariant, variants, iconMap (+26 more)

### Community 5 - "Marten Event Sourcing & Projections"
Cohesion: 0.09
Nodes (8): NeoWallet.Domain.Common, NeoWallet.Infrastructure.IntegrationTests.Repositories, NeoWallet.Domain.Events, NeoWallet.Domain.UnitTests.Common, NeoWallet.Domain.ValueObjects, NeoWallet.Domain.Aggregates, NeoWallet.Domain.UnitTests.Aggregates, NeoWallet.Domain.UnitTests.ValueObjects

### Community 6 - "MassTransit Distributed Sagas"
Cohesion: 0.06
Nodes (33): IConsumer, ConsumeContext, Task, CreditWalletAfterPaymentConsumer, CreditWalletAfterPaymentCommand, ConsumeContext, Task, CompensateSourceWalletConsumer (+25 more)

### Community 7 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.05
Nodes (45): AccountCard(), AccountCardProps, fmt(), AccountSummary(), AccountSummaryProps, fmt(), AccountsPageClient(), AccountType (+37 more)

### Community 8 - "REST API Endpoints & DTOs"
Cohesion: 0.05
Nodes (35): ConcurrentDictionary, IPipelineBehavior, CancellationToken, RequestHandlerDelegate, Task, IdempotencyBehavior, CancellationToken, ILogger (+27 more)

### Community 9 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.09
Nodes (35): accountTypes, Step, typeColors, CandlestickData, CandlestickProps, CandlestickTooltip(), chartConfig, CoinInsight() (+27 more)

### Community 10 - "REST API Endpoints & DTOs"
Cohesion: 0.07
Nodes (39): AbstractValidator, IRequest, ICommand, ICommandHandler, IIdempotentCommand, WalletDto, GetAuditTrailQueryValidator, CreateApiKeyCommandValidator (+31 more)

### Community 11 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.09
Nodes (14): DateTime, Guard, DateTime, int, string, TotpProvider, Fact, InlineData (+6 more)

### Community 12 - "Audit Hash Chaining & Reconciliation"
Cohesion: 0.09
Nodes (29): compactNumber(), MarketOverview(), SortDir, SortField, getPl(), HoldingsTable(), SortDir, SortKey (+21 more)

### Community 13 - "Marten Event Sourcing & Projections"
Cohesion: 0.09
Nodes (26): SingleStreamProjection, UserRole, DateTime, Guid, ApiKeyCreated, DateTime, Guid, ApiKeyRevoked (+18 more)

### Community 14 - "REST API Endpoints & DTOs"
Cohesion: 0.08
Nodes (22): NeoWallet.Api.IntegrationTests.Controllers, NeoWallet.Api.IntegrationTests.Common, IClassFixture, IWebHostBuilder, CustomWebApplicationFactory, Fact, HttpClient, Task (+14 more)

### Community 15 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.07
Nodes (30): DAY_LABELS, intensityClass(), SpendingHeatmap(), botResponses, categoryColors, categoryFilters, categoryIcons, ChatMessage (+22 more)

### Community 16 - "Marten Event Sourcing & Projections"
Cohesion: 0.08
Nodes (16): NeoWallet.Infrastructure.Authentication.Options, NeoWallet.Application.Features.Wallets.Queries.GetWalletSummary, NeoWallet.Application.Common.Interfaces, NeoWallet.Infrastructure.Services, NeoWallet.Application.Features.Wallets.Queries.GetTransactionHistory, NeoWallet.Application.DTOs.Wallet, NeoWallet.Infrastructure.Authentication, NeoWallet.Infrastructure.IntegrationTests.Security (+8 more)

### Community 17 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.09
Nodes (26): CategoryDonut(), categoryColors, RecentTransactions(), TransactionActions(), TransactionActionsProps, TransactionFilters(), fmt(), TransactionSummary() (+18 more)

### Community 18 - "Marten Event Sourcing & Projections"
Cohesion: 0.13
Nodes (14): NeoWallet.Domain.Repositories, NeoWallet.Application.Features.Identity.Commands.RefreshToken, NeoWallet.Application.Features.Identity.Commands.VerifyTwoFactor, NeoWallet.Application.Features.Identity.Commands.EnableTwoFactor, NeoWallet.Application.Features.Identity.Commands.RevokeApiKey, NeoWallet.Application.UnitTests.Features.Identity, NeoWallet.Application.Features.Identity.Commands.RegisterUser, NeoWallet.Application.Features.Identity.Commands.CreateApiKey (+6 more)

### Community 19 - "Domain Model & Invariants"
Cohesion: 0.18
Nodes (4): DateTime, Wallet, Result, Money

### Community 20 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.10
Nodes (25): COLORS, QuickTransfer(), SendState, QuickSend(), SendState, Avatar(), AvatarBadge(), AvatarFallback() (+17 more)

### Community 21 - "Marten Event Sourcing & Projections"
Cohesion: 0.14
Nodes (12): Guid, CancellationToken, Guid, IDocumentSession, ILogger, Task, MartenAggregateRepository, CancellationToken (+4 more)

### Community 22 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.09
Nodes (17): NeoWallet.Benchmarks, GlobalSetup, HashAlgorithmName, int, PasswordHasher, Benchmark, Guid, string (+9 more)

### Community 23 - "Marten Event Sourcing & Projections"
Cohesion: 0.12
Nodes (18): WalletStatus, DateTime, Guid, WalletCreated, DateTime, Guid, WalletLocked, DateTime (+10 more)

### Community 24 - "Shadcn UI Components & Primitives"
Cohesion: 0.11
Nodes (28): data, NavMain(), NavSecondary(), NavUser(), Sidebar(), SidebarContent(), SidebarContext, SidebarContextProps (+20 more)

### Community 25 - "Marten Event Sourcing & Projections"
Cohesion: 0.13
Nodes (23): IQuerySession, CancellationToken, Guid, IReadOnlyList, Task, IWalletReadService, TransactionHistoryDto, WalletSummaryDto (+15 more)

### Community 26 - "REST API Endpoints & DTOs"
Cohesion: 0.11
Nodes (21): ClaimsPrincipal, IJwtProvider, IPasswordHasher, AuthResponseDto, CancellationToken, Task, LoginCommand, LoginCommandHandler (+13 more)

### Community 27 - "Domain Model & Invariants"
Cohesion: 0.11
Nodes (16): DateTime, Payment, PaymentGatewayProvider, DateTime, Guid, PaymentInitiated, DateTime, Guid (+8 more)

### Community 28 - "Domain Model & Invariants"
Cohesion: 0.08
Nodes (19): IEnumerable, IReadOnlyCollection, List, AggregateRoot, DateTime, Guid, IDomainEvent, DateTime (+11 more)

### Community 29 - "Frontend React Query & Client Hooks"
Cohesion: 0.07
Nodes (28): compilerOptions, allowJs, esModuleInterop, incremental, isolatedModules, jsx, lib, module (+20 more)

### Community 31 - "Marten Event Sourcing & Projections"
Cohesion: 0.12
Nodes (12): NeoWallet.Infrastructure.ReadModels, NeoWallet.Infrastructure.IntegrationTests.Projections, NeoWallet.Infrastructure.Reconciliation, NeoWallet.Infrastructure.Projections, NeoWallet.Domain.Enums, UserDto, PaymentStatus, TransactionType (+4 more)

### Community 33 - "3D Interactive Globe Visualization"
Cohesion: 0.08
Nodes (25): eslint, eslint-config-next, devDependencies, eslint, eslint-config-next, shadcn, tailwindcss, @tailwindcss/postcss (+17 more)

### Community 34 - "Domain Model & Invariants"
Cohesion: 0.12
Nodes (16): IComparable, DateTime, Guid, MoneyDeposited, DateTime, Guid, MoneyTransferredIn, DateTime (+8 more)

### Community 35 - "REST API Endpoints & DTOs"
Cohesion: 0.19
Nodes (9): CancellationToken, Task, Fact, Task, IdentityCommandHandlersTests, Fact, InlineData, Theory (+1 more)

### Community 36 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.14
Nodes (18): CardControlsProps, CardList(), CardListProps, formatCurrency(), CardsPageClient(), InteractiveCard(), InteractiveCardProps, randomDigits() (+10 more)

### Community 37 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.10
Nodes (18): Block, DashboardCustomizer(), defaultBlocks, sizeClass, SortableWidget(), WidgetSize, describeArc(), FactorDetail() (+10 more)

### Community 39 - "3D Interactive Globe Visualization"
Cohesion: 0.09
Nodes (23): axios, clsx, cmdk, @dnd-kit/core, dependencies, axios, clsx, cmdk (+15 more)

### Community 40 - "Shadcn UI Components & Primitives"
Cohesion: 0.09
Nodes (21): aliases, components, hooks, lib, ui, utils, iconLibrary, menuAccent (+13 more)

### Community 41 - "Marten Event Sourcing & Projections"
Cohesion: 0.29
Nodes (6): TransferService, Fact, TransferServiceTests, Fact, Task, MartenIntegrationTests

### Community 42 - "REST API Endpoints & DTOs"
Cohesion: 0.14
Nodes (16): CancellationToken, DateTime, Task, IDiscrepancyNotifier, IReconciliationService, ReconciliationReportDto, CancellationToken, Task (+8 more)

### Community 43 - "REST API Endpoints & DTOs"
Cohesion: 0.16
Nodes (11): CancellationToken, Task, CancellationToken, Task, TransferMoneyCommandHandler, CancellationToken, Task, ITransferService (+3 more)

### Community 44 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.16
Nodes (9): Func, Error, ErrorType, Audit, Concurrency, DomainErrors, Identity, Transaction (+1 more)

### Community 45 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.14
Nodes (12): DateTime, ITotpProvider, CancellationToken, Task, DisableTwoFactorCommand, DisableTwoFactorCommandHandler, DisableTwoFactorCommandValidator, DateTime (+4 more)

### Community 46 - "Domain Model & Invariants"
Cohesion: 0.19
Nodes (13): DateTime, Guid, IReadOnlyCollection, IReadOnlyList, List, User, DateTime, Guid (+5 more)

### Community 47 - "Audit Hash Chaining & Reconciliation"
Cohesion: 0.15
Nodes (13): DateTime, Guid, IEnumerable, IReadOnlyList, List, ReconciliationReport, CancellationToken, Task (+5 more)

### Community 48 - "Marten Event Sourcing & Projections"
Cohesion: 0.21
Nodes (10): IWalletRepository, CancellationToken, IDocumentSession, ILogger, IReadOnlyList, Task, MartenWalletRepository, Fact (+2 more)

### Community 49 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.12
Nodes (13): GlobeDemo, GlobeDemo, colors, globeConfig, sampleArcs, World, Globe, GlobeConfig (+5 more)

### Community 50 - "REST API Endpoints & DTOs"
Cohesion: 0.23
Nodes (12): WALLET_KEYS, walletApi, CreateWalletRequest, DepositRequest, DiscrepancyDto, LockWalletRequest, TransactionType, TransferRequest (+4 more)

### Community 51 - "REST API Endpoints & DTOs"
Cohesion: 0.15
Nodes (10): JwtSecurityTokenHandler, ClaimsPrincipal, JwtProvider, string, JwtSettings, SymmetricSecurityKey, Fact, InlineData (+2 more)

### Community 52 - "MassTransit Distributed Sagas"
Cohesion: 0.16
Nodes (7): NeoWallet.Application.Features.Sagas.Payment.Contracts, NeoWallet.Infrastructure.IntegrationTests.Gateways, NeoWallet.Application.DTOs.Payment, NeoWallet.Application.Features.Sagas.Payment.Consumers, NeoWallet.Infrastructure.Gateways, NeoWallet.ArchitectureTests, NeoWallet.Api.Common

### Community 53 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.20
Nodes (9): KeyHash, PlainTextKey, Prefix, string, ApiKeyService, Fact, InlineData, Theory (+1 more)

### Community 54 - "REST API Endpoints & DTOs"
Cohesion: 0.15
Nodes (7): NeoWallet.Application.UnitTests.Common.Behaviors, NeoWallet.Application.Common.Behaviors, CancellationToken, ILogger, RequestHandlerDelegate, Task, LoggingBehavior

### Community 55 - "MassTransit Distributed Sagas"
Cohesion: 0.18
Nodes (5): NeoWallet.Application.UnitTests.Sagas, NeoWallet.Application.Features.Sagas.Transfer.Contracts, NeoWallet.Application.Features.Sagas.Transfer, NeoWallet.Application.Features.Sagas.Payment, NeoWallet.Application.Features.Sagas.Transfer.Consumers

### Community 56 - "Dashboard & Interactive Wallet Cards"
Cohesion: 0.18
Nodes (9): AppSidebar(), CommandPalette(), DynamicBreadcrumb(), emptySubscribe(), ThemeToggle(), useIsMounted(), SidebarInset(), SidebarProvider() (+1 more)

### Community 57 - "Domain Model & Invariants"
Cohesion: 0.24
Nodes (7): IEquatable, Entity, Fact, Guid, AnotherEntity, EntityAndAggregateRootTests, TestEntity

### Community 58 - "REST API Endpoints & DTOs"
Cohesion: 0.14
Nodes (13): Microsoft.AspNetCore.Authentication.JwtBearer (8.0.13), Microsoft.AspNetCore.OpenApi (8.0.13), OpenTelemetry.Exporter.OpenTelemetryProtocol (1.11.1), OpenTelemetry.Exporter.Prometheus.AspNetCore (1.11.0-beta.1), OpenTelemetry.Extensions.Hosting (1.11.1), OpenTelemetry.Instrumentation.AspNetCore (1.11.0), OpenTelemetry.Instrumentation.Http (1.11.0), Serilog.AspNetCore (8.0.3) (+5 more)

### Community 59 - "REST API Endpoints & DTOs"
Cohesion: 0.19
Nodes (9): KeyHash, PlainTextKey, Prefix, IApiKeyService, ApiKeyDto, CancellationToken, Task, CreateApiKeyCommand (+1 more)

### Community 60 - "Marten Event Sourcing & Projections"
Cohesion: 0.17
Nodes (4): NeoWallet.Domain.Entities, NeoWallet.Infrastructure.Audit, NeoWallet.Domain.UnitTests.Entities, NeoWallet.Infrastructure.Notifications

### Community 61 - "REST API Endpoints & DTOs"
Cohesion: 0.27
Nodes (8): AuthContext, AuthContextType, AuthProvider(), failedQueue, authService, AuthResultDto, ProblemDetails, UserDto

### Community 62 - "Audit Hash Chaining & Reconciliation"
Cohesion: 0.24
Nodes (9): CancellationToken, Guid, IReadOnlyList, Task, IAuditStore, CancellationToken, Task, VerifyAuditChainQuery (+1 more)

### Community 63 - "Audit Hash Chaining & Reconciliation"
Cohesion: 0.31
Nodes (6): DateTime, Guid, string, AuditLogEntry, Fact, AuditLogEntryTests

### Community 64 - "REST API Endpoints & DTOs"
Cohesion: 0.24
Nodes (9): NeoWallet.Application.Features.Wallets.Commands.UnlockWallet, NeoWallet.Application.Features.Wallets.Commands.DepositMoney, NeoWallet.Application.Features.Wallets.Commands.CreateWallet, NeoWallet.Application.Features.Wallets.Commands.TransferMoney, NeoWallet.Application.Features.Wallets.Commands.WithdrawMoney, NeoWallet.Application.UnitTests.Features.Wallets, NeoWallet.Domain.UnitTests.Services, NeoWallet.Application.Features.Wallets.Commands.LockWallet (+1 more)

### Community 65 - "REST API Endpoints & DTOs"
Cohesion: 0.21
Nodes (6): NeoWallet.Application.Features.Reconciliation.Commands.RunReconciliation, NeoWallet.Application.Features.Audit.Queries.GetAuditTrail, NeoWallet.Application.Features.Audit.Queries.VerifyAuditChain, NeoWallet.Application.UnitTests.Features, NeoWallet.Application.DTOs.Audit, NeoWallet.Api.Controllers

### Community 66 - "REST API Endpoints & DTOs"
Cohesion: 0.30
Nodes (3): BalanceChangedEvent, WalletHubService, TransactionHistoryDto

### Community 68 - "Domain Model & Invariants"
Cohesion: 0.27
Nodes (6): CancellationToken, Task, IUserRepository, GeneratedRegex, Regex, Email

### Community 69 - "NeoWallet.Api.Extensions Module"
Cohesion: 0.18
Nodes (7): NeoWallet.Api.Extensions, IConfiguration, IServiceCollection, string, OpenTelemetryExtensions, IServiceCollection, SwaggerExtensions

### Community 70 - "REST API Endpoints & DTOs"
Cohesion: 0.25
Nodes (9): ApiKeysTab(), API_KEY_KEYS, useApiKeys(), useCreateApiKey(), useDeleteApiKey(), authApi, ApiKeyDto, CreateApiKeyRequest (+1 more)

### Community 71 - "Shadcn UI Components & Primitives"
Cohesion: 0.18
Nodes (7): Sheet(), SheetContent(), SheetDescription(), SheetFooter(), SheetHeader(), SheetOverlay(), SheetTitle()

### Community 72 - "REST API Endpoints & DTOs"
Cohesion: 0.29
Nodes (9): IRequestHandler, IQuery, IQueryHandler, AuditEntryDto, CancellationToken, IReadOnlyList, Task, GetAuditTrailQuery (+1 more)

### Community 73 - "MassTransit Distributed Sagas"
Cohesion: 0.18
Nodes (10): Microsoft.Extensions.DependencyInjection (10.0.11), net8.0, coverlet.collector (6.0.4), FluentAssertions (8.1.1), MassTransit (8.3.6), Microsoft.NET.Test.Sdk (17.13.0), NSubstitute (6.2.0), xunit (2.9.3) (+2 more)

### Community 74 - "Authentication & Security (JWT/2FA)"
Cohesion: 0.33
Nodes (4): Fact, InlineData, Theory, TotpSecretTests

### Community 75 - "REST API Endpoints & DTOs"
Cohesion: 0.24
Nodes (6): NeoWallet.Api.Middlewares, HttpContext, RequestDelegate, string, Task, CorrelationIdMiddleware

### Community 76 - "Marten Event Sourcing & Projections"
Cohesion: 0.36
Nodes (5): EventProjection, IDocumentOperations, TransactionHistoryProjection, Fact, TransactionHistoryProjectionTests

### Community 77 - "Shadcn UI Components & Primitives"
Cohesion: 0.33
Nodes (8): labelMap, Breadcrumb(), BreadcrumbEllipsis(), BreadcrumbItem(), BreadcrumbLink(), BreadcrumbList(), BreadcrumbPage(), BreadcrumbSeparator()

### Community 78 - "MassTransit Distributed Sagas"
Cohesion: 0.20
Nodes (9): FluentValidation (11.11.0), FluentValidation.DependencyInjectionExtensions (11.11.0), MediatR (12.4.1), Microsoft.Extensions.Hosting.Abstractions (10.0.11), net8.0, MassTransit (8.3.6), Microsoft.Extensions.DependencyInjection.Abstractions (10.0.11), Microsoft.Extensions.Logging.Abstractions (10.0.11) (+1 more)

### Community 79 - "MassTransit Distributed Sagas"
Cohesion: 0.20
Nodes (9): Marten (8.32.1), Microsoft.Extensions.Configuration.Abstractions (10.0.11), Microsoft.Extensions.Options.ConfigurationExtensions (10.0.11), System.IdentityModel.Tokens.Jwt (8.22.0), net8.0, MassTransit (8.3.6), Microsoft.Extensions.DependencyInjection.Abstractions (10.0.11), Microsoft.Extensions.Logging.Abstractions (10.0.11) (+1 more)

### Community 80 - "Unit & Integration Test Suites"
Cohesion: 0.20
Nodes (9): Testcontainers.PostgreSql (4.14.0), net8.0, coverlet.collector (6.0.0), FluentAssertions (8.10.0), Microsoft.NET.Test.Sdk (17.8.0), NSubstitute (6.2.0), xunit (2.5.3), xunit.runner.visualstudio (2.5.3) (+1 more)

### Community 81 - "Marten Event Sourcing & Projections"
Cohesion: 0.31
Nodes (7): CancellationToken, Guid, IDocumentSession, ILogger, IReadOnlyList, Task, MartenAuditStore

### Community 82 - "Audit Hash Chaining & Reconciliation"
Cohesion: 0.31
Nodes (7): BackgroundService, IServiceScopeFactory, CancellationToken, ILogger, Task, TimeSpan, ReconciliationWorker

### Community 83 - "Audit Hash Chaining & Reconciliation"
Cohesion: 0.25
Nodes (5): NeoWallet.Api.Hubs, NeoWallet.Application.Features.Reconciliation.Workers, NeoWallet.Api.Services, NeoWallet.Infrastructure, Program

### Community 84 - "Unit & Integration Test Suites"
Cohesion: 0.25
Nodes (6): NeoWallet.Infrastructure.IntegrationTests.Fixtures, IAsyncLifetime, IServiceProvider, PostgreSqlContainer, Task, PostgreSqlFixture

### Community 85 - "Shadcn UI Components & Primitives"
Cohesion: 0.22
Nodes (8): name, private, scripts, build, dev, lint, start, version

### Community 86 - "REST API Endpoints & DTOs"
Cohesion: 0.25
Nodes (5): AUDIT_KEYS, auditApi, apiClient, AuditVerificationResultDto, ReconciliationReportDto

### Community 87 - "REST API Endpoints & DTOs"
Cohesion: 0.31
Nodes (5): paymentApi, PaymentInitiatedResponse, PaymentInitiateRequest, PaymentVerificationResponse, PaymentVerifyRequest

### Community 88 - "Unit & Integration Test Suites"
Cohesion: 0.22
Nodes (8): Microsoft.AspNetCore.Mvc.Testing (8.0.13), net8.0, FluentAssertions (7.2.0), Microsoft.NET.Test.Sdk (17.13.0), NSubstitute (5.3.0), xunit (2.9.3), xunit.runner.visualstudio (3.0.2), Microsoft.NET.Sdk

### Community 89 - "REST API Endpoints & DTOs"
Cohesion: 0.39
Nodes (3): Assembly, Fact, ArchitectureTests

### Community 90 - "REST API Endpoints & DTOs"
Cohesion: 0.36
Nodes (6): Exception, HttpContext, ILogger, RequestDelegate, Task, ExceptionHandlingMiddleware

### Community 91 - "SignalR Real-time Event Streaming"
Cohesion: 0.29
Nodes (5): geistMono, geistSans, metadata, QueryProvider(), SignalRProvider()

### Community 92 - "Unit & Integration Test Suites"
Cohesion: 0.29
Nodes (5): BenchmarkDotNet (0.14.0), net8.0, Microsoft.NET.Sdk, net8.0, Microsoft.NET.Sdk

### Community 93 - "Unit & Integration Test Suites"
Cohesion: 0.25
Nodes (7): NetArchTest.eNhancedEdition (1.4.3), net8.0, FluentAssertions (7.2.0), Microsoft.NET.Test.Sdk (17.13.0), xunit (2.9.3), xunit.runner.visualstudio (3.0.2), Microsoft.NET.Sdk

### Community 94 - "Unit & Integration Test Suites"
Cohesion: 0.25
Nodes (7): net8.0, coverlet.collector (6.0.0), FluentAssertions (8.10.0), Microsoft.NET.Test.Sdk (17.8.0), xunit (2.5.3), xunit.runner.visualstudio (2.5.3), Microsoft.NET.Sdk

### Community 95 - "Domain Model & Invariants"
Cohesion: 0.38
Nodes (4): Dictionary, GeneratedRegex, Regex, Currency

### Community 96 - "SignalR Real-time Event Streaming"
Cohesion: 0.43
Nodes (5): IHubContext, CancellationToken, Guid, Task, SignalRNotificationService

### Community 97 - "Transactions & Ledger History UI"
Cohesion: 0.43
Nodes (4): CancellationToken, Guid, Task, IWalletNotificationService

### Community 98 - "REST API Endpoints & DTOs"
Cohesion: 0.43
Nodes (6): TwoFactorSetupDto, CancellationToken, Task, EnableTwoFactorCommand, EnableTwoFactorCommandHandler, EnableTwoFactorCommandValidator

### Community 99 - "Domain Model & Invariants"
Cohesion: 0.60
Nodes (3): CancellationToken, Task, IAggregateRepository

### Community 100 - "REST API Endpoints & DTOs"
Cohesion: 0.60
Nodes (3): Fact, Task, AuditAndReconciliationTests

### Community 101 - "MassTransit Distributed Sagas"
Cohesion: 0.60
Nodes (3): Fact, Task, TransferStateMachineTests

### Community 102 - "NeoWallet.Application Module"
Cohesion: 0.40
Nodes (3): NeoWallet.Application, IServiceCollection, DependencyInjection

### Community 103 - "SignalR Real-time Event Streaming"
Cohesion: 0.50
Nodes (3): Hub, Task, WalletHub

### Community 104 - "MassTransit Distributed Sagas"
Cohesion: 0.60
Nodes (3): Fact, Task, DepositPaymentStateMachineTests

## Knowledge Gaps
- **329 isolated node(s):** `$schema`, `style`, `rsc`, `tsx`, `config` (+324 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **28 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Result` connect `Domain Model & Invariants` to `REST API Endpoints & DTOs`, `MassTransit Distributed Sagas`, `MassTransit Distributed Sagas`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `Marten Event Sourcing & Projections`, `Marten Event Sourcing & Projections`, `REST API Endpoints & DTOs`, `Domain Model & Invariants`, `Domain Model & Invariants`, `REST API Endpoints & DTOs`, `Domain Model & Invariants`, `REST API Endpoints & DTOs`, `Domain Model & Invariants`, `Marten Event Sourcing & Projections`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `Authentication & Security (JWT/2FA)`, `Authentication & Security (JWT/2FA)`, `Domain Model & Invariants`, `Audit Hash Chaining & Reconciliation`, `Marten Event Sourcing & Projections`, `REST API Endpoints & DTOs`, `Audit Hash Chaining & Reconciliation`, `Audit Hash Chaining & Reconciliation`, `REST API Endpoints & DTOs`, `Domain Model & Invariants`, `REST API Endpoints & DTOs`, `Authentication & Security (JWT/2FA)`, `Marten Event Sourcing & Projections`, `REST API Endpoints & DTOs`, `Domain Model & Invariants`?**
  _High betweenness centrality (0.112) - this node is a cross-community bridge._
- **Why does `NeoWallet.Domain.Common` connect `Marten Event Sourcing & Projections` to `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `Authentication & Security (JWT/2FA)`, `Authentication & Security (JWT/2FA)`, `Domain Model & Invariants`, `REST API Endpoints & DTOs`, `Marten Event Sourcing & Projections`, `Marten Event Sourcing & Projections`, `Domain Model & Invariants`, `MassTransit Distributed Sagas`, `REST API Endpoints & DTOs`, `Domain Model & Invariants`, `Marten Event Sourcing & Projections`, `Marten Event Sourcing & Projections`?**
  _High betweenness centrality (0.068) - this node is a cross-community bridge._
- **Why does `NeoWallet.Application.Common.Interfaces` connect `Marten Event Sourcing & Projections` to `Transactions & Ledger History UI`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `Marten Event Sourcing & Projections`, `Audit Hash Chaining & Reconciliation`, `MassTransit Distributed Sagas`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `REST API Endpoints & DTOs`, `Marten Event Sourcing & Projections`, `Marten Event Sourcing & Projections`?**
  _High betweenness centrality (0.056) - this node is a cross-community bridge._
- **What connects `$schema`, `style`, `rsc` to the rest of the system?**
  _329 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Marten Event Sourcing & Projections` be split into smaller, more focused modules?**
  _Cohesion score 0.06558558558558558 - nodes in this community are weakly interconnected._
- **Should `REST API Endpoints & DTOs` be split into smaller, more focused modules?**
  _Cohesion score 0.06259780907668232 - nodes in this community are weakly interconnected._
- **Should `Authentication & Security (JWT/2FA)` be split into smaller, more focused modules?**
  _Cohesion score 0.06349206349206349 - nodes in this community are weakly interconnected._