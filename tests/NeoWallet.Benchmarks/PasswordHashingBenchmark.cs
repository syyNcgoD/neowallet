using BenchmarkDotNet.Attributes;
using NeoWallet.Infrastructure.Authentication;

namespace NeoWallet.Benchmarks;

[MemoryDiagnoser]
public class PasswordHashingBenchmark
{
    private readonly PasswordHasher _hasher = new();
    private const string SamplePassword = "P@ssword123SecureEnterpriseWallet!";
    private string _hash = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        _hash = _hasher.HashPassword(SamplePassword);
    }

    [Benchmark]
    public string HashPassword()
    {
        return _hasher.HashPassword(SamplePassword);
    }

    [Benchmark]
    public bool VerifyPassword()
    {
        return _hasher.VerifyPassword(SamplePassword, _hash);
    }
}
