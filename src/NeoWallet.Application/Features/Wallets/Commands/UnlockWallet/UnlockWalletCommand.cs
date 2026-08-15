using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Wallets.Commands.UnlockWallet;

public sealed record UnlockWalletCommand(
    Guid WalletId,
    string Reason) : ICommand;

public sealed class UnlockWalletCommandValidator : AbstractValidator<UnlockWalletCommand>
{
    public UnlockWalletCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithErrorCode("Unlock.EmptyWalletId").WithMessage("Wallet ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("Unlock.EmptyReason").WithMessage("Unlock reason is required.");
    }
}

public sealed class UnlockWalletCommandHandler : ICommandHandler<UnlockWalletCommand>
{
    private readonly IWalletRepository _walletRepository;

    public UnlockWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(UnlockWalletCommand request, CancellationToken cancellationToken)
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
        var unlockResult = wallet.Unlock(request.Reason);
        if (unlockResult.IsFailure)
        {
            return unlockResult;
        }

        return await _walletRepository.StoreAsync(wallet, cancellationToken);
    }
}
