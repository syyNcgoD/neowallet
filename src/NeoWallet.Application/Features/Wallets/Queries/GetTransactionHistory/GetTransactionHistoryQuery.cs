using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Features.Wallets.Queries.GetTransactionHistory;

public sealed record GetTransactionHistoryQuery(
    Guid WalletId,
    int Page = 1,
    int PageSize = 50) : IQuery<IReadOnlyList<TransactionHistoryDto>>;

public sealed class GetTransactionHistoryQueryValidator : AbstractValidator<GetTransactionHistoryQuery>
{
    public GetTransactionHistoryQueryValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithErrorCode("History.EmptyWalletId").WithMessage("Wallet ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithErrorCode("History.InvalidPage").WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithErrorCode("History.InvalidPageSize").WithMessage("Page size must be between 1 and 100.");
    }
}

public sealed class GetTransactionHistoryQueryHandler : IQueryHandler<GetTransactionHistoryQuery, IReadOnlyList<TransactionHistoryDto>>
{
    private readonly IWalletReadService _walletReadService;

    public GetTransactionHistoryQueryHandler(IWalletReadService walletReadService)
    {
        _walletReadService = walletReadService;
    }

    public async Task<Result<IReadOnlyList<TransactionHistoryDto>>> Handle(
        GetTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        return await _walletReadService.GetTransactionHistoryAsync(
            request.WalletId,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
