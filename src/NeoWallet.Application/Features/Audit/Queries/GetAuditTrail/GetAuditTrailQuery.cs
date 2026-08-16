using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Audit;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Features.Audit.Queries.GetAuditTrail;

public sealed record GetAuditTrailQuery(Guid AggregateId) : IQuery<IReadOnlyList<AuditEntryDto>>;

public sealed class GetAuditTrailQueryValidator : AbstractValidator<GetAuditTrailQuery>
{
    public GetAuditTrailQueryValidator()
    {
        RuleFor(x => x.AggregateId)
            .NotEmpty().WithErrorCode("Audit.EmptyAggregateId").WithMessage("Aggregate ID is required.");
    }
}

public sealed class GetAuditTrailQueryHandler : IQueryHandler<GetAuditTrailQuery, IReadOnlyList<AuditEntryDto>>
{
    private readonly IAuditStore _auditStore;

    public GetAuditTrailQueryHandler(IAuditStore auditStore)
    {
        _auditStore = auditStore;
    }

    public async Task<Result<IReadOnlyList<AuditEntryDto>>> Handle(
        GetAuditTrailQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _auditStore.GetAuditTrailAsync(request.AggregateId, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<IReadOnlyList<AuditEntryDto>>(result.Error);
        }

        var dtos = result.Value.Select(e => new AuditEntryDto(
            e.Id,
            e.AggregateId,
            e.AggregateType,
            e.EventType,
            e.EventDataJson,
            e.PreviousHash,
            e.CurrentHash,
            e.SequenceNumber,
            e.TimestampUtc)).ToList();

        return Result.Success<IReadOnlyList<AuditEntryDto>>(dtos);
    }
}
