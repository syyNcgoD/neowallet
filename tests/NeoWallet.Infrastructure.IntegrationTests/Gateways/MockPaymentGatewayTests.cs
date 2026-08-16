using FluentAssertions;
using NeoWallet.Application.DTOs.Payment;
using NeoWallet.Domain.Enums;
using NeoWallet.Infrastructure.Gateways;

namespace NeoWallet.Infrastructure.IntegrationTests.Gateways;

public sealed class MockPaymentGatewayTests
{
    private readonly MockPaymentGateway _sut = new();

    [Fact]
    public async Task InitiateAsync_WithValidAmount_ShouldReturnCheckoutUrlAndToken()
    {
        var paymentId = Guid.NewGuid();
        var request = new PaymentInitiateRequestDto(
            paymentId,
            Guid.NewGuid(),
            250m,
            "USD",
            PaymentGatewayProvider.Mock,
            "Deposit to wallet",
            "https://app.neowallet.com/callback");

        var result = await _sut.InitiateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().Be(paymentId);
        result.Value.PaymentToken.Should().StartWith("tok_mock_");
        result.Value.PaymentUrl.Should().Contain(result.Value.PaymentToken);
        result.Value.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public async Task InitiateAsync_WithInvalidAmount_ShouldReturnValidationFailure()
    {
        var request = new PaymentInitiateRequestDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0m,
            "USD",
            PaymentGatewayProvider.Mock,
            "Deposit",
            "https://app.neowallet.com/callback");

        var result = await _sut.InitiateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.InvalidAmount");
    }

    [Fact]
    public async Task VerifyAsync_WithStandardToken_ShouldReturnSuccessfulVerification()
    {
        var paymentId = Guid.NewGuid();
        var request = new PaymentVerifyRequestDto(paymentId, "tok_mock_12345abcdef");

        var result = await _sut.VerifyAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeTrue();
        result.Value.PaymentId.Should().Be(paymentId);
        result.Value.ExternalTransactionId.Should().StartWith("txn_mock_");
        result.Value.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task VerifyAsync_WithFailingToken_ShouldReturnDeclinedPaymentResult()
    {
        var paymentId = Guid.NewGuid();
        var request = new PaymentVerifyRequestDto(paymentId, "tok_mock_12345_fail");

        var result = await _sut.VerifyAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSuccess.Should().BeFalse();
        result.Value.FailureReason.Should().Be("Payment was declined by the issuing bank.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task VerifyAsync_WithNullOrEmptyToken_ShouldReturnValidationFailure(string? token)
    {
        var request = new PaymentVerifyRequestDto(Guid.NewGuid(), token!);

        var result = await _sut.VerifyAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Payment.EmptyToken");
    }
}
