using NeoWallet.Domain.Enums;

namespace NeoWallet.Infrastructure.ReadModels;

/// <summary>
/// CQRS Read Model representing an indexed historical transaction entry.
/// </summary>
public sealed class TransactionHistory
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal BalanceAfter { get; set; }
    public Guid? RelatedWalletId { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public DateTime TimestampUtc { get; set; }
}
