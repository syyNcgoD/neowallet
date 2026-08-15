using NeoWallet.Domain.Enums;

namespace NeoWallet.Application.DTOs.Identity;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInMinutes,
    bool RequiresTwoFactor,
    UserDto User);

public sealed record UserDto(
    Guid Id,
    string Email,
    UserRole Role,
    bool IsTwoFactorEnabled);

public sealed record TwoFactorSetupDto(
    string Secret,
    string QrCodeUri);

public sealed record ApiKeyDto(
    Guid Id,
    string Name,
    string Prefix,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    bool IsRevoked,
    string? PlainTextKey = null);
