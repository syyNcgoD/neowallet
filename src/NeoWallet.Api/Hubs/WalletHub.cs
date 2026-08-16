using Microsoft.AspNetCore.SignalR;

namespace NeoWallet.Api.Hubs;

public sealed class WalletHub : Hub
{
    public async Task JoinWalletGroup(string walletId)
    {
        if (Guid.TryParse(walletId, out var id) && id != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"wallet_{id}");
        }
    }

    public async Task LeaveWalletGroup(string walletId)
    {
        if (Guid.TryParse(walletId, out var id) && id != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"wallet_{id}");
        }
    }
}
