using FluentAssertions;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.Authentication;

namespace NeoWallet.Infrastructure.IntegrationTests.Security;

public sealed class TotpProviderTests
{
    private readonly TotpProvider _sut = new();
    private readonly TotpSecret _secret = TotpSecret.Create("JBSWY3DPEHPK3PXP").Value;

    [Fact]
    public void GenerateCode_ShouldProduce6DigitString()
    {
        var code = _sut.GenerateCode(_secret);

        code.Should().HaveLength(6);
        int.TryParse(code, out _).Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_WithCurrentCode_ShouldReturnTrue()
    {
        var now = DateTime.UtcNow;
        var code = _sut.GenerateCode(_secret, now);

        var isValid = _sut.VerifyCode(_secret, code, toleranceSteps: 1, now);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_WithinToleranceWindow_ShouldReturnTrue()
    {
        var baseTime = DateTime.UtcNow;
        // Generate code from previous 30s time step
        var pastCode = _sut.GenerateCode(_secret, baseTime.AddSeconds(-30));

        // Verify with 1 step tolerance
        var isValid = _sut.VerifyCode(_secret, pastCode, toleranceSteps: 1, baseTime);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyCode_OutsideToleranceWindow_ShouldReturnFalse()
    {
        var baseTime = DateTime.UtcNow;
        // Generate code from 5 minutes ago
        var oldCode = _sut.GenerateCode(_secret, baseTime.AddMinutes(-5));

        var isValid = _sut.VerifyCode(_secret, oldCode, toleranceSteps: 1, baseTime);

        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("")]
    [InlineData(null)]
    public void VerifyCode_WithInvalidCodeFormat_ShouldReturnFalse(string? code)
    {
        var isValid = _sut.VerifyCode(_secret, code!);
        isValid.Should().BeFalse();
    }
}
