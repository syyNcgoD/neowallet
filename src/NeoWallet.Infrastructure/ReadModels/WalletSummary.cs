using NeoWallet.Domain.Enums;

namespace NeoWallet.Infrastructure.ReadModels;

/// <summary>
/// CQRS Read Model representing the current projected state of a wallet.
/// Optimized for high-throughput balance queries and dashboard views.
/// </summary>
public sealed class WalletSummary
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public WalletStatus Status { get; set; }
    public long Version { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
