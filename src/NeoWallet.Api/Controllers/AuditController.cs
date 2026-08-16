using Microsoft.AspNetCore.Mvc;
using NeoWallet.Api.Common;
using NeoWallet.Application.Features.Audit.Queries.GetAuditTrail;
using NeoWallet.Application.Features.Audit.Queries.VerifyAuditChain;
using NeoWallet.Application.Features.Reconciliation.Commands.RunReconciliation;

namespace NeoWallet.Api.Controllers;

public sealed class AuditController : ApiController
{
    public sealed record ReconciliationRequest(DateTime FromUtc, DateTime ToUtc);

    [HttpGet("verify-chain")]
    public async Task<IActionResult> VerifyChain(CancellationToken ct)
    {
        var result = await Mediator.Send(new VerifyAuditChainQuery(), ct);
        return HandleResult(result);
    }

    [HttpGet("trail/{aggregateId:guid}")]
    public async Task<IActionResult> GetAuditTrail(Guid aggregateId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAuditTrailQuery(aggregateId), ct);
        return HandleResult(result);
    }

    [HttpPost("reconciliation")]
    public async Task<IActionResult> RunReconciliation([FromBody] ReconciliationRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RunReconciliationCommand(request.FromUtc, request.ToUtc), ct);
        return HandleResult(result);
    }
}
