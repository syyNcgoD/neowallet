using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Services;

/// <summary>
/// Domain service responsible for executing peer-to-peer (P2P) transfers between two wallet aggregate roots.
/// </summary>
public interface ITransferService
{
    /// <summary>
    /// Executes a transfer from the source wallet to the target wallet enforcing domain invariants.
    /// </summary>
    /// <param name="sourceWallet">The wallet from which funds will be deducted.</param>
    /// <param name="targetWallet">The wallet to which funds will be credited.</param>
    /// <param name="transactionId">Unique transaction identifier for tracing.</param>
    /// <param name="amount">The monetary amount to transfer.</param>
    /// <param name="reference">Optional external reference or note.</param>
    /// <returns>A Result indicating success or the specific domain failure.</returns>
    Result Transfer(
        Wallet sourceWallet,
        Wallet targetWallet,
        TransactionId transactionId,
        Money amount,
        string? reference = null);
}
