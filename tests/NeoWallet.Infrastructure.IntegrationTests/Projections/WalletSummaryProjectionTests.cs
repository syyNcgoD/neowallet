using FluentAssertions;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.Projections;

namespace NeoWallet.Infrastructure.IntegrationTests.Projections;

public sealed class WalletSummaryProjectionTests
{
    private readonly WalletId _walletId = WalletId.New();
    private readonly OwnerId _ownerId = OwnerId.New();
    private readonly Currency _currency = Currency.USD;

    [Fact]
    public void Create_FromWalletCreated_ShouldInitializeSummary()
    {
        // Arrange
        var @event = WalletCreated.Create(_walletId, _ownerId, _currency);

        // Act
        var summary = WalletSummaryProjection.Create(@event);

        // Assert
        summary.Id.Should().Be(_walletId.Value);
        summary.OwnerId.Should().Be(_ownerId.Value);
        summary.Currency.Should().Be("USD");
        summary.Balance.Should().Be(0m);
        summary.Status.Should().Be(WalletStatus.Active);
        summary.Version.Should().Be(0);
        summary.CreatedAtUtc.Should().Be(@event.CreatedAtUtc);
    }

    [Fact]
    public void Apply_MoneyDeposited_ShouldUpdateBalanceAndVersion()
    {
        // Arrange
        var summary = WalletSummaryProjection.Create(WalletCreated.Create(_walletId, _ownerId, _currency));
        var depositEvent = MoneyDeposited.Create(
            _walletId,
            TransactionId.New(),
            Money.Create(150m, _currency).Value,
            Money.Create(150m, _currency).Value,
            "REF-01",
            "Initial deposit");

        // Act
        WalletSummaryProjection.Apply(depositEvent, summary);

        // Assert
        summary.Balance.Should().Be(150m);
        summary.Version.Should().Be(1);
        summary.LastModifiedAtUtc.Should().Be(depositEvent.DepositedAtUtc);
    }

    [Fact]
    public void Apply_MoneyWithdrawn_ShouldUpdateBalanceAndVersion()
    {
        // Arrange
        var summary = WalletSummaryProjection.Create(WalletCreated.Create(_walletId, _ownerId, _currency));
        summary.Balance = 300m;
        var withdrawEvent = MoneyWithdrawn.Create(
            _walletId,
            TransactionId.New(),
            Money.Create(100m, _currency).Value,
            Money.Create(200m, _currency).Value,
            "ATM-01",
            "Cash out");

        // Act
        WalletSummaryProjection.Apply(withdrawEvent, summary);

        // Assert
        summary.Balance.Should().Be(200m);
        summary.Version.Should().Be(1);
        summary.LastModifiedAtUtc.Should().Be(withdrawEvent.WithdrawnAtUtc);
    }

    [Fact]
    public void Apply_MoneyTransferredOut_ShouldUpdateBalanceAndVersion()
    {
        // Arrange
        var summary = WalletSummaryProjection.Create(WalletCreated.Create(_walletId, _ownerId, _currency));
        summary.Balance = 500m;
        var transferOut = MoneyTransferredOut.Create(
            _walletId,
            WalletId.New(),
            TransactionId.New(),
            Money.Create(200m, _currency).Value,
            Money.Create(300m, _currency).Value,
            "TRF-OUT");

        // Act
        WalletSummaryProjection.Apply(transferOut, summary);

        // Assert
        summary.Balance.Should().Be(300m);
        summary.Version.Should().Be(1);
        summary.LastModifiedAtUtc.Should().Be(transferOut.TransferredAtUtc);
    }

    [Fact]
    public void Apply_MoneyTransferredIn_ShouldUpdateBalanceAndVersion()
    {
        // Arrange
        var summary = WalletSummaryProjection.Create(WalletCreated.Create(_walletId, _ownerId, _currency));
        var transferIn = MoneyTransferredIn.Create(
            _walletId,
            WalletId.New(),
            TransactionId.New(),
            Money.Create(450m, _currency).Value,
            Money.Create(450m, _currency).Value,
            "TRF-IN");

        // Act
        WalletSummaryProjection.Apply(transferIn, summary);

        // Assert
        summary.Balance.Should().Be(450m);
        summary.Version.Should().Be(1);
        summary.LastModifiedAtUtc.Should().Be(transferIn.TransferredAtUtc);
    }

    [Fact]
    public void Apply_WalletLockedAndUnlocked_ShouldUpdateStatusAndVersion()
    {
        // Arrange
        var summary = WalletSummaryProjection.Create(WalletCreated.Create(_walletId, _ownerId, _currency));
        var lockEvent = WalletLocked.Create(_walletId, "Audit lock");
        var unlockEvent = WalletUnlocked.Create(_walletId, "Audit clear");

        // Act & Assert Lock
        WalletSummaryProjection.Apply(lockEvent, summary);
        summary.Status.Should().Be(WalletStatus.Locked);
        summary.Version.Should().Be(1);

        // Act & Assert Unlock
        WalletSummaryProjection.Apply(unlockEvent, summary);
        summary.Status.Should().Be(WalletStatus.Active);
        summary.Version.Should().Be(2);
    }
}
