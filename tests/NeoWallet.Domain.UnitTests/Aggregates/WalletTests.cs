using FluentAssertions;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.Aggregates;

public sealed class WalletTests
{
    private readonly WalletId _walletId = WalletId.New();
    private readonly OwnerId _ownerId = OwnerId.New();
    private readonly Currency _currency = Currency.USD;

    [Fact]
    public void Create_WithValidParameters_ShouldInitializeActiveWalletWithZeroBalanceAndEmitWalletCreatedEvent()
    {
        // Act
        var result = Wallet.Create(_walletId, _ownerId, _currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var wallet = result.Value;
        wallet.Id.Should().Be(_walletId);
        wallet.OwnerId.Should().Be(_ownerId);
        wallet.Currency.Should().Be(_currency);
        wallet.Balance.Amount.Should().Be(0m);
        wallet.Status.Should().Be(WalletStatus.Active);
        wallet.Version.Should().Be(0);

        wallet.UncommittedEvents.Should().HaveCount(1);
        var domainEvent = wallet.UncommittedEvents.Single().Should().BeOfType<WalletCreated>().Subject;
        domainEvent.WalletId.Should().Be(_walletId);
        domainEvent.OwnerId.Should().Be(_ownerId);
        domainEvent.Currency.Should().Be(_currency);
        domainEvent.AggregateId.Should().Be(_walletId.Value);
    }

    [Fact]
    public void Create_WithEmptyWalletId_ShouldReturnFailure()
    {
        // Act
        var result = Wallet.Create(WalletId.Empty, _ownerId, _currency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.EmptyId");
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldReturnFailure()
    {
        // Act
        var result = Wallet.Create(_walletId, OwnerId.Empty, _currency);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.EmptyOwnerId");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldReturnFailure()
    {
        // Act
        var result = Wallet.Create(_walletId, _ownerId, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Wallet.NullCurrency");
    }

    [Fact]
    public void Deposit_WithPositiveAmountAndMatchingCurrency_ShouldIncreaseBalanceAndEmitMoneyDepositedEvent()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        var depositAmount = Money.Create(150m, _currency).Value;
        var txId = TransactionId.New();

        // Act
        var result = wallet.Deposit(txId, depositAmount, "REF-DEP-01", "Initial deposit");

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(150m);
        wallet.Version.Should().Be(1);

        wallet.UncommittedEvents.Should().HaveCount(2);
        var @event = wallet.UncommittedEvents.Last().Should().BeOfType<MoneyDeposited>().Subject;
        @event.TransactionId.Should().Be(txId);
        @event.Amount.Should().Be(depositAmount);
        @event.BalanceAfter.Amount.Should().Be(150m);
        @event.Reference.Should().Be("REF-DEP-01");
        @event.Description.Should().Be("Initial deposit");
    }

    [Fact]
    public void Deposit_WhenWalletIsLocked_ShouldReturnIsLockedError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Lock("Suspicious activity");
        var amount = Money.Create(100m, _currency).Value;

        // Act
        var result = wallet.Deposit(TransactionId.New(), amount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.IsLocked);
    }

    [Fact]
    public void Deposit_WithNegativeOrZeroAmount_ShouldReturnInvalidAmountError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        var zeroAmount = Money.Zero(_currency);

        // Act
        var result = wallet.Deposit(TransactionId.New(), zeroAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.InvalidAmount);
    }

    [Fact]
    public void Deposit_WithMismatchedCurrency_ShouldReturnCurrencyMismatchError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        var eurAmount = Money.Create(100m, Currency.EUR).Value;

        // Act
        var result = wallet.Deposit(TransactionId.New(), eurAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.CurrencyMismatch);
    }

    [Fact]
    public void Withdraw_WithSufficientBalance_ShouldDecreaseBalanceAndEmitMoneyWithdrawnEvent()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(200m, _currency).Value);
        var withdrawAmount = Money.Create(75m, _currency).Value;
        var txId = TransactionId.New();

        // Act
        var result = wallet.Withdraw(txId, withdrawAmount, "ATM-WITHDRAW", "Cash out");

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Amount.Should().Be(125m);

