using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Events;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Domain.Aggregates;

public sealed class Payment : AggregateRoot<PaymentId>
{
    public WalletId WalletId { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public PaymentGatewayProvider Provider { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public string? ExternalTransactionId { get; private set; }
    public string ReturnUrl { get; private set; } = string.Empty;
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? VerifiedAtUtc { get; private set; }
    public DateTime? SettledAtUtc { get; private set; }

    private Payment()
    {
    }

    public static Result<Payment> Initiate(
        PaymentId id,
        WalletId walletId,
        Money amount,
        PaymentGatewayProvider provider,
        string externalReference,
        string returnUrl)
    {
        if (id.Value == Guid.Empty)
        {
            return Result.Failure<Payment>(Error.Validation("Payment.EmptyId", "Payment ID cannot be empty."));
        }

        if (walletId.Value == Guid.Empty)
        {
            return Result.Failure<Payment>(Error.Validation("Payment.EmptyWalletId", "Wallet ID cannot be empty."));
        }

        if (amount is null || amount.Amount <= 0)
        {
            return Result.Failure<Payment>(Error.Validation("Payment.InvalidAmount", "Payment amount must be greater than zero."));
        }

        if (string.IsNullOrWhiteSpace(externalReference))
        {
            return Result.Failure<Payment>(Error.Validation("Payment.EmptyReference", "External reference cannot be empty."));
        }

        var payment = new Payment();
        var @event = PaymentInitiated.Create(id, walletId, amount, provider, externalReference, returnUrl);
        payment.RaiseEvent(@event);

        return Result.Success(payment);
    }

    public Result Verify(string externalTransactionId)
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(Error.Conflict("Payment.InvalidStatus", $"Cannot verify payment in status '{Status}'."));
        }

        if (string.IsNullOrWhiteSpace(externalTransactionId))
        {
            return Result.Failure(Error.Validation("Payment.EmptyTxId", "External transaction ID is required."));
        }

        var @event = PaymentVerified.Create(Id, externalTransactionId);
        RaiseEvent(@event);

        return Result.Success();
    }

    public Result Settle()
    {
        if (Status != PaymentStatus.Processing)
        {
            return Result.Failure(Error.Conflict("Payment.InvalidStatus", $"Cannot settle payment in status '{Status}'."));
        }

        var @event = PaymentSettled.Create(Id, WalletId, Amount);
        RaiseEvent(@event);

        return Result.Success();
    }

    public Result Fail(string reason)
    {
        if (Status == PaymentStatus.Completed)
        {
            return Result.Failure(Error.Conflict("Payment.AlreadyCompleted", "Cannot fail an already completed payment."));
        }

        var @event = PaymentFailed.Create(Id, reason);
        RaiseEvent(@event);

        return Result.Success();
    }

    protected override void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case PaymentInitiated e:
                Id = e.PaymentId;
                WalletId = e.WalletId;
                Amount = e.Amount;
                Provider = e.Provider;
                Status = PaymentStatus.Pending;
                ExternalReference = e.ExternalReference;
                ReturnUrl = e.ReturnUrl;
                CreatedAtUtc = e.CreatedAtUtc;
                break;

            case PaymentVerified e:
                Status = PaymentStatus.Processing;
                ExternalTransactionId = e.ExternalTransactionId;
                VerifiedAtUtc = e.VerifiedAtUtc;
                break;

            case PaymentSettled e:
                Status = PaymentStatus.Completed;
                SettledAtUtc = e.SettledAtUtc;
                break;

            case PaymentFailed e:
                Status = PaymentStatus.Failed;
                FailureReason = e.Reason;
                break;
        }
    }
}
