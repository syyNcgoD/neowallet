using FluentAssertions;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = Money.Create(100.50m, Currency.USD);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(100.50m);
        result.Value.Currency.Should().Be(Currency.USD);
        result.Value.IsPositive.Should().BeTrue();
        result.Value.IsZero.Should().BeFalse();
        result.Value.IsNegative.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldReturnFailure()
    {
        // Act
        var result = Money.Create(100m, (Currency)null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.NullCurrency");
    }

    [Fact]
    public void Create_WithValidCurrencyCode_ShouldReturnSuccess()
    {
        // Act
        var result = Money.Create(250m, "EUR");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(250m);
        result.Value.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void Create_WithInvalidCurrencyCode_ShouldReturnFailure()
    {
        // Act
        var result = Money.Create(250m, "INVALID");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Zero_WithValidCurrency_ShouldReturnZeroAmount()
    {
        // Act
        var money = Money.Zero(Currency.USD);

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be(Currency.USD);
        money.IsZero.Should().BeTrue();
        money.IsPositive.Should().BeFalse();
        money.IsNegative.Should().BeFalse();
    }

    [Fact]
    public void Zero_WithNullCurrency_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => Money.Zero(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var m1 = Money.Create(100m, Currency.USD).Value;
        var m2 = Money.Create(50m, Currency.USD).Value;

        // Act
        var result = m1.Add(m2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_WithNullOther_ShouldReturnFailure()
    {
        // Arrange
        var m1 = Money.Create(100m, Currency.USD).Value;

        // Act
        var result = m1.Add(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldReturnFailure()
    {
        // Arrange
        var m1 = Money.Create(100m, Currency.USD).Value;
        var m2 = Money.Create(50m, Currency.EUR).Value;

        // Act
        var result = m1.Add(m2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.CurrencyMismatch");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var m1 = Money.Create(100m, Currency.USD).Value;
        var m2 = Money.Create(40m, Currency.USD).Value;

        // Act
        var result = m1.Subtract(m2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(60m);
    }

    [Fact]
    public void Subtract_WithNullOther_ShouldReturnFailure()
    {
        // Arrange
        var m1 = Money.Create(100m, Currency.USD).Value;

        // Act
        var result = m1.Subtract(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldReturnFailure()
    {
        // Arrange
        var m1 = Money.Create(100m, Currency.USD).Value;
        var m2 = Money.Create(40m, Currency.EUR).Value;

        // Act
        var result = m1.Subtract(m2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.CurrencyMismatch");
    }

    [Fact]
    public void Multiply_WithFactor_ShouldReturnMultipliedMoney()
    {
        // Arrange
        var m = Money.Create(100m, Currency.USD).Value;

        // Act
        var result = m.Multiply(2.5m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(250m);
    }

    [Fact]
    public void Divide_WithValidDivisor_ShouldReturnDividedMoney()
    {
        // Arrange
        var m = Money.Create(100m, Currency.USD).Value;

        // Act
        var result = m.Divide(4m);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(25m);
    }

    [Fact]
    public void Divide_WithZero_ShouldReturnFailure()
    {
        // Arrange
        var m = Money.Create(100m, Currency.USD).Value;

        // Act
        var result = m.Divide(0m);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Money.DivideByZero");
    }

    [Fact]
    public void PlusOperator_WithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var m1 = Money.Create(70m, Currency.USD).Value;
        var m2 = Money.Create(30m, Currency.USD).Value;

        // Act
        var sum = m1 + m2;

        // Assert
        sum.Amount.Should().Be(100m);
    }

    [Fact]
    public void PlusOperator_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var m1 = Money.Create(70m, Currency.USD).Value;
        var m2 = Money.Create(30m, Currency.EUR).Value;

        // Act
        Action act = () => _ = m1 + m2;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MinusOperator_WithSameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var m1 = Money.Create(70m, Currency.USD).Value;
        var m2 = Money.Create(30m, Currency.USD).Value;

        // Act
        var diff = m1 - m2;

        // Assert
        diff.Amount.Should().Be(40m);
    }

    [Fact]
    public void MinusOperator_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var m1 = Money.Create(70m, Currency.USD).Value;
        var m2 = Money.Create(30m, Currency.EUR).Value;

        // Act
        Action act = () => _ = m1 - m2;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ComparisonOperators_WithSameCurrency_ShouldCompareAmountsCorrectly()
    {
        // Arrange
        var small = Money.Create(50m, Currency.USD).Value;
        var large = Money.Create(100m, Currency.USD).Value;
        var equal = Money.Create(50m, Currency.USD).Value;

        // Assert
        (small < large).Should().BeTrue();
        (large > small).Should().BeTrue();
        (small <= equal).Should().BeTrue();
        (small >= equal).Should().BeTrue();
        (large <= small).Should().BeFalse();
        (small >= large).Should().BeFalse();
        small.CompareTo(large).Should().BeNegative();
        large.CompareTo(small).Should().BePositive();
        small.CompareTo(equal).Should().Be(0);
        small.CompareTo(null).Should().Be(1);
    }

    [Fact]
    public void ComparisonOperators_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var m1 = Money.Create(50m, Currency.USD).Value;
        var m2 = Money.Create(50m, Currency.EUR).Value;

        // Assert
        Action less = () => _ = m1 < m2;
        Action greater = () => _ = m1 > m2;
        Action lessEq = () => _ = m1 <= m2;
        Action greaterEq = () => _ = m1 >= m2;
        Action compare = () => m1.CompareTo(m2);

        less.Should().Throw<InvalidOperationException>();
        greater.Should().Throw<InvalidOperationException>();
        lessEq.Should().Throw<InvalidOperationException>();
        greaterEq.Should().Throw<InvalidOperationException>();
        compare.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToString_ShouldFormatWithCurrencyCode()
    {
        // Arrange
        var m = Money.Create(1234.5m, Currency.USD).Value;

        // Act
        var formatted = m.ToString();

        // Assert
        formatted.Should().Be("1,234.50 USD");
    }

    [Fact]
    public void PrecisionRounding_ShouldRoundToCurrencyDecimalPlaces()
    {
        // USD has 2 decimal places
        var m1 = Money.Create(10.555m, Currency.USD).Value;
        m1.Amount.Should().Be(10.56m);

        // IRR has 0 decimal places
        var m2 = Money.Create(150000.75m, Currency.IRR).Value;
        m2.Amount.Should().Be(150001m);
    }
}
