using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.DisableTwoFactor;

public sealed record DisableTwoFactorCommand(
    Guid UserId,
    string Code) : ICommand;

public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("TwoFactor.EmptyUserId").WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithErrorCode("TwoFactor.EmptyCode").WithMessage("2FA code is required.")
            .Length(6).WithErrorCode("TwoFactor.InvalidCodeLength").WithMessage("2FA code must be 6 digits.");
    }
}

public sealed class DisableTwoFactorCommandHandler : ICommandHandler<DisableTwoFactorCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ITotpProvider _totpProvider;

    public DisableTwoFactorCommandHandler(
        IUserRepository userRepository,
        ITotpProvider totpProvider)
    {
        _userRepository = userRepository;
        _totpProvider = totpProvider;
    }

    public async Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = OwnerId.From(request.UserId);
        if (userIdResult.IsFailure)
        {
            return userIdResult;
        }

        var loadResult = await _userRepository.LoadAsync(userIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return loadResult;
        }

        var user = loadResult.Value;
        if (!user.IsTwoFactorEnabled || user.TwoFactorSecret is null)
        {
            return Result.Failure(DomainErrors.Identity.TwoFactorNotEnabled);
        }

        var isCodeValid = _totpProvider.VerifyCode(user.TwoFactorSecret, request.Code);
        if (!isCodeValid)
        {
            return Result.Failure(DomainErrors.Identity.InvalidTwoFactorCode);
        }

        var disableResult = user.DisableTwoFactor();
        if (disableResult.IsFailure)
        {
            return disableResult;
        }

        return await _userRepository.StoreAsync(user, cancellationToken);
    }
}
