using FluentAssertions;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.ValueObjects;

public sealed class StronglyTypedIdTests
{
    [Fact]
    public void WalletId_New_ShouldGenerateUniqueNonEmptyId()
    {
        // Act
        var id1 = WalletId.New();
        var id2 = WalletId.New();

        // Assert
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void WalletId_FromGuid_WithEmptyGuid_ShouldReturnFailure()
    {
        // Act
        var result = WalletId.From(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WalletId.Empty");
    }

    [Fact]
    public void WalletId_FromGuid_WithValidGuid_ShouldReturnSuccess()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = WalletId.From(guid);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
        ((Guid)result.Value).Should().Be(guid);
    }

    [Fact]
    public void WalletId_FromString_WithValidAndInvalidStrings_ShouldHandleProperly()
    {
        var guid = Guid.NewGuid();
        var validResult = WalletId.From(guid.ToString());
        var invalidResult = WalletId.From("not-a-guid");

        validResult.IsSuccess.Should().BeTrue();
        validResult.Value.Value.Should().Be(guid);

        invalidResult.IsFailure.Should().BeTrue();
        invalidResult.Error.Code.Should().Be("WalletId.InvalidFormat");
    }

    [Fact]
    public void TransactionId_New_ShouldGenerateUniqueNonEmptyId()
    {
        var id1 = TransactionId.New();
        var id2 = TransactionId.New();

        id1.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void TransactionId_From_ShouldValidateProperly()
    {
        var empty = TransactionId.From(Guid.Empty);
        empty.IsFailure.Should().BeTrue();

        var guid = Guid.NewGuid();
        var valid = TransactionId.From(guid.ToString());
        valid.IsSuccess.Should().BeTrue();
        valid.Value.Value.Should().Be(guid);

        var invalidStr = TransactionId.From("invalid");
        invalidStr.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void OwnerId_New_ShouldGenerateUniqueNonEmptyId()
    {
        var id1 = OwnerId.New();
        var id2 = OwnerId.New();

        id1.Value.Should().NotBe(Guid.Empty);
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void OwnerId_From_ShouldValidateProperly()
    {
        var empty = OwnerId.From(Guid.Empty);
        empty.IsFailure.Should().BeTrue();

        var guid = Guid.NewGuid();
        var valid = OwnerId.From(guid.ToString());
        valid.IsSuccess.Should().BeTrue();
        valid.Value.Value.Should().Be(guid);

        var invalidStr = OwnerId.From("invalid");
        invalidStr.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void StronglyTypedIds_ToStringAndCompareTo_ShouldWork()
    {
        var g1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var g2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        var w1 = WalletId.From(g1).Value;
        var w2 = WalletId.From(g2).Value;

        w1.ToString().Should().Be(g1.ToString());
        w1.CompareTo(w2).Should().BeNegative();

        var t1 = TransactionId.From(g1).Value;
        var t2 = TransactionId.From(g2).Value;
        t1.ToString().Should().Be(g1.ToString());
        t1.CompareTo(t2).Should().BeNegative();

        var o1 = OwnerId.From(g1).Value;
        var o2 = OwnerId.From(g2).Value;
        o1.ToString().Should().Be(g1.ToString());
        o1.CompareTo(o2).Should().BeNegative();
    }
}
