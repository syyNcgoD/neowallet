using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoWallet.Application.Features.Reconciliation.Commands.RunReconciliation;

namespace NeoWallet.Application.Features.Reconciliation.Workers;

public sealed class ReconciliationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReconciliationWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public ReconciliationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ReconciliationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReconciliationWorker background service started.");

        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunReconciliationCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing periodic reconciliation.");
            }
        }

        _logger.LogInformation("ReconciliationWorker background service stopped.");
    }

    private async Task RunReconciliationCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var toUtc = DateTime.UtcNow;
        var fromUtc = toUtc.AddHours(-24);

        _logger.LogInformation("Triggering automatic reconciliation from {FromUtc} to {ToUtc}", fromUtc, toUtc);

        var result = await mediator.Send(new RunReconciliationCommand(fromUtc, toUtc), cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogError("Automatic reconciliation failed: {@Error}", result.Error);
        }
        else if (result.Value.HasDiscrepancy)
        {
            _logger.LogCritical(
                "FINANCIAL DISCREPANCY DETECTED! Expected: {Expected}, Actual: {Actual}, Discrepancy: {Discrepancy}",
                result.Value.ExpectedBalanceSum,
                result.Value.ActualBalanceSum,
                result.Value.DiscrepancyAmount);
        }
        else
        {
            _logger.LogInformation(
                "Reconciliation passed successfully. Total verified balance: {ActualSum}",
                result.Value.ActualBalanceSum);
        }
    }
}
