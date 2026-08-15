using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Wallets.Commands.CreateWallet;

public sealed record CreateWalletCommand(
    Guid OwnerId,
    string Currency) : ICommand<WalletDto>;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithErrorCode("Wallet.EmptyOwnerId").WithMessage("Owner ID is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithErrorCode("Wallet.EmptyCurrency").WithMessage("Currency code is required.")
            .Length(3).WithErrorCode("Wallet.InvalidCurrency").WithMessage("Currency code must be 3 characters.");
    }
}

public sealed class CreateWalletCommandHandler : ICommandHandler<CreateWalletCommand, WalletDto>
{
    private readonly IWalletRepository _walletRepository;

    public CreateWalletCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<WalletDto>> Handle(CreateWalletCommand request, CancellationToken cancellationToken)
    {
        var ownerIdResult = OwnerId.From(request.OwnerId);
        if (ownerIdResult.IsFailure)
        {
            return Result.Failure<WalletDto>(ownerIdResult.Error);
        }

        var currencyResult = Currency.FromCode(request.Currency);
        if (currencyResult.IsFailure)
        {
            return Result.Failure<WalletDto>(currencyResult.Error);
        }

        var walletId = WalletId.New();
        var walletResult = Wallet.Create(walletId, ownerIdResult.Value, currencyResult.Value);
        if (walletResult.IsFailure)
        {
            return Result.Failure<WalletDto>(walletResult.Error);
        }

        var storeResult = await _walletRepository.StoreAsync(walletResult.Value, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<WalletDto>(storeResult.Error);
        }

        var wallet = walletResult.Value;
        var dto = new WalletDto(
            wallet.Id.Value,
            wallet.OwnerId.Value,
            wallet.Balance.Amount,
            wallet.Currency.Code,
            wallet.Status,
            wallet.Version,
            wallet.CreatedAtUtc);

        return Result.Success(dto);
    }
}
