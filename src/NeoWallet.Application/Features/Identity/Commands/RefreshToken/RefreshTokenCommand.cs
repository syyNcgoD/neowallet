using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    Guid UserId,
    string RefreshToken,
    string? IpAddress = null) : ICommand<AuthResponseDto>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("Refresh.EmptyUserId").WithMessage("User ID is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithErrorCode("Refresh.EmptyToken").WithMessage("Refresh token is required.");
    }
}

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = OwnerId.From(request.UserId);
        if (userIdResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(userIdResult.Error);
        }

        var loadResult = await _userRepository.LoadAsync(userIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidRefreshToken);
        }

        var user = loadResult.Value;
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();
        var ip = request.IpAddress ?? "127.0.0.1";

        var rotateResult = user.RotateRefreshToken(
            request.RefreshToken,
            newRefreshToken,
            DateTime.UtcNow.AddDays(7),
            ip);

        if (rotateResult.IsFailure)
        {
            // If reuse was detected, store user to persist revocation of all tokens!
            if (rotateResult.Error.Code == DomainErrors.Identity.RefreshTokenReused.Code)
            {
                await _userRepository.StoreAsync(user, cancellationToken);
            }
            return Result.Failure<AuthResponseDto>(rotateResult.Error);
        }

        var storeResult = await _userRepository.StoreAsync(user, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(storeResult.Error);
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user, twoFactorVerified: true);
        return Result.Success(new AuthResponseDto(
            accessToken,
            newRefreshToken,
            15,
            false,
            new UserDto(user.Id.Value, user.Email.Value, user.Role, user.IsTwoFactorEnabled)));
    }
}
