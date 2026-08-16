using BenchmarkDotNet.Attributes;
using NeoWallet.Domain.Entities;

namespace NeoWallet.Benchmarks;

[MemoryDiagnoser]
public class AuditHashChainingBenchmark
{
    private readonly Guid _aggregateId = Guid.NewGuid();
    private const string Payload = "{\"Amount\":250.00,\"Currency\":\"USD\",\"TransactionType\":\"Deposit\"}";

    [Benchmark]
    public string ComputeAuditHash()
    {
        return AuditLogEntry.ComputeHash(
            AuditLogEntry.GenesisHash,
            _aggregateId,
            "Wallet",
            "MoneyDeposited",
            Payload,
            1,
            DateTime.UtcNow);
    }
}
