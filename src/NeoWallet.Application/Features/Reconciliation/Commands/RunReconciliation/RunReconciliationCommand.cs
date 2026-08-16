using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Audit;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.Features.Reconciliation.Commands.RunReconciliation;

public sealed record RunReconciliationCommand(
    DateTime FromUtc,
    DateTime ToUtc) : ICommand<ReconciliationReportDto>;

public sealed class RunReconciliationCommandValidator : AbstractValidator<RunReconciliationCommand>
{
    public RunReconciliationCommandValidator()
    {
        RuleFor(x => x.FromUtc)
            .LessThanOrEqualTo(x => x.ToUtc)
            .WithErrorCode("Reconciliation.InvalidRange")
            .WithMessage("From date must be earlier than or equal to To date.");
    }
}

public sealed class RunReconciliationCommandHandler : ICommandHandler<RunReconciliationCommand, ReconciliationReportDto>
{
    private readonly IReconciliationService _reconciliationService;
    private readonly IDiscrepancyNotifier _discrepancyNotifier;

    public RunReconciliationCommandHandler(
        IReconciliationService reconciliationService,
        IDiscrepancyNotifier discrepancyNotifier)
    {
        _reconciliationService = reconciliationService;
        _discrepancyNotifier = discrepancyNotifier;
    }

    public async Task<Result<ReconciliationReportDto>> Handle(
        RunReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _reconciliationService.RunReconciliationAsync(
            request.FromUtc,
            request.ToUtc,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<ReconciliationReportDto>(result.Error);
        }

        var report = result.Value;

        if (report.HasDiscrepancy)
        {
            await _discrepancyNotifier.NotifyDiscrepancyAsync(report, cancellationToken);
        }

        var dto = new ReconciliationReportDto(
            report.Id,
            report.PeriodStartUtc,
            report.PeriodEndUtc,
            report.TotalDeposits,
            report.TotalWithdrawals,
            report.TotalTransfers,
            report.ExpectedBalanceSum,
            report.ActualBalanceSum,
            report.DiscrepancyAmount,
            report.HasDiscrepancy,
            report.Discrepancies,
            report.GeneratedAtUtc);

        return Result.Success(dto);
    }
}
