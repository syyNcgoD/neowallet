using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Services;

/// <summary>
/// Domain service implementing cross-aggregate transfer invariants and state transitions.
/// </summary>
public sealed class TransferService : ITransferService
{
    public Result Transfer(
        Wallet sourceWallet,
        Wallet targetWallet,
        TransactionId transactionId,
        Money amount,
        string? reference = null)
    {
        if (sourceWallet is null)
        {
            return Result.Failure(Error.Validation("Transfer.NullSourceWallet", "Source wallet cannot be null."));
        }

        if (targetWallet is null)
        {
            return Result.Failure(Error.Validation("Transfer.NullTargetWallet", "Target wallet cannot be null."));
        }

        if (sourceWallet.Id == targetWallet.Id)
        {
            return Result.Failure(DomainErrors.Wallet.SameSourceAndTarget);
        }

        if (sourceWallet.Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.IsLocked);
        }

        if (targetWallet.Status == WalletStatus.Locked)
        {
            return Result.Failure(DomainErrors.Wallet.TargetWalletLocked);
        }

        if (amount is null || !amount.IsPositive)
        {
            return Result.Failure(DomainErrors.Wallet.InvalidAmount);
        }

        if (sourceWallet.Currency != targetWallet.Currency)
        {
            return Result.Failure(DomainErrors.Wallet.CurrencyMismatch);
        }

        if (amount.Currency != sourceWallet.Currency)
        {
            return Result.Failure(DomainErrors.Wallet.CurrencyMismatch);
        }

        if (sourceWallet.Balance < amount)
        {
            return Result.Failure(DomainErrors.Wallet.InsufficientFunds);
        }

        // Debit source wallet
        var debitResult = sourceWallet.TransferOut(transactionId, targetWallet.Id, amount, reference);
        if (debitResult.IsFailure)
        {
            return debitResult;
        }

        // Credit target wallet
        var creditResult = targetWallet.TransferIn(transactionId, sourceWallet.Id, amount, reference);
        if (creditResult.IsFailure)
        {
            return creditResult;
        }

        return Result.Success();
    }
}
