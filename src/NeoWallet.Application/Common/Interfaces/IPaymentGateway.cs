using NeoWallet.Application.DTOs.Payment;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.Common.Interfaces;

public interface IPaymentGateway
{
    PaymentGatewayProvider Provider { get; }
    Task<Result<PaymentInitiateResultDto>> InitiateAsync(PaymentInitiateRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<PaymentVerifyResultDto>> VerifyAsync(PaymentVerifyRequestDto request, CancellationToken cancellationToken = default);
}
