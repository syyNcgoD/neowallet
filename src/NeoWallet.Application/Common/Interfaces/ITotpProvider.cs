using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Common.Interfaces;

public interface ITotpProvider
{
    string GenerateCode(TotpSecret secret, DateTime? timestampUtc = null);
    bool VerifyCode(TotpSecret secret, string code, int toleranceSteps = 1, DateTime? timestampUtc = null);
}
