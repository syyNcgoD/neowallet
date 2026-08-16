using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Payment;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;

namespace NeoWallet.Infrastructure.Gateways;

public sealed class MockPaymentGateway : IPaymentGateway
{
    public PaymentGatewayProvider Provider => PaymentGatewayProvider.Mock;

    public Task<Result<PaymentInitiateResultDto>> InitiateAsync(
        PaymentInitiateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            return Task.FromResult(Result.Failure<PaymentInitiateResultDto>(
                Error.Validation("Payment.InvalidAmount", "Amount must be strictly greater than zero.")));
        }

        var token = $"tok_mock_{Guid.NewGuid():N}";
        var paymentUrl = $"https://checkout.neowallet.com/pay/{token}?returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";

        var result = new PaymentInitiateResultDto(
            request.PaymentId,
            paymentUrl,
            token,
            PaymentStatus.Pending);

        return Task.FromResult(Result.Success(result));
    }

    public Task<Result<PaymentVerifyResultDto>> VerifyAsync(
        PaymentVerifyRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentToken))
        {
            return Task.FromResult(Result.Failure<PaymentVerifyResultDto>(
                Error.Validation("Payment.EmptyToken", "Payment token cannot be empty.")));
        }

        if (request.PaymentToken.EndsWith("_fail", StringComparison.OrdinalIgnoreCase))
        {
            var failedResult = new PaymentVerifyResultDto(
                request.PaymentId,
                false,
                string.Empty,
                "Payment was declined by the issuing bank.");

            return Task.FromResult(Result.Success(failedResult));
        }

        var successResult = new PaymentVerifyResultDto(
            request.PaymentId,
            true,
            $"txn_mock_{Guid.NewGuid():N}",
            null);

        return Task.FromResult(Result.Success(successResult));
    }
}
