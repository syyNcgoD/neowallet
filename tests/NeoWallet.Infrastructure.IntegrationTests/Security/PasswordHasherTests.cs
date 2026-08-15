using FluentAssertions;
using NeoWallet.Infrastructure.Authentication;

namespace NeoWallet.Infrastructure.IntegrationTests.Security;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void HashPassword_ShouldGenerateUniqueSaltAndHash()
    {
        var password = "SuperSecretPassword123!";

        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        hash1.Should().NotBeNullOrWhiteSpace();
        hash2.Should().NotBeNullOrWhiteSpace();
        hash1.Should().NotBe(hash2); // Different salts!
        hash1.Should().StartWith("$pbkdf2-sha512$i=100000$l=32$");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        var password = "P@ssw0rdSecureFintech!";
        var hash = _sut.HashPassword(password);

        var isValid = _sut.VerifyPassword(password, hash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        var password = "CorrectPassword123";
        var hash = _sut.HashPassword(password);

        var isValid = _sut.VerifyPassword("WrongPassword456", hash);

        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("pass", "")]
    [InlineData(null, "hash")]
    [InlineData("pass", null)]
    [InlineData("pass", "invalid_format_hash")]
    public void VerifyPassword_WithInvalidInputs_ShouldReturnFalse(string? pass, string? hash)
    {
        var isValid = _sut.VerifyPassword(pass!, hash!);
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void HashPassword_WithNullOrEmpty_ShouldThrowArgumentException(string? pass)
    {
        Action act = () => _sut.HashPassword(pass!);
        act.Should().Throw<ArgumentException>();
    }
}
