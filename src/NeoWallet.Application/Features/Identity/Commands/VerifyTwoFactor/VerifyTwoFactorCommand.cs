using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.VerifyTwoFactor;

public sealed record VerifyTwoFactorCommand(
    Guid UserId,
    string Code,
    string? IpAddress = null) : ICommand<AuthResponseDto>;

public sealed class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("TwoFactor.EmptyUserId").WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("TwoFactor.EmptyCode").WithMessage("2FA code is required.")
            .Length(6).WithErrorCode("TwoFactor.InvalidCodeLength").WithMessage("2FA code must be 6 digits.")
            .Matches(@"^[0-9]{6}$").WithErrorCode("TwoFactor.InvalidCodeFormat").WithMessage("2FA code must be numeric.");
    }
}

public sealed class VerifyTwoFactorCommandHandler : ICommandHandler<VerifyTwoFactorCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITotpProvider _totpProvider;
    private readonly IJwtProvider _jwtProvider;

    public VerifyTwoFactorCommandHandler(
        IUserRepository userRepository,
        ITotpProvider totpProvider,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _totpProvider = totpProvider;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthResponseDto>> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = OwnerId.From(request.UserId);
        if (userIdResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(userIdResult.Error);
        }

        var loadResult = await _userRepository.LoadAsync(userIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(loadResult.Error);
        }

        var user = loadResult.Value;
        if (!user.IsTwoFactorEnabled || user.TwoFactorSecret is null)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.TwoFactorNotEnabled);
        }

        var isCodeValid = _totpProvider.VerifyCode(user.TwoFactorSecret, request.Code);
        if (!isCodeValid)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidTwoFactorCode);
        }

        var ip = request.IpAddress ?? "127.0.0.1";
        var refreshTokenString = _jwtProvider.GenerateRefreshToken();
        var addToken = user.AddRefreshToken(refreshTokenString, DateTime.UtcNow.AddDays(7), ip);
        if (addToken.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(addToken.Error);
        }

        var storeResult = await _userRepository.StoreAsync(user, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(storeResult.Error);
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user, twoFactorVerified: true);
        return Result.Success(new AuthResponseDto(
            accessToken,
            refreshTokenString,
            15,
            false,
            new UserDto(user.Id.Value, user.Email.Value, user.Role, user.IsTwoFactorEnabled)));
    }
}
