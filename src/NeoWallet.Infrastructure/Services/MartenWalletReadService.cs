using Marten;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.ValueObjects;
using NeoWallet.Infrastructure.ReadModels;

namespace NeoWallet.Infrastructure.Services;

public sealed class MartenWalletReadService : IWalletReadService
{
    private readonly IQuerySession _session;

    public MartenWalletReadService(IQuerySession session)
    {
        _session = session;
    }

    public async Task<Result<WalletSummaryDto>> GetWalletSummaryAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        if (walletId == Guid.Empty)
        {
            return Result.Failure<WalletSummaryDto>(Error.Validation("Wallet.EmptyId", "Wallet ID cannot be empty."));
        }

        var summary = await _session.LoadAsync<WalletSummary>(walletId, cancellationToken);
        if (summary is null)
        {
            return Result.Failure<WalletSummaryDto>(DomainErrors.Wallet.NotFound(WalletId.From(walletId).Value));
        }

        var dto = new WalletSummaryDto(
            summary.Id,
            summary.OwnerId,
            summary.Balance,
            summary.Currency,
            summary.Status,
            summary.Version,
            summary.LastModifiedAtUtc);

        return Result.Success(dto);
    }

    public async Task<Result<IReadOnlyList<TransactionHistoryDto>>> GetTransactionHistoryAsync(
        Guid walletId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (walletId == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<TransactionHistoryDto>>(Error.Validation("Wallet.EmptyId", "Wallet ID cannot be empty."));
        }

        var query = await _session.Query<TransactionHistory>()
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = query.Select(t => new TransactionHistoryDto(
            t.Id,
            t.WalletId,
            t.Type,
            t.Amount,
            t.Currency,
            t.BalanceAfter,
            t.Reference,
            t.Description,
            t.RelatedWalletId,
            t.TimestampUtc)).ToList();

        return Result.Success<IReadOnlyList<TransactionHistoryDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<WalletSummaryDto>>> GetUserWalletsAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        if (ownerId == Guid.Empty)
        {
            return Result.Failure<IReadOnlyList<WalletSummaryDto>>(Error.Validation("OwnerId.Empty", "Owner ID cannot be empty."));
        }

        var summaries = await _session.Query<WalletSummary>()
            .Where(w => w.OwnerId == ownerId)
            .ToListAsync(cancellationToken);

        var dtos = summaries.Select(s => new WalletSummaryDto(
            s.Id,
            s.OwnerId,
            s.Balance,
            s.Currency,
            s.Status,
            s.Version,
            s.LastModifiedAtUtc)).ToList();

        return Result.Success<IReadOnlyList<WalletSummaryDto>>(dtos);
    }
}
