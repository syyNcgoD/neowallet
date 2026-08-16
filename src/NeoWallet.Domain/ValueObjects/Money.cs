using System.Globalization;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.ValueObjects;
public sealed record Money : IComparable<Money>
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public bool IsZero => Amount == 0m;
    public bool IsPositive => Amount > 0m;
    public bool IsNegative => Amount < 0m;

    [System.Text.Json.Serialization.JsonConstructor]
    public Money(decimal amount, Currency currency)
    {
        Currency = currency;
        // Normalize amount to currency's decimal precision
        Amount = currency is not null ? Math.Round(amount, currency.DecimalPlaces, MidpointRounding.AwayFromZero) : amount;
    }

    public static Money Zero(Currency currency)
    {
        Guard.AgainstNull(currency, nameof(currency));
        return new Money(0m, currency);
    }

    public static Result<Money> Create(decimal amount, Currency? currency)
    {
        if (currency is null)
        {
            return Result.Failure<Money>(Error.Validation("Money.NullCurrency", "Currency cannot be null."));
        }

        return Result.Success(new Money(amount, currency));
    }

    public static Result<Money> Create(decimal amount, string currencyCode)
    {
        var currencyResult = Currency.FromCode(currencyCode);
        if (currencyResult.IsFailure)
        {
            return Result.Failure<Money>(currencyResult.Error);
        }

        return Create(amount, currencyResult.Value);
    }

    public Result<Money> Add(Money other)
    {
        if (other is null)
        {
            return Result.Failure<Money>(Error.NullValue);
        }

        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(Error.Validation(
                "Money.CurrencyMismatch",
                $"Cannot add money with different currencies ({Currency.Code} and {other.Currency.Code})."));
        }

        return Result.Success(new Money(Amount + other.Amount, Currency));
    }

    public Result<Money> Subtract(Money other)
    {
        if (other is null)
        {
            return Result.Failure<Money>(Error.NullValue);
        }

        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(Error.Validation(
                "Money.CurrencyMismatch",
                $"Cannot subtract money with different currencies ({Currency.Code} and {other.Currency.Code})."));
        }

        return Result.Success(new Money(Amount - other.Amount, Currency));
    }

    public Result<Money> Multiply(decimal factor)
    {
        return Result.Success(new Money(Amount * factor, Currency));
    }

    public Result<Money> Divide(decimal divisor)
    {
        if (divisor == 0m)
        {
            return Result.Failure<Money>(Error.Validation("Money.DivideByZero", "Cannot divide money by zero."));
        }

        return Result.Success(new Money(Amount / divisor, Currency));
    }

    public static Money operator +(Money left, Money right)
    {
        Guard.AgainstNull(left, nameof(left));
        Guard.AgainstNull(right, nameof(right));

        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot add amounts with different currencies: {left.Currency.Code} vs {right.Currency.Code}.");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        Guard.AgainstNull(left, nameof(left));
        Guard.AgainstNull(right, nameof(right));

        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot subtract amounts with different currencies: {left.Currency.Code} vs {right.Currency.Code}.");
        }

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static bool operator <(Money left, Money right)
    {
        Guard.AgainstNull(left, nameof(left));
        Guard.AgainstNull(right, nameof(right));
        EnsureSameCurrency(left, right);

        return left.Amount < right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        Guard.AgainstNull(left, nameof(left));
        Guard.AgainstNull(right, nameof(right));
        EnsureSameCurrency(left, right);

        return left.Amount > right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        Guard.AgainstNull(left, nameof(left));
        Guard.AgainstNull(right, nameof(right));
        EnsureSameCurrency(left, right);

        return left.Amount <= right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        Guard.AgainstNull(left, nameof(left));
        Guard.AgainstNull(right, nameof(right));
        EnsureSameCurrency(left, right);

        return left.Amount >= right.Amount;
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException($"Cannot compare amounts with different currencies: {left.Currency.Code} vs {right.Currency.Code}.");
        }
    }

    public int CompareTo(Money? other)
    {
        if (other is null)
        {
            return 1;
        }

        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    public override string ToString() =>
        $"{Amount.ToString($"N{Currency.DecimalPlaces}", CultureInfo.InvariantCulture)} {Currency.Code}";
}
