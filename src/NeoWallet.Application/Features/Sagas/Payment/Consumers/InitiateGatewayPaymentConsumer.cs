using MassTransit;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Payment;
using NeoWallet.Application.Features.Sagas.Payment.Contracts;

namespace NeoWallet.Application.Features.Sagas.Payment.Consumers;

public sealed class InitiateGatewayPaymentConsumer : IConsumer<InitiateDepositPaymentSagaCommand>
{
    private readonly IPaymentGateway _paymentGateway;

    public InitiateGatewayPaymentConsumer(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public async Task Consume(ConsumeContext<InitiateDepositPaymentSagaCommand> context)
    {
        var msg = context.Message;
        var req = new PaymentInitiateRequestDto(
            msg.SagaId,
            msg.WalletId,
            msg.Amount,
            msg.Currency,
            msg.Provider,
            "Wallet Deposit",
            msg.ReturnUrl);

        var initResult = await _paymentGateway.InitiateAsync(req, context.CancellationToken);

        if (initResult.IsFailure)
        {
            await context.Publish(new PaymentGatewayFailedEvent(msg.SagaId, initResult.Error.Description));
            return;
        }

        await context.Publish(new PaymentGatewayInitiatedEvent(
            msg.SagaId,
            initResult.Value.PaymentId,
            initResult.Value.PaymentUrl,
            initResult.Value.PaymentToken));
    }
}
