namespace NeoWallet.Application.DTOs.Audit;

public sealed record AuditEntryDto(
    Guid Id,
    Guid AggregateId,
    string AggregateType,
    string EventType,
    string EventDataJson,
    string PreviousHash,
    string CurrentHash,
    long SequenceNumber,
    DateTime TimestampUtc);

public sealed record ReconciliationReportDto(
    Guid Id,
    DateTime PeriodStartUtc,
    DateTime PeriodEndUtc,
    decimal TotalDeposits,
    decimal TotalWithdrawals,
    decimal TotalTransfers,
    decimal ExpectedBalanceSum,
    decimal ActualBalanceSum,
    decimal DiscrepancyAmount,
    bool HasDiscrepancy,
    IReadOnlyList<string> Discrepancies,
    DateTime GeneratedAtUtc);
