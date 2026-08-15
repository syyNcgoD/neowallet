using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    UserRole Role = UserRole.Customer,
    string? IpAddress = null) : ICommand<AuthResponseDto>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("Register.EmptyEmail").WithMessage("Email is required.")
            .EmailAddress().WithErrorCode("Register.InvalidEmail").WithMessage("Email is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode("Register.EmptyPassword").WithMessage("Password is required.")
            .MinimumLength(8).WithErrorCode("Register.PasswordTooShort").WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithErrorCode("Register.PasswordMissingUpper").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithErrorCode("Register.PasswordMissingLower").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithErrorCode("Register.PasswordMissingDigit").WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\?\*\@\#\$\%\^\&\+\=]").WithErrorCode("Register.PasswordMissingSpecial").WithMessage("Password must contain at least one special character.");
    }
}

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(emailResult.Error);
        }

        var isUnique = await _userRepository.IsEmailUniqueAsync(emailResult.Value, cancellationToken);
        if (isUnique.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(isUnique.Error);
        }

        if (!isUnique.Value)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.EmailAlreadyInUse);
        }

        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var passwordHashResult = PasswordHash.Create(hashedPassword);
        if (passwordHashResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(passwordHashResult.Error);
        }

        var userId = OwnerId.New();
        var userResult = User.Create(userId, emailResult.Value, passwordHashResult.Value, request.Role);
        if (userResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(userResult.Error);
        }

        var user = userResult.Value;
        var refreshTokenString = _jwtProvider.GenerateRefreshToken();
        var ip = request.IpAddress ?? "127.0.0.1";
        var addTokenResult = user.AddRefreshToken(refreshTokenString, DateTime.UtcNow.AddDays(7), ip);
        if (addTokenResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(addTokenResult.Error);
        }

        var storeResult = await _userRepository.StoreAsync(user, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<AuthResponseDto>(storeResult.Error);
        }

        var accessToken = _jwtProvider.GenerateAccessToken(user, twoFactorVerified: true);
        var authResponse = new AuthResponseDto(
            accessToken,
            refreshTokenString,
            15,
            false,
            new UserDto(user.Id.Value, user.Email.Value, user.Role, user.IsTwoFactorEnabled));

        return Result.Success(authResponse);
    }
}
