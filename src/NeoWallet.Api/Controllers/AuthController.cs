using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NeoWallet.Api.Common;
using NeoWallet.Application.Features.Identity.Commands.CreateApiKey;
using NeoWallet.Application.Features.Identity.Commands.DisableTwoFactor;
using NeoWallet.Application.Features.Identity.Commands.EnableTwoFactor;
using NeoWallet.Application.Features.Identity.Commands.Login;
using NeoWallet.Application.Features.Identity.Commands.RefreshToken;
using NeoWallet.Application.Features.Identity.Commands.RegisterUser;
using NeoWallet.Application.Features.Identity.Commands.RevokeApiKey;
using NeoWallet.Application.Features.Identity.Commands.VerifyTwoFactor;
using NeoWallet.Domain.Enums;

namespace NeoWallet.Api.Controllers;

[EnableRateLimiting("auth-limit")]
public sealed class AuthController : ApiController
{
    public sealed record RegisterRequest(string Email, string Password, UserRole Role = UserRole.Customer);
    public sealed record LoginRequest(string Email, string Password, string? TwoFactorCode = null);
    public sealed record RefreshTokenRequest(Guid UserId, string RefreshToken);
    public sealed record Verify2FARequest(Guid UserId, string Code);
    public sealed record Disable2FARequest(Guid UserId, string Code);
    public sealed record CreateApiKeyRequest(Guid UserId, string Name, IReadOnlyList<string> Scopes, DateTime? ExpiresAtUtc = null);
    public sealed record RevokeApiKeyRequest(Guid UserId, Guid ApiKeyId, string? Reason = null);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegisterUserCommand(request.Email, request.Password, request.Role), ct);
        return HandleResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new LoginCommand(request.Email, request.Password, request.TwoFactorCode), ct);
        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RefreshTokenCommand(request.UserId, request.RefreshToken), ct);
        return HandleResult(result);
    }

    [HttpPost("2fa/enable")]
    public async Task<IActionResult> Enable2FA([FromQuery] Guid userId, CancellationToken ct)
    {
        var result = await Mediator.Send(new EnableTwoFactorCommand(userId), ct);
        return HandleResult(result);
    }

    [HttpPost("2fa/verify")]
    public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new VerifyTwoFactorCommand(request.UserId, request.Code), ct);
        return HandleResult(result);
    }

    [HttpPost("2fa/disable")]
    public async Task<IActionResult> Disable2FA([FromBody] Disable2FARequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new DisableTwoFactorCommand(request.UserId, request.Code), ct);
        return HandleResult(result);
    }

    [HttpPost("api-keys")]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateApiKeyCommand(request.UserId, request.Name, request.Scopes, request.ExpiresAtUtc), ct);
        return HandleResult(result, StatusCodes.Status201Created);
    }

    [HttpDelete("api-keys")]
    public async Task<IActionResult> RevokeApiKey([FromBody] RevokeApiKeyRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RevokeApiKeyCommand(request.UserId, request.ApiKeyId, request.Reason ?? "Manual Revocation"), ct);
        return HandleResult(result);
    }
}
