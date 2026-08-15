using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Wallets.Commands.DepositMoney;

public sealed record DepositMoneyCommand(
    Guid WalletId,
    decimal Amount,
    string Currency,
    string? Reference = null,
    string? Description = null,
    string? IdempotencyKey = null) : ICommand<WalletDto>, IIdempotentCommand;

public sealed class DepositMoneyCommandValidator : AbstractValidator<DepositMoneyCommand>
{
    public DepositMoneyCommandValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithErrorCode("Deposit.EmptyWalletId").WithMessage("Wallet ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithErrorCode("Deposit.InvalidAmount").WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithErrorCode("Deposit.EmptyCurrency").WithMessage("Currency is required.")
            .Length(3).WithErrorCode("Deposit.InvalidCurrency").WithMessage("Currency must be 3 characters.");
    }
}

public sealed class DepositMoneyCommandHandler : ICommandHandler<DepositMoneyCommand, WalletDto>
{
    private readonly IWalletRepository _walletRepository;

    public DepositMoneyCommandHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result<WalletDto>> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        var walletIdResult = WalletId.From(request.WalletId);
        if (walletIdResult.IsFailure)
        {
            return Result.Failure<WalletDto>(walletIdResult.Error);
        }

        var currencyResult = Currency.FromCode(request.Currency);
        if (currencyResult.IsFailure)
        {
            return Result.Failure<WalletDto>(currencyResult.Error);
        }

        var moneyResult = Money.Create(request.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
        {
            return Result.Failure<WalletDto>(moneyResult.Error);
        }

        var loadResult = await _walletRepository.LoadAsync(walletIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return Result.Failure<WalletDto>(loadResult.Error);
        }

        var wallet = loadResult.Value;
        var txId = TransactionId.New();

        var depositResult = wallet.Deposit(txId, moneyResult.Value, request.Reference, request.Description);
        if (depositResult.IsFailure)
        {
            return Result.Failure<WalletDto>(depositResult.Error);
        }

        var storeResult = await _walletRepository.StoreAsync(wallet, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<WalletDto>(storeResult.Error);
        }

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
