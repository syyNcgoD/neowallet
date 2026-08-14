using FluentAssertions;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.Persistence.Repositories;

namespace NeoWallet.Infrastructure.IntegrationTests.Repositories;

public sealed class MartenWalletRepositoryUnitTests
{
    [Fact]
    public async Task StoreAsync_WithNullWallet_ShouldReturnValidationFailure()
    {
        // Arrange
        var repo = new MartenWalletRepository(null!, NullLogger<MartenWalletRepository>.Instance);

        // Act
        var result = await repo.StoreAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.Null");
    }

    [Fact]
    public async Task StoreAsync_WithNoUncommittedEvents_ShouldReturnSuccessImmediately()
    {
        // Arrange
        var repo = new MartenWalletRepository(null!, NullLogger<MartenWalletRepository>.Instance);
        var wallet = Wallet.Create(WalletId.New(), OwnerId.New(), Currency.USD).Value;
        wallet.ClearUncommittedEvents();

        // Act
        var result = await repo.StoreAsync(wallet);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_WithEmptyWalletId_ShouldReturnValidationFailure()
    {
        // Arrange
        var repo = new MartenWalletRepository(null!, NullLogger<MartenWalletRepository>.Instance);

        // Act
        var result = await repo.LoadAsync(WalletId.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WalletId.Empty");
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyWalletId_ShouldReturnValidationFailure()
    {
        // Arrange
        var repo = new MartenWalletRepository(null!, NullLogger<MartenWalletRepository>.Instance);

        // Act
        var result = await repo.ExistsAsync(WalletId.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WalletId.Empty");
    }

    [Fact]
    public async Task GetEventStreamAsync_WithEmptyWalletId_ShouldReturnValidationFailure()
    {
        // Arrange
        var repo = new MartenWalletRepository(null!, NullLogger<MartenWalletRepository>.Instance);

        // Act
        var result = await repo.GetEventStreamAsync(WalletId.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WalletId.Empty");
    }
}
