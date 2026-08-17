namespace NeoWallet.Application.DTOs.Market;

public sealed record CryptoCoinDto(
    string Id,
    string Symbol,
    string Name,
    decimal CurrentPriceUsd,
    decimal Change24hPercent,
    decimal High24h,
    decimal Low24h,
    decimal Volume24h);

public sealed record StockQuoteDto(
    string Symbol,
    string CompanyName,
    decimal CurrentPrice,
    decimal Change,
    decimal PercentChange,
    decimal HighPriceOfDay,
    decimal LowPriceOfDay,
    decimal OpenPriceOfDay,
    decimal PreviousClosePrice);
