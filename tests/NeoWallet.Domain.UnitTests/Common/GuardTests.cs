using FluentAssertions;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.UnitTests.Common;

public sealed class GuardTests
{
    [Fact]
    public void AgainstNull_WithNonNull_ShouldReturnValue()
    {
        var val = "hello";
        var res = Guard.AgainstNull(val, nameof(val));
        res.Should().Be("hello");
    }

    [Fact]
    public void AgainstNull_WithNull_ShouldThrowArgumentNullException()
    {
        string? val = null;
        Action act = () => Guard.AgainstNull(val, nameof(val));
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_WithInvalidInput_ShouldThrowArgumentException(string? input)
    {
        Action act = () => Guard.AgainstNullOrWhiteSpace(input, "param");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_WithValidInput_ShouldReturnValue()
    {
        var res = Guard.AgainstNullOrWhiteSpace("valid", "param");
        res.Should().Be("valid");
    }

    [Fact]
    public void AgainstDefault_WithDefaultStruct_ShouldThrowArgumentException()
    {
        Guid emptyGuid = Guid.Empty;
        Action act = () => Guard.AgainstDefault(emptyGuid, nameof(emptyGuid));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AgainstDefault_WithNonDefaultStruct_ShouldReturnValue()
    {
        Guid validGuid = Guid.NewGuid();
        var res = Guard.AgainstDefault(validGuid, nameof(validGuid));
        res.Should().Be(validGuid);
    }

    [Fact]
    public void AgainstNegative_WithNegativeDecimal_ShouldThrowArgumentOutOfRangeException()
    {
        Action act = () => Guard.AgainstNegative(-1m, "amount");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AgainstNegative_WithZeroOrPositiveDecimal_ShouldReturnValue()
    {
        Guard.AgainstNegative(0m, "amount").Should().Be(0m);
        Guard.AgainstNegative(10m, "amount").Should().Be(10m);
    }

    [Fact]
    public void AgainstZeroOrNegative_WithZeroOrNegative_ShouldThrowArgumentOutOfRangeException()
    {
        Action act1 = () => Guard.AgainstZeroOrNegative(0m, "amount");
        Action act2 = () => Guard.AgainstZeroOrNegative(-5m, "amount");

        act1.Should().Throw<ArgumentOutOfRangeException>();
        act2.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AgainstZeroOrNegative_WithPositive_ShouldReturnValue()
    {
        Guard.AgainstZeroOrNegative(5.5m, "amount").Should().Be(5.5m);
    }

    [Fact]
    public void AgainstNonUtc_WithUtc_ShouldReturnValue()
    {
        var utcNow = DateTime.UtcNow;
        Guard.AgainstNonUtc(utcNow, nameof(utcNow)).Should().Be(utcNow);
    }

    [Fact]
    public void AgainstNonUtc_WithLocalOrUnspecified_ShouldThrowArgumentException()
    {
        var localNow = DateTime.Now;
        var unspec = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        Action act1 = () => Guard.AgainstNonUtc(localNow, "local");
        Action act2 = () => Guard.AgainstNonUtc(unspec, "unspec");

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }
}
