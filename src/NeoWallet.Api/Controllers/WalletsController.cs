using Microsoft.AspNetCore.Mvc;
using NeoWallet.Api.Common;
using NeoWallet.Application.Features.Wallets.Commands.CreateWallet;
using NeoWallet.Application.Features.Wallets.Commands.DepositMoney;
using NeoWallet.Application.Features.Wallets.Commands.LockWallet;
using NeoWallet.Application.Features.Wallets.Commands.TransferMoney;
using NeoWallet.Application.Features.Wallets.Commands.UnlockWallet;
using NeoWallet.Application.Features.Wallets.Commands.WithdrawMoney;
using NeoWallet.Application.Features.Wallets.Queries.GetTransactionHistory;
using NeoWallet.Application.Features.Wallets.Queries.GetWalletSummary;

namespace NeoWallet.Api.Controllers;

public sealed class WalletsController : ApiController
{
    public sealed record CreateWalletRequest(Guid OwnerId, string Currency);
    public sealed record DepositRequest(decimal Amount, string Currency, string? Reference = null, string? Description = null);
    public sealed record WithdrawRequest(decimal Amount, string Currency, string? Reference = null, string? Description = null);
    public sealed record TransferRequest(Guid TargetWalletId, decimal Amount, string Currency, string? Reference = null, string? Description = null);
    public sealed record StatusChangeRequest(string? Reason = null);

    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateWalletCommand(request.OwnerId, request.Currency), ct);
        return HandleResult(result, StatusCodes.Status201Created);
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWalletSummaryQuery(id), ct);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTransactionHistoryQuery(id, page, pageSize), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/deposit")]
    public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new DepositMoneyCommand(id, request.Amount, request.Currency, request.Reference, request.Description), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id, [FromBody] WithdrawRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new WithdrawMoneyCommand(id, request.Amount, request.Currency, request.Reference, request.Description), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<IActionResult> Transfer(Guid id, [FromBody] TransferRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new TransferMoneyCommand(id, request.TargetWalletId, request.Amount, request.Currency, request.Reference, request.Description), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id, [FromBody] StatusChangeRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new LockWalletCommand(id, request.Reason ?? "Manual Lock"), ct);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid id, [FromBody] StatusChangeRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UnlockWalletCommand(id, request.Reason ?? "Manual Unlock"), ct);
        return HandleResult(result);
    }
}
