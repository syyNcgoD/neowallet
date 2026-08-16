using MassTransit;
using Microsoft.AspNetCore.Mvc;
using NeoWallet.Api.Common;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Payment;
using NeoWallet.Application.Features.Sagas.Payment.Contracts;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;

namespace NeoWallet.Api.Controllers;

public sealed class PaymentsController : ApiController
{
    private readonly IBus _bus;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentsController(IBus bus, IPaymentGateway paymentGateway)
    {
        _bus = bus;
        _paymentGateway = paymentGateway;
    }

    public sealed record InitiatePaymentRequest(
        Guid WalletId,
        decimal Amount,
        string Currency,
        PaymentGatewayProvider Provider,
        string ReturnUrl);

    public sealed record VerifyPaymentRequest(
        Guid SagaId,
        Guid PaymentId,
        string PaymentToken);

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request, CancellationToken ct)
    {
        var sagaId = Guid.NewGuid();
        await _bus.Publish(new InitiateDepositPaymentSagaCommand(
            sagaId,
            request.WalletId,
            request.Amount,
            request.Currency,
            request.Provider,
            request.ReturnUrl), ct);

        return Ok(new { SagaId = sagaId, Status = "Initiated" });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentRequest request, CancellationToken ct)
    {
        var verifyResult = await _paymentGateway.VerifyAsync(
            new PaymentVerifyRequestDto(request.PaymentId, request.PaymentToken), ct);

        if (verifyResult.IsFailure)
        {
            return HandleResult(verifyResult);
        }

        if (verifyResult.Value.IsSuccess)
        {
            await _bus.Publish(new PaymentGatewayVerifiedEvent(
                request.SagaId,
                request.PaymentId,
                verifyResult.Value.ExternalTransactionId), ct);

            return Ok(new { Status = "Verified", ExternalTransactionId = verifyResult.Value.ExternalTransactionId });
        }

        await _bus.Publish(new PaymentGatewayFailedEvent(
            request.SagaId,
            verifyResult.Value.FailureReason ?? "Payment verification failed"), ct);

        return HandleResult(Result.Failure(Error.Conflict("Payment.Declined", verifyResult.Value.FailureReason ?? "Payment declined.")));
    }
}
