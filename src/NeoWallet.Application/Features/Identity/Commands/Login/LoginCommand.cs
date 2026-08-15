using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress = null) : ICommand<AuthResponseDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("Login.EmptyEmail").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("Login.InvalidEmail").WithMessage("Email is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("Login.EmptyPassword").WithMessage("Password is required.");
    }
}

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        var userResult = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        var user = userResult.Value;
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Value);
        if (!isPasswordValid)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        var ip = request.IpAddress ?? "127.0.0.1";

        // If 2FA is enabled, return partial auth response with temporary token requiring 2FA completion
        if (user.IsTwoFactorEnabled)
        {
            var tempToken = _jwtProvider.GenerateAccessToken(user, twoFactorVerified: false);
            return Result.Success(new AuthResponseDto(
                tempToken,
                string.Empty,
                5,
                true,
                new UserDto(user.Id.Value, user.Email.Value, user.Role, user.IsTwoFactorEnabled)));
        }

        // Standard 1-factor login
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
