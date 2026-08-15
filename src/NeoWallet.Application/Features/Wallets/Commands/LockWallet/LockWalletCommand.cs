using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Wallets.Commands.LockWallet;

public sealed record LockWalletCommand(
    Guid WalletId,
    string Reason) : ICommand;

public sealed class LockWalletCommandValidator : AbstractValidator<LockWalletCommand>
{
    public LockWalletCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithErrorCode("Lock.EmptyWalletId").WithMessage("Wallet ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("Lock.EmptyReason").WithMessage("Lock reason is required.");
    }
}

public sealed class LockWalletCommandHandler : ICommandHandler<LockWalletCommand>
{
    private readonly IWalletRepository _walletRepository;

    public LockWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(LockWalletCommand request, CancellationToken cancellationToken)
    {
        var walletIdResult = WalletId.From(request.WalletId);
        if (walletIdResult.IsFailure)
        {
            return walletIdResult;
        }

        var loadResult = await _walletRepository.LoadAsync(walletIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return loadResult;
        }

        var wallet = loadResult.Value;
        var lockResult = wallet.Lock(request.Reason);
        if (lockResult.IsFailure)
        {
            return lockResult;
        }

        return await _walletRepository.StoreAsync(wallet, cancellationToken);
    }
}
