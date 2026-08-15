using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Common.Interfaces;

public interface IWalletReadService
{
    Task<Result<WalletSummaryDto>> GetWalletSummaryAsync(Guid walletId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TransactionHistoryDto>>> GetTransactionHistoryAsync(Guid walletId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<WalletSummaryDto>>> GetUserWalletsAsync(Guid ownerId, CancellationToken cancellationToken = default);
}
