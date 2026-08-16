using MassTransit;
using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.Features.Sagas.Payment;

public sealed class DepositPaymentState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentGatewayProvider Provider { get; set; }
    public string? PaymentUrl { get; set; }
    public string? PaymentToken { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
