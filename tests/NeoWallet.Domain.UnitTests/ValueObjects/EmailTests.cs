using FluentAssertions;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("  USER@EXAMPLE.COM  ", "user@example.com")]
    [InlineData("john.doe+tag@sub.domain.org", "john.doe+tag@sub.domain.org")]
    public void Create_WithValidEmail_ShouldNormalizeAndReturnSuccess(string input, string expected)
    {
        var result = Email.Create(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        ((string)result.Value).Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldReturnValidationFailure(string? input)
    {
        var result = Email.Create(input);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Empty");
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@.com")]
    [InlineData("username@domain")]
    public void Create_WithInvalidFormat_ShouldReturnValidationFailure(string input)
    {
        var result = Email.Create(input);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.InvalidFormat");
    }

    [Fact]
    public void Create_WithTooLongEmail_ShouldReturnValidationFailure()
    {
        var longEmail = new string('a', 250) + "@domain.com";
        var result = Email.Create(longEmail);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.TooLong");
    }

    [Fact]
    public void Equality_And_Comparison_ShouldWorkProperly()
    {
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("TEST@example.com").Value;
        var email3 = Email.Create("other@example.com").Value;

        email1.Should().Be(email2);
        email1.Equals(email3).Should().BeFalse();
        email1.CompareTo(email3).Should().BePositive();
        email1.CompareTo(null).Should().Be(1);
        email1.ToString().Should().Be("test@example.com");
    }
}
