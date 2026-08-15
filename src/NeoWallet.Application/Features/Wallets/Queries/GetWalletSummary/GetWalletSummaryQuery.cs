using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Features.Wallets.Queries.GetWalletSummary;

public sealed record GetWalletSummaryQuery(Guid WalletId) : IQuery<WalletSummaryDto>;

public sealed class GetWalletSummaryQueryValidator : AbstractValidator<GetWalletSummaryQuery>
{
    public GetWalletSummaryQueryValidator()
    {
        RuleFor(x => x.WalletId)
            .NotEmpty().WithErrorCode("Wallet.EmptyId").WithMessage("Wallet ID is required.");
    }
}

public sealed class GetWalletSummaryQueryHandler : IQueryHandler<GetWalletSummaryQuery, WalletSummaryDto>
{
    private readonly IWalletReadService _walletReadService;

    public GetWalletSummaryQueryHandler(IWalletReadService walletReadService)
    {
        _walletReadService = walletReadService;
    }

    public async Task<Result<WalletSummaryDto>> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _walletReadService.GetWalletSummaryAsync(request.WalletId, cancellationToken);
    }
}
