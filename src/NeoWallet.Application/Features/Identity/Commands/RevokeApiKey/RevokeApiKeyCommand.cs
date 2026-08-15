using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand(
    Guid UserId,
    Guid ApiKeyId,
    string Reason) : ICommand;

public sealed class RevokeApiKeyCommandValidator : AbstractValidator<RevokeApiKeyCommand>
{
    public RevokeApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("ApiKey.EmptyUserId").WithMessage("User ID is required.");

        RuleFor(x => x.ApiKeyId)
            .NotEmpty().WithErrorCode("ApiKey.EmptyApiKeyId").WithMessage("API key ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithErrorCode("ApiKey.EmptyReason").WithMessage("Revocation reason is required.");
    }
}

public sealed class RevokeApiKeyCommandHandler : ICommandHandler<RevokeApiKeyCommand>
{
    private readonly IUserRepository _userRepository;

    public RevokeApiKeyCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
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
        var revokeResult = user.RevokeApiKey(request.ApiKeyId, request.Reason);
        if (revokeResult.IsFailure)
        {
            return revokeResult;
        }

        return await _userRepository.StoreAsync(user, cancellationToken);
    }
}
