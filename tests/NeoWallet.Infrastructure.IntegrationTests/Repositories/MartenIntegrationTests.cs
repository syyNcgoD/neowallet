using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.IntegrationTests.Fixtures;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.IntegrationTests.Repositories;

public sealed class MartenIntegrationTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;

    public MartenIntegrationTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task StoreAndLoad_WithNewWallet_ShouldPersistAndRehydrateSuccessfully()
    {
        if (!_fixture.IsAvailable)
        {
            return; // Skip when Docker engine is not running locally
        }

        using var scope = _fixture.ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWalletRepository>();

        // Arrange
        var walletId = WalletId.New();
        var ownerId = OwnerId.New();
        var currency = Currency.USD;
        var wallet = Wallet.Create(walletId, ownerId, currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(500m, currency).Value, "DEP-01", "Initial deposit");

        // Act - Store
        var storeResult = await repository.StoreAsync(wallet);
        storeResult.IsSuccess.Should().BeTrue();

        // Act - Load
        var loadResult = await repository.LoadAsync(walletId);

        // Assert
        loadResult.IsSuccess.Should().BeTrue(loadResult.Error?.Description ?? "no error");
        var loadedWallet = loadResult.Value;
        loadedWallet.Id.Should().Be(walletId);
        loadedWallet.OwnerId.Should().Be(ownerId);
        loadedWallet.Currency.Should().Be(currency);
        loadedWallet.Balance.Amount.Should().Be(500m);
        loadedWallet.Version.Should().Be(1);
    }

    [Fact]
    public async Task OptimisticConcurrency_WhenSavingWithStaleVersion_ShouldReturnConflictError()
    {
        if (!_fixture.IsAvailable)
        {
            return;
        }

        // Arrange: Create and save initial wallet in setup scope
        var walletId = WalletId.New();
        var ownerId = OwnerId.New();
        var currency = Currency.USD;

        using (var initScope = _fixture.ServiceProvider.CreateScope())
        {
            var initRepo = initScope.ServiceProvider.GetRequiredService<IWalletRepository>();
            var initialWallet = Wallet.Create(walletId, ownerId, currency).Value;
            initialWallet.Deposit(TransactionId.New(), Money.Create(1000m, currency).Value);
            var initStore = await initRepo.StoreAsync(initialWallet);
            initStore.IsSuccess.Should().BeTrue();
        }

        using var scope1 = _fixture.ServiceProvider.CreateScope();
        using var scope2 = _fixture.ServiceProvider.CreateScope();
        var repo1 = scope1.ServiceProvider.GetRequiredService<IWalletRepository>();
        var repo2 = scope2.ServiceProvider.GetRequiredService<IWalletRepository>();

        // Act: Load same wallet in two concurrent scopes
        var user1Wallet = (await repo1.LoadAsync(walletId)).Value;
        var user2Wallet = (await repo2.LoadAsync(walletId)).Value;

        // User 1 withdraws and saves successfully
        user1Wallet.Withdraw(TransactionId.New(), Money.Create(200m, currency).Value);
        var user1SaveResult = await repo1.StoreAsync(user1Wallet);
        user1SaveResult.IsSuccess.Should().BeTrue(user1SaveResult.Error?.Description ?? "no error");

        // User 2 tries to withdraw from stale state and save -> should trigger OCC conflict!
        user2Wallet.Withdraw(TransactionId.New(), Money.Create(300m, currency).Value);
        var user2SaveResult = await repo2.StoreAsync(user2Wallet);

        // Assert
        user2SaveResult.IsFailure.Should().BeTrue();
        user2SaveResult.Error.Code.Should().Be(DomainErrors.Concurrency.Conflict.Code);
    }

    [Fact]
    public async Task InlineProjections_ShouldUpdateReadModelsSynchronously()
    {
        if (!_fixture.IsAvailable)
        {
            return;
        }

        using var scope = _fixture.ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IWalletRepository>();
        var session = scope.ServiceProvider.GetRequiredService<IQuerySession>();

        // Arrange
        var walletId = WalletId.New();
        var ownerId = OwnerId.New();
        var currency = Currency.USD;
        var wallet = Wallet.Create(walletId, ownerId, currency).Value;
        var txId = TransactionId.New();
        wallet.Deposit(txId, Money.Create(750m, currency).Value, "DEP-PROJ", "Projection test deposit");

        // Act
        await repository.StoreAsync(wallet);

        // Assert: Query read model directly
        var summary = await session.LoadAsync<WalletSummary>(walletId.Value);
        summary.Should().NotBeNull();
        summary!.Balance.Should().Be(750m);
        summary.Currency.Should().Be("USD");
        summary.OwnerId.Should().Be(ownerId.Value);

        var history = await session.LoadAsync<TransactionHistory>(txId.Value);
        history.Should().NotBeNull();
        history!.WalletId.Should().Be(walletId.Value);
        history.Amount.Should().Be(750m);
        history.Reference.Should().Be("DEP-PROJ");
    }
}
