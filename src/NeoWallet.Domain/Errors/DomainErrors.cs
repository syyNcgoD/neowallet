using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Errors;
public static class DomainErrors
{
    public static class Wallet
    {
        public static Error NotFound(WalletId id) =>
            Error.NotFound("Wallet.NotFound", $"Wallet with ID '{id}' was not found.");

        public static readonly Error InsufficientFunds =
            Error.Conflict("Wallet.InsufficientFunds", "Insufficient funds in the wallet to complete the operation.");

        public static readonly Error IsLocked =
            Error.Conflict("Wallet.IsLocked", "The wallet is currently locked and cannot perform financial operations.");

        public static readonly Error AlreadyLocked =
            Error.Conflict("Wallet.AlreadyLocked", "The wallet is already in locked status.");

        public static readonly Error AlreadyActive =
            Error.Conflict("Wallet.AlreadyActive", "The wallet is already in active status.");

        public static readonly Error CurrencyMismatch =
            Error.Validation("Wallet.CurrencyMismatch", "The currency of the operation does not match the wallet's currency.");

        public static readonly Error InvalidAmount =
            Error.Validation("Wallet.InvalidAmount", "The specified amount must be strictly greater than zero.");

        public static readonly Error SameSourceAndTarget =
            Error.Validation("Wallet.SameSourceAndTarget", "Source and target wallets cannot be the same.");

        public static readonly Error TargetWalletLocked =
            Error.Conflict("Wallet.TargetWalletLocked", "The destination wallet is locked and cannot receive funds.");
    }

    public static class Transaction
    {
        public static Error NotFound(TransactionId id) =>
            Error.NotFound("Transaction.NotFound", $"Transaction with ID '{id}' was not found.");

        public static readonly Error DuplicateId =
            Error.Conflict("Transaction.DuplicateId", "A transaction with this identifier has already been processed.");
    }

    public static class Concurrency
    {
        public static readonly Error Conflict =
            Error.Conflict("Concurrency.Conflict", "A concurrency conflict occurred. The aggregate was modified by another operation.");
    }
}
