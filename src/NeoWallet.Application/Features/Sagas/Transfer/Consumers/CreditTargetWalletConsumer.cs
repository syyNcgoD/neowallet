using MassTransit;
using NeoWallet.Application.Features.Sagas.Transfer.Contracts;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Sagas.Transfer.Consumers;

public sealed class CreditTargetWalletConsumer : IConsumer<CreditTargetWalletCommand>
{
    private readonly IWalletRepository _walletRepository;

    public CreditTargetWalletConsumer(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task Consume(ConsumeContext<CreditTargetWalletCommand> context)
    {
        var msg = context.Message;
        var walletIdResult = WalletId.From(msg.TargetWalletId);
        var currencyResult = Currency.FromCode(msg.Currency);

        if (walletIdResult.IsFailure || currencyResult.IsFailure)
        {
            await context.Publish(new TransferTargetFailedEvent(msg.SagaId, "Invalid target wallet ID or currency"));
            return;
        }

        var moneyResult = Money.Create(msg.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
        {
            await context.Publish(new TransferTargetFailedEvent(msg.SagaId, moneyResult.Error.Description));
            return;
        }

        var loadResult = await _walletRepository.LoadAsync(walletIdResult.Value, cancellationToken: context.CancellationToken);
        if (loadResult.IsFailure)
        {
            await context.Publish(new TransferTargetFailedEvent(msg.SagaId, loadResult.Error.Description));
            return;
        }

        var wallet = loadResult.Value;
        var txId = TransactionId.New();
        var depositResult = wallet.Deposit(txId, moneyResult.Value, msg.Reference, "Transfer in");

        if (depositResult.IsFailure)
        {
            await context.Publish(new TransferTargetFailedEvent(msg.SagaId, depositResult.Error.Description));
            return;
        }

        var storeResult = await _walletRepository.StoreAsync(wallet, context.CancellationToken);
        if (storeResult.IsFailure)
        {
            await context.Publish(new TransferTargetFailedEvent(msg.SagaId, storeResult.Error.Description));
            return;
        }

        await context.Publish(new TransferTargetCreditedEvent(
            msg.SagaId,
            msg.TargetWalletId,
            msg.Amount,
            msg.Currency));
    }
}
