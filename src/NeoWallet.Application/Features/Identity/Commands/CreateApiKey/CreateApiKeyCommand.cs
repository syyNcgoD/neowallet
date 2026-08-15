using FluentValidation;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;

namespace NeoWallet.Application.Features.Identity.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    Guid UserId,
    string Name,
    IReadOnlyList<string>? Permissions = null,
    DateTime? ExpiresAtUtc = null) : ICommand<ApiKeyDto>;

public sealed class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode("ApiKey.EmptyUserId").WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("ApiKey.EmptyName").WithMessage("API key name is required.")
            .MaximumLength(100).WithErrorCode("ApiKey.NameTooLong").WithMessage("API key name cannot exceed 100 characters.");
    }
}

public sealed class CreateApiKeyCommandHandler : ICommandHandler<CreateApiKeyCommand, ApiKeyDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IApiKeyService _apiKeyService;

    public CreateApiKeyCommandHandler(
        IUserRepository userRepository,
        IApiKeyService apiKeyService)
    {
        _userRepository = userRepository;
        _apiKeyService = apiKeyService;
    }

    public async Task<Result<ApiKeyDto>> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = OwnerId.From(request.UserId);
        if (userIdResult.IsFailure)
        {
            return Result.Failure<ApiKeyDto>(userIdResult.Error);
        }

        var loadResult = await _userRepository.LoadAsync(userIdResult.Value, cancellationToken: cancellationToken);
        if (loadResult.IsFailure)
        {
            return Result.Failure<ApiKeyDto>(loadResult.Error);
        }

        var user = loadResult.Value;
        var (plainTextKey, prefix, keyHash) = _apiKeyService.GenerateApiKey("live");
        var apiKeyId = Guid.NewGuid();

        var addKeyResult = user.AddApiKey(
            apiKeyId,
            request.Name,
            prefix,
            keyHash,
            request.Permissions,
            request.ExpiresAtUtc);

        if (addKeyResult.IsFailure)
        {
            return Result.Failure<ApiKeyDto>(addKeyResult.Error);
        }

        var storeResult = await _userRepository.StoreAsync(user, cancellationToken);
        if (storeResult.IsFailure)
        {
            return Result.Failure<ApiKeyDto>(storeResult.Error);
        }

        var apiKey = addKeyResult.Value;
        var dto = new ApiKeyDto(
            apiKey.Id,
            apiKey.Name,
            apiKey.Prefix,
            apiKey.Permissions,
            apiKey.CreatedAtUtc,
            apiKey.ExpiresAtUtc,
            apiKey.IsRevoked,
            plainTextKey);

        return Result.Success(dto);
    }
}
