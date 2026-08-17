using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Features.Wallets.Queries.GetUserWallets;

public sealed record GetUserWalletsQuery(Guid OwnerId) : IQuery<IReadOnlyList<WalletSummaryDto>>;

public sealed class GetUserWalletsQueryValidator : AbstractValidator<GetUserWalletsQuery>
{
    public GetUserWalletsQueryValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithErrorCode("OwnerId.Empty").WithMessage("Owner ID is required.");
    }
}

public sealed class GetUserWalletsQueryHandler : IQueryHandler<GetUserWalletsQuery, IReadOnlyList<WalletSummaryDto>>
{
    private readonly IWalletReadService _walletReadService;

    public GetUserWalletsQueryHandler(IWalletReadService walletReadService)
    {
        _walletReadService = walletReadService;
    }

    public async Task<Result<IReadOnlyList<WalletSummaryDto>>> Handle(GetUserWalletsQuery request, CancellationToken cancellationToken)
    {
        return await _walletReadService.GetUserWalletsAsync(request.OwnerId, cancellationToken);
    }
}
