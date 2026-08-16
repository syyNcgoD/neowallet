using Microsoft.AspNetCore.SignalR;
using NeoWallet.Api.Hubs;
using NeoWallet.Application.Common.Interfaces;

namespace NeoWallet.Api.Services;

public sealed class SignalRNotificationService : IWalletNotificationService
{
    private readonly IHubContext<WalletHub> _hubContext;

    public SignalRNotificationService(IHubContext<WalletHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyBalanceChangedAsync(
        Guid walletId,
        decimal newBalance,
        string currency,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"wallet_{walletId}").SendAsync(
            "BalanceChanged",
            new { WalletId = walletId, NewBalance = newBalance, Currency = currency },
            cancellationToken);
    }

    public async Task NotifyTransactionOccurredAsync(
        Guid walletId,
        Guid transactionId,
        string type,
        decimal amount,
        string currency,
        decimal balanceAfter,
        CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"wallet_{walletId}").SendAsync(
            "TransactionOccurred",
            new
            {
                WalletId = walletId,
                TransactionId = transactionId,
                Type = type,
                Amount = amount,
                Currency = currency,
                BalanceAfter = balanceAfter
            },
            cancellationToken);
    }
}
