using System.Text.RegularExpressions;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;
public sealed partial record Currency : IComparable<Currency>
{
    private static readonly Regex CurrencyCodeRegex = Iso4217Regex();

    public static readonly Currency USD = new("USD", "$", 2);
    public static readonly Currency EUR = new("EUR", "€", 2);
    public static readonly Currency GBP = new("GBP", "£", 2);
    public static readonly Currency IRR = new("IRR", "﷼", 0);
    public static readonly Currency AED = new("AED", "د.إ", 2);
    public static readonly Currency CAD = new("CAD", "$", 2);
    public static readonly Currency JPY = new("JPY", "¥", 0);

    private static readonly Dictionary<string, Currency> KnownCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        [USD.Code] = USD,
        [EUR.Code] = EUR,
        [GBP.Code] = GBP,
        [IRR.Code] = IRR,
        [AED.Code] = AED,
        [CAD.Code] = CAD,
        [JPY.Code] = JPY
    };

    public string Code { get; }
    public string Symbol { get; }
    public int DecimalPlaces { get; }

    private Currency(string code, string symbol, int decimalPlaces)
    {
        Code = code.ToUpperInvariant();
        Symbol = symbol;
        DecimalPlaces = decimalPlaces;
    }

    public static Result<Currency> FromCode(string? code, string? symbol = null, int decimalPlaces = 2)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Currency>(Error.Validation("Currency.EmptyCode", "Currency code cannot be null or whitespace."));
        }

        var normalizedCode = code.Trim().ToUpperInvariant();

        if (KnownCurrencies.TryGetValue(normalizedCode, out var knownCurrency))
        {
            return Result.Success(knownCurrency);
        }

        if (!CurrencyCodeRegex.IsMatch(normalizedCode))
        {
            return Result.Failure<Currency>(Error.Validation("Currency.InvalidCode", $"Currency code '{code}' must be a valid 3-letter ISO-4217 code."));
        }

        if (decimalPlaces < 0 || decimalPlaces > 8)
        {
            return Result.Failure<Currency>(Error.Validation("Currency.InvalidDecimalPlaces", "Decimal places must be between 0 and 8."));
        }

        return Result.Success(new Currency(normalizedCode, symbol ?? normalizedCode, decimalPlaces));
    }

    public int CompareTo(Currency? other) =>
        other is null ? 1 : string.Compare(Code, other.Code, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Code;

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex Iso4217Regex();
}