        var @event = wallet.UncommittedEvents.Last().Should().BeOfType<MoneyWithdrawn>().Subject;
        @event.TransactionId.Should().Be(txId);
        @event.Amount.Should().Be(withdrawAmount);
        @event.BalanceAfter.Amount.Should().Be(125m);
    }

    [Fact]
    public void Withdraw_WithInsufficientBalance_ShouldReturnInsufficientFundsError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(50m, _currency).Value);
        var withdrawAmount = Money.Create(100m, _currency).Value;

        // Act
        var result = wallet.Withdraw(TransactionId.New(), withdrawAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.InsufficientFunds);
    }

    [Fact]
    public void Withdraw_WhenWalletIsLocked_ShouldReturnIsLockedError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(500m, _currency).Value);
        wallet.Lock("Security check");

        // Act
        var result = wallet.Withdraw(TransactionId.New(), Money.Create(100m, _currency).Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.IsLocked);
    }

    [Fact]
    public void Withdraw_WithMismatchedCurrency_ShouldReturnCurrencyMismatchError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(500m, _currency).Value);
        var eurAmount = Money.Create(50m, Currency.EUR).Value;

        // Act
        var result = wallet.Withdraw(TransactionId.New(), eurAmount);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.CurrencyMismatch);
    }

    [Fact]
    public void TransferOut_WithSufficientFunds_ShouldDecreaseBalanceAndEmitMoneyTransferredOutEvent()
    {
        // Arrange
        var sourceWallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        sourceWallet.Deposit(TransactionId.New(), Money.Create(1000m, _currency).Value);
        var targetWalletId = WalletId.New();
        var txId = TransactionId.New();
        var transferAmount = Money.Create(250m, _currency).Value;

        // Act
        var result = sourceWallet.TransferOut(txId, targetWalletId, transferAmount, "P2P-TRANSFER");

        // Assert
        result.IsSuccess.Should().BeTrue();
        sourceWallet.Balance.Amount.Should().Be(750m);

        var @event = sourceWallet.UncommittedEvents.Last().Should().BeOfType<MoneyTransferredOut>().Subject;
        @event.SourceWalletId.Should().Be(_walletId);
        @event.TargetWalletId.Should().Be(targetWalletId);
        @event.Amount.Should().Be(transferAmount);
        @event.BalanceAfter.Amount.Should().Be(750m);
    }

    [Fact]
    public void TransferOut_ToSameWallet_ShouldReturnSameSourceAndTargetError()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(100m, _currency).Value);

        // Act
        var result = wallet.TransferOut(TransactionId.New(), _walletId, Money.Create(50m, _currency).Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Wallet.SameSourceAndTarget);
    }

    [Fact]
    public void TransferIn_WithValidParameters_ShouldIncreaseBalanceAndEmitMoneyTransferredInEvent()
    {
        // Arrange
        var targetWallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        var sourceWalletId = WalletId.New();
        var txId = TransactionId.New();
        var transferAmount = Money.Create(300m, _currency).Value;

        // Act
        var result = targetWallet.TransferIn(txId, sourceWalletId, transferAmount, "P2P-RECV");

        // Assert
        result.IsSuccess.Should().BeTrue();
        targetWallet.Balance.Amount.Should().Be(300m);

        var @event = targetWallet.UncommittedEvents.Last().Should().BeOfType<MoneyTransferredIn>().Subject;
        @event.TargetWalletId.Should().Be(_walletId);
        @event.SourceWalletId.Should().Be(sourceWalletId);
        @event.Amount.Should().Be(transferAmount);
        @event.BalanceAfter.Amount.Should().Be(300m);
    }

    [Fact]
    public void LockAndUnlock_ShouldUpdateStatusAndEmitEvents()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;

        // Act & Assert 1: Lock
        var lockResult = wallet.Lock("Compliance audit");
        lockResult.IsSuccess.Should().BeTrue();
        wallet.Status.Should().Be(WalletStatus.Locked);

        var lockEvent = wallet.UncommittedEvents.Last().Should().BeOfType<WalletLocked>().Subject;
        lockEvent.Reason.Should().Be("Compliance audit");

        // Act & Assert 2: Double Lock should fail
        var doubleLock = wallet.Lock("Another reason");
        doubleLock.IsFailure.Should().BeTrue();
        doubleLock.Error.Should().Be(DomainErrors.Wallet.AlreadyLocked);

        // Act & Assert 3: Unlock
        var unlockResult = wallet.Unlock("Audit passed");
        unlockResult.IsSuccess.Should().BeTrue();
        wallet.Status.Should().Be(WalletStatus.Active);

        var unlockEvent = wallet.UncommittedEvents.Last().Should().BeOfType<WalletUnlocked>().Subject;
        unlockEvent.Reason.Should().Be("Audit passed");

        // Act & Assert 4: Double Unlock should fail
        var doubleUnlock = wallet.Unlock("Another reason");
        doubleUnlock.IsFailure.Should().BeTrue();
        doubleUnlock.Error.Should().Be(DomainErrors.Wallet.AlreadyActive);
    }

    [Fact]
    public void Lock_WithEmptyReason_ShouldReturnValidationFailure()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;

        // Act
        var result = wallet.Lock(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Unlock_WithEmptyReason_ShouldReturnValidationFailure()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Lock("Locked for test");

        // Act
        var result = wallet.Unlock("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void EventSourcing_LoadFromHistory_ShouldReconstructWalletStateAccurately()
    {
        // Arrange: Generate stream of historical events
        var walletId = WalletId.New();
        var ownerId = OwnerId.New();
        var currency = Currency.USD;
        var now = DateTime.UtcNow;

        var history = new List<IDomainEvent>
        {
            WalletCreated.Create(walletId, ownerId, currency, now),
            MoneyDeposited.Create(walletId, TransactionId.New(), Money.Create(500m, currency).Value, Money.Create(500m, currency).Value, "DEP1", "First deposit", now.AddMinutes(1)),
            MoneyWithdrawn.Create(walletId, TransactionId.New(), Money.Create(100m, currency).Value, Money.Create(400m, currency).Value, "WTH1", "First withdraw", now.AddMinutes(2)),
            MoneyTransferredIn.Create(walletId, WalletId.New(), TransactionId.New(), Money.Create(250m, currency).Value, Money.Create(650m, currency).Value, "TRF-IN", now.AddMinutes(3)),
            MoneyTransferredOut.Create(walletId, WalletId.New(), TransactionId.New(), Money.Create(50m, currency).Value, Money.Create(600m, currency).Value, "TRF-OUT", now.AddMinutes(4)),
            WalletLocked.Create(walletId, "Security lock", now.AddMinutes(5)),
            WalletUnlocked.Create(walletId, "Unlocked by admin", now.AddMinutes(6))
        };

        // Act: Create an empty wallet instance and replay history
        var rehydratedWallet = (Wallet)Activator.CreateInstance(typeof(Wallet), nonPublic: true)!;
        rehydratedWallet.LoadFromHistory(history);

        // Assert
        rehydratedWallet.Id.Should().Be(walletId);
        rehydratedWallet.OwnerId.Should().Be(ownerId);
        rehydratedWallet.Currency.Should().Be(currency);
        rehydratedWallet.Balance.Amount.Should().Be(600m);
        rehydratedWallet.Status.Should().Be(WalletStatus.Active);
        rehydratedWallet.Version.Should().Be(6); // 7 events -> 0..6
        rehydratedWallet.UncommittedEvents.Should().BeEmpty(); // No uncommitted events on rehydration
    }

    [Fact]
    public void ClearUncommittedEvents_ShouldEmptyUncommittedEventsList()
    {
        // Arrange
        var wallet = Wallet.Create(_walletId, _ownerId, _currency).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(100m, _currency).Value);
        wallet.UncommittedEvents.Should().HaveCount(2);

        // Act
        wallet.ClearUncommittedEvents();

        // Assert
        wallet.UncommittedEvents.Should().BeEmpty();
    }
}
