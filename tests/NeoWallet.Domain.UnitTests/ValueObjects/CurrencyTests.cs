using FluentAssertions;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.ValueObjects;

public sealed class CurrencyTests
{
    [Theory]
    [InlineData("USD", "$", 2)]
    [InlineData("EUR", "€", 2)]
    [InlineData("GBP", "£", 2)]
    [InlineData("IRR", "﷼", 0)]
    [InlineData("AED", "د.إ", 2)]
    [InlineData("CAD", "$", 2)]
    [InlineData("JPY", "¥", 0)]
    public void KnownCurrencies_ShouldHaveExpectedProperties(string code, string symbol, int decimals)
    {
        // Act
        var result = Currency.FromCode(code);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be(code);
        result.Value.Symbol.Should().Be(symbol);
        result.Value.DecimalPlaces.Should().Be(decimals);
    }

    [Fact]
    public void FromCode_WithCustomValidIsoCode_ShouldCreateCurrency()
    {
        // Act
        var resultWithSymbol = Currency.FromCode("CHF", "Fr.", 2);
        var resultWithoutSymbol = Currency.FromCode("NOK");

        // Assert
        resultWithSymbol.IsSuccess.Should().BeTrue();
        resultWithSymbol.Value.Code.Should().Be("CHF");
        resultWithSymbol.Value.Symbol.Should().Be("Fr.");
        resultWithSymbol.Value.DecimalPlaces.Should().Be(2);

        resultWithoutSymbol.IsSuccess.Should().BeTrue();
        resultWithoutSymbol.Value.Code.Should().Be("NOK");
        resultWithoutSymbol.Value.Symbol.Should().Be("NOK");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void FromCode_WithNullOrWhitespace_ShouldReturnFailure(string? code)
    {
        // Act
        var result = Currency.FromCode(code);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Currency.EmptyCode");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("123")]
    [InlineData("US1")]
    public void FromCode_WithInvalidIsoCode_ShouldReturnFailure(string code)
    {
        // Act
        var result = Currency.FromCode(code);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Currency.InvalidCode");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void FromCode_WithInvalidDecimalPlaces_ShouldReturnFailure(int decimals)
    {
        // Act
        var result = Currency.FromCode("SEK", "kr", decimals);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Currency.InvalidDecimalPlaces");
    }

    [Fact]
    public void Equality_WithSameCodeCaseInsensitive_ShouldBeEqual()
    {
        // Arrange
        var c1 = Currency.FromCode("usd").Value;
        var c2 = Currency.USD;

        // Assert
        c1.Should().Be(c2);
        (c1 == c2).Should().BeTrue();
    }

    [Fact]
    public void CompareTo_ShouldSortAlphabetically()
    {
        // Arrange
        var eur = Currency.EUR;
        var usd = Currency.USD;

        // Assert
        eur.CompareTo(usd).Should().BeNegative();
        usd.CompareTo(eur).Should().BePositive();
        usd.CompareTo(null).Should().Be(1);
    }

    [Fact]
    public void ToString_ShouldReturnCode()
    {
        // Act & Assert
        Currency.USD.ToString().Should().Be("USD");
    }
}
