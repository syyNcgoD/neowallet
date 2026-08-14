using Marten;
using Marten.Events.Projections;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.Projections;
public sealed class TransactionHistoryProjection : EventProjection
{
    public void Project(MoneyDeposited @event, IDocumentOperations ops)
    {
        ops.Store(new TransactionHistory
        {
            Id = @event.TransactionId.Value,
            WalletId = @event.WalletId.Value,
            Type = TransactionType.Deposit,
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency.Code,
            BalanceAfter = @event.BalanceAfter.Amount,
            RelatedWalletId = null,
            Reference = @event.Reference,
            Description = @event.Description,
            TimestampUtc = @event.DepositedAtUtc
        });
    }

    public void Project(MoneyWithdrawn @event, IDocumentOperations ops)
    {
        ops.Store(new TransactionHistory
        {
            Id = @event.TransactionId.Value,
            WalletId = @event.WalletId.Value,
            Type = TransactionType.Withdrawal,
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency.Code,
            BalanceAfter = @event.BalanceAfter.Amount,
            RelatedWalletId = null,
            Reference = @event.Reference,
            Description = @event.Description,
            TimestampUtc = @event.WithdrawnAtUtc
        });
    }

    public void Project(MoneyTransferredOut @event, IDocumentOperations ops)
    {
        ops.Store(new TransactionHistory
        {
            Id = @event.TransactionId.Value,
            WalletId = @event.SourceWalletId.Value,
            Type = TransactionType.TransferOut,
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency.Code,
            BalanceAfter = @event.BalanceAfter.Amount,
            RelatedWalletId = @event.TargetWalletId.Value,
            Reference = @event.Reference,
            Description = $"Transfer to {@event.TargetWalletId.Value}",
            TimestampUtc = @event.TransferredAtUtc
        });
    }

    public void Project(MoneyTransferredIn @event, IDocumentOperations ops)
    {
        ops.Store(new TransactionHistory
        {
            Id = @event.TransactionId.Value,
            WalletId = @event.TargetWalletId.Value,
            Type = TransactionType.TransferIn,
            Amount = @event.Amount.Amount,
            Currency = @event.Amount.Currency.Code,
            BalanceAfter = @event.BalanceAfter.Amount,
            RelatedWalletId = @event.SourceWalletId.Value,
            Reference = @event.Reference,
            Description = $"Transfer from {@event.SourceWalletId.Value}",
            TimestampUtc = @event.TransferredAtUtc
        });
    }
}
