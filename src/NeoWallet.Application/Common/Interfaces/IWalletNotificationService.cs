namespace NeoWallet.Application.Common.Interfaces;

public interface IWalletNotificationService
{
    Task NotifyBalanceChangedAsync(
        Guid walletId,
        decimal newBalance,
        string currency,
        CancellationToken cancellationToken = default);

    Task NotifyTransactionOccurredAsync(
        Guid walletId,
        Guid transactionId,
        string type,
        decimal amount,
        string currency,
        decimal balanceAfter,
        CancellationToken cancellationToken = default);
}
