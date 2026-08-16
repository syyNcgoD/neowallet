using FluentAssertions;
using NeoWallet.Domain.Entities;

namespace NeoWallet.Domain.UnitTests.Entities;

public sealed class AuditLogEntryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldProduceValidChainedHash()
    {
        var entryId = Guid.NewGuid();
        var aggregateId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var result = AuditLogEntry.Create(
            entryId,
            aggregateId,
            "Wallet",
            "MoneyDeposited",
            "{\"Amount\":100}",
            AuditLogEntry.GenesisHash,
            1,
            timestamp);

        result.IsSuccess.Should().BeTrue();
        var entry = result.Value;
        entry.Id.Should().Be(entryId);
        entry.AggregateId.Should().Be(aggregateId);
        entry.SequenceNumber.Should().Be(1);
        entry.PreviousHash.Should().Be(AuditLogEntry.GenesisHash);
        entry.CurrentHash.Should().NotBeNullOrWhiteSpace();
        entry.CurrentHash.Length.Should().Be(64); // SHA-256 hex length
    }

    [Fact]
    public void VerifyIntegrity_WhenDataAndPreviousHashAreIntact_ShouldReturnTrue()
    {
        var aggregateId = Guid.NewGuid();
        var timestamp1 = DateTime.UtcNow;
        var timestamp2 = timestamp1.AddSeconds(1);

        // Entry 1 (Genesis)
        var entry1 = AuditLogEntry.Create(
            Guid.NewGuid(),
            aggregateId,
            "Wallet",
            "WalletCreated",
            "{}",
            AuditLogEntry.GenesisHash,
            1,
            timestamp1).Value;

        // Entry 2 (Chained to Entry 1)
        var entry2 = AuditLogEntry.Create(
            Guid.NewGuid(),
            aggregateId,
            "Wallet",
            "MoneyDeposited",
            "{\"Amount\":500}",
            entry1.CurrentHash,
            2,
            timestamp2).Value;

        entry1.VerifyIntegrity(AuditLogEntry.GenesisHash).Should().BeTrue();
        entry2.VerifyIntegrity(entry1.CurrentHash).Should().BeTrue();
    }

    [Fact]
    public void VerifyIntegrity_WhenPreviousHashIsTampered_ShouldReturnFalse()
    {
        var entry = AuditLogEntry.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Wallet",
            "MoneyDeposited",
            "{\"Amount\":100}",
            AuditLogEntry.GenesisHash,
            1,
            DateTime.UtcNow).Value;

        var fakePreviousHash = "1111111111111111111111111111111111111111111111111111111111111111";

        entry.VerifyIntegrity(fakePreviousHash).Should().BeFalse();
    }

    [Fact]
    public void Create_WithInvalidParameters_ShouldReturnValidationFailure()
    {
        var r1 = AuditLogEntry.Create(Guid.Empty, Guid.NewGuid(), "Wallet", "Type", "{}", "", 1);
        var r2 = AuditLogEntry.Create(Guid.NewGuid(), Guid.Empty, "Wallet", "Type", "{}", "", 1);
        var r3 = AuditLogEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "", "Type", "{}", "", 1);
        var r4 = AuditLogEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "Wallet", "", "{}", "", 1);

        r1.IsFailure.Should().BeTrue();
        r2.IsFailure.Should().BeTrue();
        r3.IsFailure.Should().BeTrue();
        r4.IsFailure.Should().BeTrue();
    }
}
