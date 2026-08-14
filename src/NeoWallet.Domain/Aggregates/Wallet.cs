using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Aggregates;

/// <summary>
/// Event Sourced Aggregate Root representing a financial wallet.
/// Encapsulates balance mutations, currency invariant enforcement, and lifecycle state changes.
/// </summary>
public sealed class Wallet : AggregateRoot<WalletId>
{
    public OwnerId OwnerId { get; private set; }
    public Currency Currency { get; private set; } = default!;
    public Money Balance { get; private set; } = default!;
    public WalletStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    /// <summary>
    /// Parameterless constructor required for Event Sourcing snapshotting and rehydration.
    /// </summary>
    private Wallet()
    {
    }

    /// <summary>
    /// Factory method to initialize a new Wallet aggregate root.
    /// </summary>
    public static Result<Wallet> Create(WalletId id, OwnerId ownerId, Currency currency)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<Wallet>(Error.Validation("Wallet.EmptyId", "Wallet ID cannot be empty."));
        }

        if (ownerId.Value == Guid.Empty)
        {
            return Result.Failure<Wallet>(Error.Validation("Wallet.EmptyOwnerId", "Owner ID cannot be empty."));
        }

        if (currency is null)
        {
            return Result.Failure<Wallet>(Error.Validation("Wallet.NullCurrency", "Currency cannot be null."));
        }

        var wallet = new Wallet();
        var @event = WalletCreated.Create(id, ownerId, currency);
        wallet.RaiseEvent(@event);

        return Result.Success(wallet);
    }

    /// <summary>
    /// Deposits a monetary amount into the wallet.
    /// </summary>
    public Result Deposit(
        TransactionId transactionId,
        Money amount,
        string? reference = null,
        string? description = null)
    {
        if (Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.IsLocked);
        }

        if (amount is null || !amount.IsPositive)
        {
            return Result.Failure(DomainErrors.Wallet.InvalidAmount);
        }

        if (amount.Currency != Currency)
        {
            return Result.Failure(DomainErrors.Wallet.CurrencyMismatch);
        }

        var newBalance = Balance + amount;
        var @event = MoneyDeposited.Create(Id, transactionId, amount, newBalance, reference, description);
        RaiseEvent(@event);

        return Result.Success();
    }

    /// <summary>
    /// Withdraws a monetary amount from the wallet.
    /// </summary>
    public Result Withdraw(
        TransactionId transactionId,
        Money amount,
        string? reference = null,
        string? description = null)
    {
        if (Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.IsLocked);
        }

        if (amount is null || !amount.IsPositive)
        {
            return Result.Failure(DomainErrors.Wallet.InvalidAmount);
        }

        if (amount.Currency != Currency)
        {
            return Result.Failure(DomainErrors.Wallet.CurrencyMismatch);
        }

        if (Balance < amount)
        {
            return Result.Failure(DomainErrors.Wallet.InsufficientFunds);
        }

        var newBalance = Balance - amount;
        var @event = MoneyWithdrawn.Create(Id, transactionId, amount, newBalance, reference, description);
        RaiseEvent(@event);

        return Result.Success();
    }

    /// <summary>
    /// Transfers money out of this wallet to another wallet (P2P debit step).
    /// </summary>
    public Result TransferOut(
        TransactionId transactionId,
        WalletId targetWalletId,
        Money amount,
        string? reference = null)
    {
        if (Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.IsLocked);
        }

        if (targetWalletId == Id)
        {
            return Result.Failure(DomainErrors.Wallet.SameSourceAndTarget);
        }

        if (amount is null || !amount.IsPositive)
        {
            return Result.Failure(DomainErrors.Wallet.InvalidAmount);
        }

        if (amount.Currency != Currency)
        {
            return Result.Failure(DomainErrors.Wallet.CurrencyMismatch);
        }

        if (Balance < amount)
        {
            return Result.Failure(DomainErrors.Wallet.InsufficientFunds);
        }

        var newBalance = Balance - amount;
        var @event = MoneyTransferredOut.Create(Id, targetWalletId, transactionId, amount, newBalance, reference);
        RaiseEvent(@event);

        return Result.Success();
    }

    /// <summary>
    /// Receives money transferred into this wallet from another wallet (P2P credit step).
    /// </summary>
    public Result TransferIn(
        TransactionId transactionId,
        WalletId sourceWalletId,
        Money amount,
        string? reference = null)
    {
        if (Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.IsLocked);
        }

        if (sourceWalletId == Id)
        {
            return Result.Failure(DomainErrors.Wallet.SameSourceAndTarget);
        }

        if (amount is null || !amount.IsPositive)
        {
            return Result.Failure(DomainErrors.Wallet.InvalidAmount);
        }

        if (amount.Currency != Currency)
        {
            return Result.Failure(DomainErrors.Wallet.CurrencyMismatch);
        }

        var newBalance = Balance + amount;
        var @event = MoneyTransferredIn.Create(Id, sourceWalletId, transactionId, amount, newBalance, reference);
        RaiseEvent(@event);

        return Result.Success();
    }

    /// <summary>
    /// Locks the wallet preventing all debit and credit operations.
    /// </summary>
    public Result Lock(string reason)
    {
        if (Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.AlreadyLocked);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(Error.Validation("Wallet.EmptyLockReason", "A reason must be provided when locking a wallet."));
        }

        var @event = WalletLocked.Create(Id, reason);
        RaiseEvent(@event);

        return Result.Success();
    }

    /// <summary>
    /// Unlocks a previously locked wallet back to active status.
    /// </summary>
    public Result Unlock(string reason)
    {
        if (Status == WalletStatus.Active)
        {
            return Result.Failure(DomainErrors.Wallet.AlreadyActive);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(Error.Validation("Wallet.EmptyUnlockReason", "A reason must be provided when unlocking a wallet."));
        }

        var @event = WalletUnlocked.Create(Id, reason);
        RaiseEvent(@event);

        return Result.Success();
    }

    /// <summary>
    /// Dispatches domain events to aggregate state mutators.
    /// </summary>
    protected override void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case WalletCreated e:
                Id = e.WalletId;
                OwnerId = e.OwnerId;
                Currency = e.Currency;
                Balance = Money.Zero(e.Currency);
                Status = WalletStatus.Active;
                CreatedAtUtc = e.CreatedAtUtc;
                break;

            case MoneyDeposited e:
                Balance = e.BalanceAfter;
                LastModifiedAtUtc = e.DepositedAtUtc;
                break;

            case MoneyWithdrawn e:
                Balance = e.BalanceAfter;
                LastModifiedAtUtc = e.WithdrawnAtUtc;
                break;

            case MoneyTransferredOut e:
                Balance = e.BalanceAfter;
                LastModifiedAtUtc = e.TransferredAtUtc;
                break;

            case MoneyTransferredIn e:
                Balance = e.BalanceAfter;
                LastModifiedAtUtc = e.TransferredAtUtc;
                break;

            case WalletLocked e:
                Status = WalletStatus.Locked;
                LastModifiedAtUtc = e.LockedAtUtc;
                break;

            case WalletUnlocked e:
                Status = WalletStatus.Active;
                LastModifiedAtUtc = e.UnlockedAtUtc;
                break;
        }
    }
}
