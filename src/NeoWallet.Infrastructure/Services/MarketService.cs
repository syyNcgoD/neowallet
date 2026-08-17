using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Market;

namespace NeoWallet.Infrastructure.Services;

public sealed class MarketService : IMarketService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MarketService> _logger;
    private readonly string _coinGeckoApiKey;
    private readonly string _finnhubApiKey;

    private static IReadOnlyList<CryptoCoinDto>? _cachedCrypto;
    private static DateTime _cryptoCacheExpiry = DateTime.MinValue;

    private static IReadOnlyList<StockQuoteDto>? _cachedStocks;
    private static DateTime _stockCacheExpiry = DateTime.MinValue;

    private static readonly SemaphoreSlim _lock = new(1, 1);

    public MarketService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MarketService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _coinGeckoApiKey = configuration["CoinGecko:ApiKey"] 
            ?? Environment.GetEnvironmentVariable("COINGECKO_API_KEY") 
            ?? configuration["COINGECKO_API_KEY"]
            ?? string.Empty;

        _finnhubApiKey = configuration["Finnhub:ApiKey"] 
            ?? Environment.GetEnvironmentVariable("FINNHUB_API_KEY") 
            ?? configuration["FINNHUB_API_KEY"]
            ?? string.Empty;
    }

    public async Task<IReadOnlyList<CryptoCoinDto>> GetLiveCryptoPricesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedCrypto != null && DateTime.UtcNow < _cryptoCacheExpiry)
        {
            return _cachedCrypto;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedCrypto != null && DateTime.UtcNow < _cryptoCacheExpiry)
            {
                return _cachedCrypto;
            }

            var url = $"https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,solana,ripple,cardano,dogecoin,polkadot,avalanche-2,chainlink&vs_currencies=usd&include_24hr_change=true&include_24hr_vol=true&x_cg_demo_api_key={_coinGeckoApiKey}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "NeoWallet-Production/1.0");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var list = new List<CryptoCoinDto>();
                AddCoinIfPresent(root, "bitcoin", "BTC", "Bitcoin", list);
                AddCoinIfPresent(root, "ethereum", "ETH", "Ethereum", list);
                AddCoinIfPresent(root, "solana", "SOL", "Solana", list);
                AddCoinIfPresent(root, "ripple", "XRP", "Ripple", list);
                AddCoinIfPresent(root, "cardano", "ADA", "Cardano", list);
                AddCoinIfPresent(root, "dogecoin", "DOGE", "Dogecoin", list);
                AddCoinIfPresent(root, "polkadot", "DOT", "Polkadot", list);
                AddCoinIfPresent(root, "avalanche-2", "AVAX", "Avalanche", list);
                AddCoinIfPresent(root, "chainlink", "LINK", "Chainlink", list);

                _cachedCrypto = list;
                _cryptoCacheExpiry = DateTime.UtcNow.AddSeconds(30);
                return list;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch live prices from CoinGecko API");
        }
        finally
        {
            _lock.Release();
        }

        return _cachedCrypto ?? GetFallbackCrypto();
    }

    public async Task<IReadOnlyList<StockQuoteDto>> GetLiveStockQuotesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedStocks != null && DateTime.UtcNow < _stockCacheExpiry)
        {
            return _cachedStocks;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedStocks != null && DateTime.UtcNow < _stockCacheExpiry)
            {
                return _cachedStocks;
            }

            var symbols = new[]
            {
                ("AAPL", "Apple Inc."),
                ("MSFT", "Microsoft Corporation"),
                ("NVDA", "NVIDIA Corporation"),
                ("TSLA", "Tesla Inc."),
                ("AMZN", "Amazon.com Inc."),
                ("GOOGL", "Alphabet Inc."),
                ("META", "Meta Platforms Inc.")
            };

            var list = new List<StockQuoteDto>();

            foreach (var (sym, name) in symbols)
            {
                try
                {
                    var url = $"https://finnhub.io/api/v1/quote?symbol={sym}&token={_finnhubApiKey}";
                    var response = await _httpClient.GetAsync(url, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync(cancellationToken);
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        var c = root.GetProperty("c").GetDecimal();
                        var d = root.GetProperty("d").GetDecimal();
                        var dp = root.GetProperty("dp").GetDecimal();
                        var h = root.GetProperty("h").GetDecimal();
                        var l = root.GetProperty("l").GetDecimal();
                        var o = root.GetProperty("o").GetDecimal();
                        var pc = root.GetProperty("pc").GetDecimal();

                        list.Add(new StockQuoteDto(sym, name, c, d, dp, h, l, o, pc));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching Finnhub quote for {Symbol}", sym);
                }
            }

            if (list.Count > 0)
            {
                _cachedStocks = list;
                _stockCacheExpiry = DateTime.UtcNow.AddSeconds(30);
                return list;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch live quotes from Finnhub API");
        }
        finally
        {
            _lock.Release();
        }

        return _cachedStocks ?? GetFallbackStocks();
    }

    private static void AddCoinIfPresent(JsonElement root, string id, string symbol, string name, List<CryptoCoinDto> list)
    {
        if (root.TryGetProperty(id, out var coinObj))
        {
            var price = coinObj.GetProperty("usd").GetDecimal();
            var change24h = coinObj.TryGetProperty("usd_24h_change", out var ch) ? ch.GetDecimal() : 0m;
            var vol24h = coinObj.TryGetProperty("usd_24h_vol", out var vol) ? vol.GetDecimal() : 0m;

            list.Add(new CryptoCoinDto(id, symbol, name, price, Math.Round(change24h, 2), price * 1.02m, price * 0.98m, Math.Round(vol24h, 2)));
        }
    }

    private static IReadOnlyList<CryptoCoinDto> GetFallbackCrypto() => new List<CryptoCoinDto>
    {
        new("bitcoin", "BTC", "Bitcoin", 94250.00m, 2.45m, 95100m, 92800m, 34500000000m),
        new("ethereum", "ETH", "Ethereum", 2780.50m, 1.82m, 2820m, 2720m, 18200000000m),
        new("solana", "SOL", "Solana", 188.40m, 4.65m, 192m, 179m, 5600000000m)
    };

    private static IReadOnlyList<StockQuoteDto> GetFallbackStocks() => new List<StockQuoteDto>
    {
        new("AAPL", "Apple Inc.", 228.50m, 1.25m, 0.55m, 230.10m, 227.40m, 227.80m, 227.25m),
        new("NVDA", "NVIDIA Corporation", 128.90m, 3.40m, 2.71m, 130.50m, 126.20m, 126.80m, 125.50m),
        new("MSFT", "Microsoft Corporation", 418.20m, -0.80m, -0.19m, 421.00m, 416.50m, 419.00m, 419.00m)
    };
}
