using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.EnableTwoFactor;

public sealed record EnableTwoFactorCommand(Guid UserId) : ICommand<TwoFactorSetupDto>;

public sealed class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("TwoFactor.EmptyUserId").WithMessage("User ID is required.");
    }
}

public sealed class EnableTwoFactorCommandHandler : ICommandHandler<EnableTwoFactorCommand, TwoFactorSetupDto>
{
    private readonly IUserRepository _userRepository;

    public EnableTwoFactorCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<TwoFactorSetupDto>> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = OwnerId.From(request.UserId);
        if (userIdResult.IsFailure)
        {
            return Result.Failure<TwoFactorSetupDto>(userIdResult.Error);
        }

        var loadResult = await _userRepository.LoadAsync(userIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return Result.Failure<TwoFactorSetupDto>(loadResult.Error);
        }

        var user = loadResult.Value;
        var secret = TotpSecret.Generate();

        var enableResult = user.EnableTwoFactor(secret);
        if (enableResult.IsFailure)
        {
            return Result.Failure<TwoFactorSetupDto>(enableResult.Error);
        }

        var storeResult = await _userRepository.StoreAsync(user, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<TwoFactorSetupDto>(storeResult.Error);
        }

        var qrCodeUri = secret.GenerateQrCodeUri("NeoWallet", user.Email.Value);
        var dto = new TwoFactorSetupDto(secret.Value, qrCodeUri);

        return Result.Success(dto);
    }
}
