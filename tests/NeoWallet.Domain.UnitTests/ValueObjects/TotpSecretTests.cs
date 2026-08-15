using FluentAssertions;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.ValueObjects;

public sealed class TotpSecretTests
{
    [Fact]
    public void Generate_ShouldProduceValidBase32Secret()
    {
        var secret = TotpSecret.Generate();

        secret.Value.Should().NotBeNullOrWhiteSpace();
        secret.Value.Length.Should().BeGreaterThanOrEqualTo(16);
        secret.ToString().Should().Be("********");
    }

    [Fact]
    public void Create_WithValidBase32String_ShouldReturnSuccess()
    {
        var validBase32 = "JBSWY3DPEHPK3PXP";
        var result = TotpSecret.Create(validBase32);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validBase32);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldReturnValidationFailure(string? input)
    {
        var result = TotpSecret.Create(input);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TotpSecret.Empty");
    }

    [Fact]
    public void Create_WithInvalidBase32Characters_ShouldReturnValidationFailure()
    {
        var invalid = "JBSWY3DPEHPK3PXP8901!"; // 8, 9, 0, 1 are not standard RFC Base32
        var result = TotpSecret.Create(invalid);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TotpSecret.InvalidBase32");
    }

    [Fact]
    public void Create_WithTooShortSecret_ShouldReturnValidationFailure()
    {
        var shortSecret = "JBSWY3";
        var result = TotpSecret.Create(shortSecret);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TotpSecret.TooShort");
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldFormatOtpAuthStandard()
    {
        var secret = TotpSecret.Create("JBSWY3DPEHPK3PXP").Value;
        var uri = secret.GenerateQrCodeUri("NeoWallet", "user@domain.com");

        uri.Should().StartWith("otpauth://totp/NeoWallet:user%40domain.com?");
        uri.Should().Contain("secret=JBSWY3DPEHPK3PXP");
        uri.Should().Contain("issuer=NeoWallet");
        uri.Should().Contain("algorithm=SHA1");
        uri.Should().Contain("digits=6");
    }
}
