namespace NeoWallet.Domain.Enums;

/// <summary>
/// Specifies the type of financial transaction processed on a wallet.
/// </summary>
public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    TransferIn = 3,
    TransferOut = 4
}
