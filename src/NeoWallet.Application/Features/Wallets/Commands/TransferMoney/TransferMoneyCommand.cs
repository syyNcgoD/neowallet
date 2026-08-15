using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.Services;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Wallets.Commands.TransferMoney;

public sealed record TransferMoneyCommand(
    Guid SourceWalletId,
    Guid TargetWalletId,
    decimal Amount,
    string Currency,
    string? Reference = null,
    string? Description = null,
    string? IdempotencyKey = null) : ICommand, IIdempotentCommand;

public sealed class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.SourceWalletId)
            .NotEmpty().WithErrorCode("Transfer.EmptySourceWalletId").WithMessage("Source wallet ID is required.");

        RuleFor(x => x.TargetWalletId)
            .NotEmpty().WithErrorCode("Transfer.EmptyTargetWalletId").WithMessage("Target wallet ID is required.")
            .NotEqual(x => x.SourceWalletId).WithErrorCode("Transfer.SameWallets").WithMessage("Source and target wallets cannot be the same.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithErrorCode("Transfer.InvalidAmount").WithMessage("Transfer amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithErrorCode("Transfer.EmptyCurrency").WithMessage("Currency is required.")
            .Length(3).WithErrorCode("Transfer.InvalidCurrency").WithMessage("Currency must be 3 characters.");
    }
}

public sealed class TransferMoneyCommandHandler : ICommandHandler<TransferMoneyCommand>
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransferService _transferService;

    public TransferMoneyCommandHandler(
        IWalletRepository walletRepository,
        ITransferService transferService)
    {
        _walletRepository = walletRepository;
        _transferService = transferService;
    }

    public async Task<Result> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        var sourceIdResult = WalletId.From(request.SourceWalletId);
        if (sourceIdResult.IsFailure)
        {
            return sourceIdResult;
        }

        var targetIdResult = WalletId.From(request.TargetWalletId);
        if (targetIdResult.IsFailure)
        {
            return targetIdResult;
        }

        var currencyResult = Currency.FromCode(request.Currency);
        if (currencyResult.IsFailure)
        {
            return currencyResult;
        }

        var moneyResult = Money.Create(request.Amount, currencyResult.Value);
        if (moneyResult.IsFailure)
        {
            return moneyResult;
        }

        var sourceLoad = await _walletRepository.LoadAsync(sourceIdResult.Value, cancellationToken: cancellationToken);
        if (sourceLoad.IsFailure)
        {
            return sourceLoad;
        }

        var targetLoad = await _walletRepository.LoadAsync(targetIdResult.Value, cancellationToken: cancellationToken);
        if (targetLoad.IsFailure)
        {
            return targetLoad;
        }

        var txId = TransactionId.New();
        var transferResult = _transferService.Transfer(
            sourceLoad.Value,
            targetLoad.Value,
            txId,
            moneyResult.Value,
            request.Reference);

        if (transferResult.IsFailure)
        {
            return transferResult;
        }

        var storeSource = await _walletRepository.StoreAsync(sourceLoad.Value, cancellationToken);
        if (storeSource.IsFailure)
        {
            return storeSource;
        }

        var storeTarget = await _walletRepository.StoreAsync(targetLoad.Value, cancellationToken);
        if (storeTarget.IsFailure)
        {
            return storeTarget;
        }

        return Result.Success();
    }
}
