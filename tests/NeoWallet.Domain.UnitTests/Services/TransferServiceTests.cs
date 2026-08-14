using FluentAssertions;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Services;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.Services;

public sealed class TransferServiceTests
{
    private readonly TransferService _sut = new();
    private readonly Currency _currency = Currency.USD;

    [Fact]
    public void Transfer_WithValidParameters_ShouldDebitSourceAndCreditTarget()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(1000m, _currency).Value);

        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        targetWallet.Deposit(TransactionId.New(), Money.Create(100m, _currency).Value);

        var txId = TransactionId.New();
        var transferAmount = Money.Create(400m, _currency).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, txId, transferAmount, "P2P-REF");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sourceWallet.Balance.Amount.Should().Be(600m);
        targetWallet.Balance.Amount.Should().Be(500m);
    }

    [Fact]
    public void Transfer_WithNullSourceWallet_ShouldReturnFailure()
    {
        // Arrange
        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = _sut.Transfer(null!, targetWallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transfer.NullSourceWallet");
    }

    [Fact]
    public void Transfer_WithNullTargetWallet_ShouldReturnFailure()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, null!, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transfer.NullTargetWallet");
    }

    [Fact]
    public void Transfer_WithSameSourceAndTarget_ShouldReturnSameSourceAndTargetError()
    {
        // Arrange
        var wallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(500m, _currency).Value);
        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = _sut.Transfer(wallet, wallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.SameSourceAndTarget);
    }

    [Fact]
    public void Transfer_WhenSourceWalletIsLocked_ShouldReturnIsLockedError()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(500m, _currency).Value);
        sourceWallet.Lock("Frozen account");

        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.IsLocked);
    }

    [Fact]
    public void Transfer_WhenTargetWalletIsLocked_ShouldReturnTargetWalletLockedError()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(500m, _currency).Value);

        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        targetWallet.Lock("Frozen receiver");

        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.TargetWalletLocked);
    }

    [Fact]
    public void Transfer_WithInvalidAmount_ShouldReturnInvalidAmountError()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, TransactionId.New(), Money.Zero(_currency));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.InvalidAmount);
    }

    [Fact]
    public void Transfer_WithCurrencyMismatchBetweenWallets_ShouldReturnCurrencyMismatchError()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), Currency.USD).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(500m, Currency.USD).Value);

        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), Currency.EUR).Value;
        var amount = Money.Create(100m, Currency.USD).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.CurrencyMismatch);
    }

    [Fact]
    public void Transfer_WithCurrencyMismatchBetweenAmountAndWallet_ShouldReturnCurrencyMismatchError()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), Currency.USD).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(500m, Currency.USD).Value);

        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), Currency.USD).Value;
        var amount = Money.Create(100m, Currency.EUR).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.CurrencyMismatch);
    }

    [Fact]
    public void Transfer_WithInsufficientFundsInSource_ShouldReturnInsufficientFundsError()
    {
        // Arrange
        var sourceWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(50m, _currency).Value);

        var targetWallet = Wallet.Create(WalletId.New(), OwnerId.New(), _currency).Value;
        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = _sut.Transfer(sourceWallet, targetWallet, TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.InsufficientFunds);
    }
}
