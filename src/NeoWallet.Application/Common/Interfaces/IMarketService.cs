using NeoWallet.Application.DTOs.Market;

namespace NeoWallet.Application.Common.Interfaces;

public interface IMarketService
{
    Task<IReadOnlyList<CryptoCoinDto>> GetLiveCryptoPricesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockQuoteDto>> GetLiveStockQuotesAsync(CancellationToken cancellationToken = default);
}
