using MassTransit;
using NeoWallet.Application.Features.Sagas.Transfer.Contracts;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Sagas.Transfer.Consumers;

public sealed class DeductSourceWalletConsumer : IConsumer<DeductSourceWalletCommand>
{
    private readonly IWalletRepository _walletRepository;

    public DeductSourceWalletConsumer(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task Consume(ConsumeContext<DeductSourceWalletCommand> context)
    {
        var msg = context.Message;
        var walletIdResult = WalletId.From(msg.SourceWalletId);
        var currencyResult = Currency.FromCode(msg.Currency);

        if (walletIdResult.IsFailure || currencyResult.IsFailure)
        {
            await context.Publish(new TransferSourceFailedEvent(msg.SagaId, "Invalid wallet ID or currency"));
            return;
        }

        var moneyResult = Money.Create(msg.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
        {
            await context.Publish(new TransferSourceFailedEvent(msg.SagaId, moneyResult.Error.Description));
            return;
        }

        var loadResult = await _walletRepository.LoadAsync(walletIdResult.Value, cancellationToken: context.CancellationToken);
        if (loadResult.IsFailure)
        {
            await context.Publish(new TransferSourceFailedEvent(msg.SagaId, loadResult.Error.Description));
            return;
        }

        var wallet = loadResult.Value;
        var txId = TransactionId.New();
        var withdrawResult = wallet.Withdraw(txId, moneyResult.Value, msg.Reference, "Transfer out");

        if (withdrawResult.IsFailure)
        {
            await context.Publish(new TransferSourceFailedEvent(msg.SagaId, withdrawResult.Error.Description));
            return;
        }

        var storeResult = await _walletRepository.StoreAsync(wallet, context.CancellationToken);
        if (storeResult.IsFailure)
        {
            await context.Publish(new TransferSourceFailedEvent(msg.SagaId, storeResult.Error.Description));
            return;
        }

        await context.Publish(new TransferSourceDeductedEvent(
            msg.SagaId,
            msg.SourceWalletId,
            Guid.Empty,
            msg.Amount,
            msg.Currency));
    }
}
