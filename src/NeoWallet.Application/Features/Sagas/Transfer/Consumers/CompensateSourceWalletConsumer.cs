using MassTransit;
using NeoWallet.Application.Features.Sagas.Transfer.Contracts;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Sagas.Transfer.Consumers;

public sealed class CompensateSourceWalletConsumer : IConsumer<TransferCompensateSourceCommand>
{
    private readonly IWalletRepository _walletRepository;

    public CompensateSourceWalletConsumer(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task Consume(ConsumeContext<TransferCompensateSourceCommand> context)
    {
        var msg = context.Message;
        var walletIdResult = WalletId.From(msg.SourceWalletId);
        var currencyResult = Currency.FromCode(msg.Currency);

        if (walletIdResult.IsFailure || currencyResult.IsFailure)
        {
            await context.Publish(new TransferCompensatedEvent(msg.SagaId));
            return;
        }

        var moneyResult = Money.Create(msg.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
        {
            await context.Publish(new TransferCompensatedEvent(msg.SagaId));
            return;
        }

        var loadResult = await _walletRepository.LoadAsync(walletIdResult.Value, cancellationToken: context.CancellationToken);
        if (loadResult.IsFailure)
        {
            await context.Publish(new TransferCompensatedEvent(msg.SagaId));
            return;
        }

        var wallet = loadResult.Value;
        var txId = TransactionId.New();
        wallet.Deposit(txId, moneyResult.Value, "REFUND", msg.Reason);

        await _walletRepository.StoreAsync(wallet, context.CancellationToken);
        await context.Publish(new TransferCompensatedEvent(msg.SagaId));
    }
}
