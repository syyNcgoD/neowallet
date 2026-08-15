using FluentAssertions;
using NeoWallet.Infrastructure.Authentication;

namespace NeoWallet.Infrastructure.IntegrationTests.Security;

public sealed class ApiKeyServiceTests
{
    private readonly ApiKeyService _sut = new();

    [Fact]
    public void GenerateApiKey_ShouldReturnKeyPrefixAndConsistentHash()
    {
        var (plainText, prefix, hash) = _sut.GenerateApiKey("live");

        plainText.Should().StartWith("nw_live_");
        prefix.Should().StartWith("nw_live_");
        hash.Should().HaveLength(64); // SHA-256 hex string

        var recomputedHash = _sut.HashApiKey(plainText);
        recomputedHash.Should().Be(hash);
    }

    [Fact]
    public void ExtractPrefix_WithValidApiKey_ShouldExtractPrefixCorrectly()
    {
        var (plainText, prefix, _) = _sut.GenerateApiKey("test");

        var extracted = _sut.ExtractPrefix(plainText);

        extracted.Should().Be(prefix);
        extracted.Should().StartWith("nw_test_");
    }

    [Theory]
    [InlineData("invalid_key")]
    [InlineData("other_live_1234")]
    [InlineData("")]
    [InlineData(null)]
    public void ExtractPrefix_WithInvalidKey_ShouldReturnNull(string? key)
    {
        var extracted = _sut.ExtractPrefix(key!);
        extracted.Should().BeNull();
    }
}
