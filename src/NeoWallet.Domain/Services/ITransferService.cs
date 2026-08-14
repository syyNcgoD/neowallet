using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Services;
public interface ITransferService
{
    Result Transfer(
        Wallet sourceWallet,
        Wallet targetWallet,
        TransactionId transactionId,
        Money amount,
        string? reference = null);
}
