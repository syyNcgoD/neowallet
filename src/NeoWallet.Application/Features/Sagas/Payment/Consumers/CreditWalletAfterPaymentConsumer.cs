using MassTransit;
using NeoWallet.Application.Features.Sagas.Payment.Contracts;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Sagas.Payment.Consumers;

public sealed class CreditWalletAfterPaymentConsumer : IConsumer<CreditWalletAfterPaymentCommand>
{
    private readonly IWalletRepository _walletRepository;

    public CreditWalletAfterPaymentConsumer(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task Consume(ConsumeContext<CreditWalletAfterPaymentCommand> context)
    {
        var msg = context.Message;
        var walletIdResult = WalletId.From(msg.WalletId);
        var currencyResult = Currency.FromCode(msg.Currency);

        if (walletIdResult.IsFailure || currencyResult.IsFailure)
        {
            return;
        }

        var moneyResult = Money.Create(msg.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
        {
            return;
        }

        var loadResult = await _walletRepository.LoadAsync(walletIdResult.Value, cancellationToken: context.CancellationToken);
        if (loadResult.IsFailure)
        {
            return;
        }

        var wallet = loadResult.Value;
        var txId = TransactionId.New();
        wallet.Deposit(txId, moneyResult.Value, msg.Reference, "Deposit via payment gateway");

        await _walletRepository.StoreAsync(wallet, context.CancellationToken);
        await context.Publish(new PaymentWalletCreditedEvent(msg.SagaId, msg.WalletId, msg.Amount, msg.Currency));
    }
}
