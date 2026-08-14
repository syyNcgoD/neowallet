using Marten.Events.Aggregation;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.Projections;

/// <summary>
/// Real-time Marten projection computing the WalletSummary read model from the wallet event stream.
/// </summary>
public sealed class WalletSummaryProjection : SingleStreamProjection<WalletSummary, Guid>
{
    public static WalletSummary Create(WalletCreated @event)
    {
        return new WalletSummary
        {
            Id = @event.WalletId.Value,
            OwnerId = @event.OwnerId.Value,
            Currency = @event.Currency.Code,
            Balance = 0m,
            Status = WalletStatus.Active,
            Version = 0,
            CreatedAtUtc = @event.CreatedAtUtc,
            LastModifiedAtUtc = @event.CreatedAtUtc
        };
    }

    public static void Apply(MoneyDeposited @event, WalletSummary current)
    {
        current.Balance = @event.BalanceAfter.Amount;
        current.LastModifiedAtUtc = @event.DepositedAtUtc;
        current.Version++;
    }

    public static void Apply(MoneyWithdrawn @event, WalletSummary current)
    {
        current.Balance = @event.BalanceAfter.Amount;
        current.LastModifiedAtUtc = @event.WithdrawnAtUtc;
        current.Version++;
    }

    public static void Apply(MoneyTransferredOut @event, WalletSummary current)
    {
        current.Balance = @event.BalanceAfter.Amount;
        current.LastModifiedAtUtc = @event.TransferredAtUtc;
        current.Version++;
    }

    public static void Apply(MoneyTransferredIn @event, WalletSummary current)
    {
        current.Balance = @event.BalanceAfter.Amount;
        current.LastModifiedAtUtc = @event.TransferredAtUtc;
        current.Version++;
    }

    public static void Apply(WalletLocked @event, WalletSummary current)
    {
        current.Status = WalletStatus.Locked;
        current.LastModifiedAtUtc = @event.LockedAtUtc;
        current.Version++;
    }

    public static void Apply(WalletUnlocked @event, WalletSummary current)
    {
        current.Status = WalletStatus.Active;
        current.LastModifiedAtUtc = @event.UnlockedAtUtc;
        current.Version++;
    }
}
