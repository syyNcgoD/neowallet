using FluentAssertions;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.UnitTests.Aggregates;

public sealed class PaymentTests
{
    private readonly PaymentId _paymentId = PaymentId.New();
    private readonly WalletId _walletId = WalletId.New();
    private readonly Money _amount = Money.Create(150m, Currency.USD).Value;

    [Fact]
    public void Initiate_WithValidParameters_ShouldCreatePaymentInPendingStatus()
    {
        var result = Payment.Initiate(
            _paymentId,
            _walletId,
            _amount,
            PaymentGatewayProvider.Mock,
            "EXT-REF-100",
            "https://app.neowallet.com/callback");

        result.IsSuccess.Should().BeTrue();
        var payment = result.Value;
        payment.Id.Should().Be(_paymentId);
        payment.WalletId.Should().Be(_walletId);
        payment.Amount.Should().Be(_amount);
        payment.Provider.Should().Be(PaymentGatewayProvider.Mock);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.ExternalReference.Should().Be("EXT-REF-100");
        payment.ReturnUrl.Should().Be("https://app.neowallet.com/callback");

        payment.UncommittedEvents.Should().HaveCount(1);
        var @event = payment.UncommittedEvents.Single().Should().BeOfType<PaymentInitiated>().Subject;
        @event.PaymentId.Should().Be(_paymentId);
    }

    [Fact]
    public void Initiate_WithInvalidParameters_ShouldReturnValidationFailure()
    {
        var r1 = Payment.Initiate(PaymentId.Empty, _walletId, _amount, PaymentGatewayProvider.Mock, "REF", "url");
        var r2 = Payment.Initiate(_paymentId, WalletId.Empty, _amount, PaymentGatewayProvider.Mock, "REF", "url");
        var r3 = Payment.Initiate(_paymentId, _walletId, null!, PaymentGatewayProvider.Mock, "REF", "url");
        var r4 = Payment.Initiate(_paymentId, _walletId, _amount, PaymentGatewayProvider.Mock, "", "url");

        r1.IsFailure.Should().BeTrue();
        r2.IsFailure.Should().BeTrue();
        r3.IsFailure.Should().BeTrue();
        r4.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void VerifyAndSettle_Lifecycle_ShouldTransitionStatesCorrectly()
    {
        var payment = Payment.Initiate(_paymentId, _walletId, _amount, PaymentGatewayProvider.Mock, "REF", "url").Value;

        // 1. Verify
        var verifyResult = payment.Verify("TXN-BANK-999");
        verifyResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.ExternalTransactionId.Should().Be("TXN-BANK-999");
        payment.VerifiedAtUtc.Should().NotBeNull();

        // 2. Settle
        var settleResult = payment.Settle();
        settleResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.SettledAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Fail_WhenPendingOrProcessing_ShouldTransitionToFailed()
    {
        var payment = Payment.Initiate(_paymentId, _walletId, _amount, PaymentGatewayProvider.Mock, "REF", "url").Value;

        var failResult = payment.Fail("Declined by card issuer");

        failResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.FailureReason.Should().Be("Declined by card issuer");
    }

    [Fact]
    public void EventSourcing_LoadFromHistory_ShouldReconstructPaymentState()
    {
        var history = new List<IDomainEvent>
        {
            PaymentInitiated.Create(_paymentId, _walletId, _amount, PaymentGatewayProvider.Stripe, "REF-STRIPE", "url"),
            PaymentVerified.Create(_paymentId, "ch_3M4L2K9"),
            PaymentSettled.Create(_paymentId, _walletId, _amount)
        };

        var rehydrated = (Payment)Activator.CreateInstance(typeof(Payment), nonPublic: true)!;
        rehydrated.LoadFromHistory(history);

        rehydrated.Id.Should().Be(_paymentId);
        rehydrated.WalletId.Should().Be(_walletId);
        rehydrated.Status.Should().Be(PaymentStatus.Completed);
        rehydrated.ExternalTransactionId.Should().Be("ch_3M4L2K9");
        rehydrated.UncommittedEvents.Should().BeEmpty();
    }
}
