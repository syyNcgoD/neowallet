using MassTransit;

namespace NeoWallet.Application.Features.Sagas.Transfer;

public sealed class TransferState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public Guid SourceWalletId { get; set; }
    public Guid TargetWalletId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? FailureReason { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
