using System.Security.Claims;
using NeoWallet.Domain.Aggregates;

namespace NeoWallet.Infrastructure.Authentication;

public interface IJwtProvider
{
    string GenerateAccessToken(User user, bool twoFactorVerified = false);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
