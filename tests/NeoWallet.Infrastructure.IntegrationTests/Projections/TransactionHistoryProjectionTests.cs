using FluentAssertions;
using Marten;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.Projections;
using NeoWallet.Infrastructure.ReadModels;
using NSubstitute;

namespace NeoWallet.Infrastructure.IntegrationTests.Projections;

public sealed class TransactionHistoryProjectionTests
{
    private readonly TransactionHistoryProjection _sut = new();
    private readonly WalletId _walletId = WalletId.New();
    private readonly Currency _currency = Currency.USD;

    [Fact]
    public void Project_MoneyDeposited_ShouldStoreTransactionHistoryDocument()
    {
        // Arrange
        var ops = Substitute.For<IDocumentOperations>();
        var txId = TransactionId.New();
        var amount = Money.Create(200m, _currency).Value;
        var depositEvent = MoneyDeposited.Create(_walletId, txId, amount, amount, "REF-1", "Salary deposit");

        // Act
        _sut.Project(depositEvent, ops);

        // Assert
        ops.Received(1).Store(Arg.Is<TransactionHistory>(doc =>
            doc.Id == txId.Value &&
            doc.WalletId == _walletId.Value &&
            doc.Type == TransactionType.Deposit &&
            doc.Amount == 200m &&
            doc.Currency == "USD" &&
            doc.BalanceAfter == 200m &&
            doc.Reference == "REF-1" &&
            doc.Description == "Salary deposit" &&
            doc.RelatedWalletId == null));
    }

    [Fact]
    public void Project_MoneyWithdrawn_ShouldStoreTransactionHistoryDocument()
    {
        // Arrange
        var ops = Substitute.For<IDocumentOperations>();
        var txId = TransactionId.New();
        var amount = Money.Create(50m, _currency).Value;
        var withdrawEvent = MoneyWithdrawn.Create(_walletId, txId, amount, Money.Create(150m, _currency).Value, "ATM", "ATM cash");

        // Act
        _sut.Project(withdrawEvent, ops);

        // Assert
        ops.Received(1).Store(Arg.Is<TransactionHistory>(doc =>
            doc.Id == txId.Value &&
            doc.WalletId == _walletId.Value &&
            doc.Type == TransactionType.Withdrawal &&
            doc.Amount == 50m &&
            doc.BalanceAfter == 150m));
    }

    [Fact]
    public void Project_MoneyTransferredOut_ShouldStoreTransactionHistoryWithRelatedWallet()
    {
        // Arrange
        var ops = Substitute.For<IDocumentOperations>();
        var txId = TransactionId.New();
        var targetWalletId = WalletId.New();
        var amount = Money.Create(80m, _currency).Value;
        var transferOut = MoneyTransferredOut.Create(_walletId, targetWalletId, txId, amount, Money.Create(70m, _currency).Value, "P2P");

        // Act
        _sut.Project(transferOut, ops);

        // Assert
        ops.Received(1).Store(Arg.Is<TransactionHistory>(doc =>
            doc.Id == txId.Value &&
            doc.WalletId == _walletId.Value &&
            doc.Type == TransactionType.TransferOut &&
            doc.RelatedWalletId == targetWalletId.Value &&
            doc.Amount == 80m &&
            doc.BalanceAfter == 70m));
    }

    [Fact]
    public void Project_MoneyTransferredIn_ShouldStoreTransactionHistoryWithRelatedWallet()
    {
        // Arrange
        var ops = Substitute.For<IDocumentOperations>();
        var txId = TransactionId.New();
        var sourceWalletId = WalletId.New();
        var amount = Money.Create(120m, _currency).Value;
        var transferIn = MoneyTransferredIn.Create(_walletId, sourceWalletId, txId, amount, Money.Create(190m, _currency).Value, "P2P-IN");

        // Act
        _sut.Project(transferIn, ops);

        // Assert
        ops.Received(1).Store(Arg.Is<TransactionHistory>(doc =>
            doc.Id == txId.Value &&
            doc.WalletId == _walletId.Value &&
            doc.Type == TransactionType.TransferIn &&
            doc.RelatedWalletId == sourceWalletId.Value &&
            doc.Amount == 120m &&
            doc.BalanceAfter == 190m));
    }
}
