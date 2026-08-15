using FluentAssertions;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.DTOs.Identity;
using NeoWallet.Application.Features.Identity.Commands.CreateApiKey;
using NeoWallet.Application.Features.Identity.Commands.DisableTwoFactor;
using NeoWallet.Application.Features.Identity.Commands.EnableTwoFactor;
using NeoWallet.Application.Features.Identity.Commands.Login;
using NeoWallet.Application.Features.Identity.Commands.RefreshToken;
using NeoWallet.Application.Features.Identity.Commands.RegisterUser;
using NeoWallet.Application.Features.Identity.Commands.RevokeApiKey;
using NeoWallet.Application.Features.Identity.Commands.VerifyTwoFactor;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.ValueObjects;
using NSubstitute;

namespace NeoWallet.Application.UnitTests.Features.Identity;

public sealed class IdentityCommandHandlersTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtProvider _jwtProvider = Substitute.For<IJwtProvider>();
    private readonly ITotpProvider _totpProvider = Substitute.For<ITotpProvider>();
    private readonly IApiKeyService _apiKeyService = Substitute.For<IApiKeyService>();

    public IdentityCommandHandlersTests()
    {
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashed_pass_123");
        _passwordHasher.VerifyPassword(Arg.Any<string>(), "hashed_pass_123").Returns(true);
        _jwtProvider.GenerateAccessToken(Arg.Any<User>(), Arg.Any<bool>()).Returns("jwt_access_token");
        _jwtProvider.GenerateRefreshToken().Returns("jwt_refresh_token");
        _userRepository.StoreAsync(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Success()));
    }

    [Fact]
    public async Task RegisterUser_WhenEmailIsUnique_ShouldCreateUserAndReturnAuthResponse()
    {
        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(true)));

        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _jwtProvider);
        var command = new RegisterUserCommand("newuser@domain.com", "Password@123", UserRole.Customer);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt_access_token");
        result.Value.RefreshToken.Should().Be("jwt_refresh_token");
        result.Value.User.Email.Should().Be("newuser@domain.com");
        await _userRepository.Received(1).StoreAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterUser_WhenEmailAlreadyExists_ShouldReturnConflictError()
    {
        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(false)));

        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _jwtProvider);
        var command = new RegisterUserCommand("existing@domain.com", "Password@123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.Identity.EmailAlreadyInUse.Code);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Without2FA_ShouldReturnFullAuthResponse()
    {
        var user = User.Create(
            OwnerId.New(),
            Email.Create("user@domain.com").Value,
            PasswordHash.Create("hashed_pass_123").Value,
            UserRole.Customer).Value;

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(user)));

        var handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtProvider);
        var command = new LoginCommand("user@domain.com", "Password@123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresTwoFactor.Should().BeFalse();
        result.Value.AccessToken.Should().Be("jwt_access_token");
        result.Value.RefreshToken.Should().Be("jwt_refresh_token");
    }

    [Fact]
    public async Task Login_With2FAEnabled_ShouldReturnRequiresTwoFactorResponse()
    {
        var user = User.Create(
            OwnerId.New(),
            Email.Create("user@domain.com").Value,
            PasswordHash.Create("hashed_pass_123").Value,
            UserRole.Customer).Value;
        user.EnableTwoFactor(TotpSecret.Generate());

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(user)));

        var handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtProvider);
        var command = new LoginCommand("user@domain.com", "Password@123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiresTwoFactor.Should().BeTrue();
        result.Value.RefreshToken.Should().BeEmpty(); // No refresh token issued until 2FA completes!
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldRotateAndReturnNewTokens()
    {
        var userId = OwnerId.New();
        var user = User.Create(
            userId,
            Email.Create("user@domain.com").Value,
            PasswordHash.Create("hashed_pass_123").Value,
            UserRole.Customer).Value;
        user.AddRefreshToken("old_refresh_tok", DateTime.UtcNow.AddDays(7), "1.1.1.1");

        _userRepository.LoadAsync(userId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(user)));

        var handler = new RefreshTokenCommandHandler(_userRepository, _jwtProvider);
        var command = new RefreshTokenCommand(userId.Value, "old_refresh_tok");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RefreshToken.Should().Be("jwt_refresh_token");
        await _userRepository.Received(1).StoreAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TwoFactor_Lifecycle_Handlers_ShouldWorkSeamlessly()
    {
        var userId = OwnerId.New();
        var user = User.Create(
            userId,
            Email.Create("user@domain.com").Value,
            PasswordHash.Create("hashed_pass_123").Value,
            UserRole.Customer).Value;

        _userRepository.LoadAsync(userId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(user)));
        _totpProvider.VerifyCode(Arg.Any<TotpSecret>(), "123456", Arg.Any<int>(), Arg.Any<DateTime?>()).Returns(true);

        // 1. Enable
        var enableHandler = new EnableTwoFactorCommandHandler(_userRepository);
        var enableResult = await enableHandler.Handle(new EnableTwoFactorCommand(userId.Value), CancellationToken.None);
        enableResult.IsSuccess.Should().BeTrue();
        enableResult.Value.Secret.Should().NotBeNullOrWhiteSpace();

        // 2. Verify
        var verifyHandler = new VerifyTwoFactorCommandHandler(_userRepository, _totpProvider, _jwtProvider);
        var verifyResult = await verifyHandler.Handle(new VerifyTwoFactorCommand(userId.Value, "123456"), CancellationToken.None);
        verifyResult.IsSuccess.Should().BeTrue();
        verifyResult.Value.AccessToken.Should().Be("jwt_access_token");

        // 3. Disable
        var disableHandler = new DisableTwoFactorCommandHandler(_userRepository, _totpProvider);
        var disableResult = await disableHandler.Handle(new DisableTwoFactorCommand(userId.Value, "123456"), CancellationToken.None);
        disableResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApiKey_CreateAndRevoke_Handlers_ShouldWorkProperly()
    {
        var userId = OwnerId.New();
        var user = User.Create(
            userId,
            Email.Create("merchant@domain.com").Value,
            PasswordHash.Create("hashed_pass_123").Value,
            UserRole.Merchant).Value;

        _userRepository.LoadAsync(userId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(user)));
        _apiKeyService.GenerateApiKey("live").Returns(("nw_live_full_secret_key", "nw_live_pfx", "hash_sha256"));

        var createHandler = new CreateApiKeyCommandHandler(_userRepository, _apiKeyService);
        var createResult = await createHandler.Handle(new CreateApiKeyCommand(userId.Value, "My Integration Key", ["wallets:read"]), CancellationToken.None);

        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.PlainTextKey.Should().Be("nw_live_full_secret_key");
        var apiKeyId = createResult.Value.Id;

        // Revoke
        var revokeHandler = new RevokeApiKeyCommandHandler(_userRepository);
        var revokeResult = await revokeHandler.Handle(new RevokeApiKeyCommand(userId.Value, apiKeyId, "Rotated key"), CancellationToken.None);
        revokeResult.IsSuccess.Should().BeTrue();
    }
}
