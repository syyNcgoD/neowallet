using FluentAssertions;
using NeoWallet.Application.Features.Wallets.Commands.CreateWallet;
using NeoWallet.Application.Features.Wallets.Commands.DepositMoney;
using NeoWallet.Application.Features.Wallets.Commands.LockWallet;
using NeoWallet.Application.Features.Wallets.Commands.TransferMoney;
using NeoWallet.Application.Features.Wallets.Commands.UnlockWallet;
using NeoWallet.Application.Features.Wallets.Commands.WithdrawMoney;
using NeoWallet.Domain.Aggregates;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NeoWallet.Domain.Errors;
using NeoWallet.Domain.Repositories;
using NeoWallet.Domain.Services;
using NeoWallet.Domain.ValueObjects;
using NSubstitute;

namespace NeoWallet.Application.UnitTests.Features.Wallets;

public sealed class WalletCommandHandlersTests
{
    private readonly IWalletRepository _walletRepository = Substitute.For<IWalletRepository>();
    private readonly ITransferService _transferService = new TransferService();

    [Fact]
    public async Task CreateWallet_WithValidData_ShouldPersistAndReturnDto()
    {
        var handler = new CreateWalletCommandHandler(_walletRepository);
        _walletRepository.StoreAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var command = new CreateWalletCommand(Guid.NewGuid(), "USD");
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Currency.Should().Be("USD");
        result.Value.Balance.Should().Be(0);
        result.Value.Status.Should().Be(WalletStatus.Active);
        await _walletRepository.Received(1).StoreAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DepositMoney_WithValidCommand_ShouldIncreaseBalanceAndReturnDto()
    {
        var walletId = WalletId.New();
        var wallet = Wallet.Create(walletId, OwnerId.New(), Currency.USD).Value;

        _walletRepository.LoadAsync(walletId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(wallet)));
        _walletRepository.StoreAsync(wallet, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var handler = new DepositMoneyCommandHandler(_walletRepository);
        var command = new DepositMoneyCommand(walletId.Value, 250m, "USD", "DEP-1", "Bonus");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(250m);
        await _walletRepository.Received(1).StoreAsync(wallet, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WithdrawMoney_WithSufficientBalance_ShouldDecreaseBalance()
    {
        var walletId = WalletId.New();
        var wallet = Wallet.Create(walletId, OwnerId.New(), Currency.USD).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(500m, Currency.USD).Value);

        _walletRepository.LoadAsync(walletId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(wallet)));
        _walletRepository.StoreAsync(wallet, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var handler = new WithdrawMoneyCommandHandler(_walletRepository);
        var command = new WithdrawMoneyCommand(walletId.Value, 150m, "USD", "ATM", "Cash");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Balance.Should().Be(350m);
    }

    [Fact]
    public async Task WithdrawMoney_WithInsufficientBalance_ShouldReturnInsufficientFundsFailure()
    {
        var walletId = WalletId.New();
        var wallet = Wallet.Create(walletId, OwnerId.New(), Currency.USD).Value;
        wallet.Deposit(TransactionId.New(), Money.Create(50m, Currency.USD).Value);

        _walletRepository.LoadAsync(walletId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(wallet)));

        var handler = new WithdrawMoneyCommandHandler(_walletRepository);
        var command = new WithdrawMoneyCommand(walletId.Value, 150m, "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.Wallet.InsufficientFunds.Code);
    }

    [Fact]
    public async Task TransferMoney_BetweenTwoActiveWallets_ShouldExecuteAndStoreBoth()
    {
        var sourceId = WalletId.New();
        var targetId = WalletId.New();
        var source = Wallet.Create(sourceId, OwnerId.New(), Currency.USD).Value;
        var target = Wallet.Create(targetId, OwnerId.New(), Currency.USD).Value;
        source.Deposit(TransactionId.New(), Money.Create(1000m, Currency.USD).Value);

        _walletRepository.LoadAsync(sourceId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(source)));
        _walletRepository.LoadAsync(targetId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(target)));
        _walletRepository.StoreAsync(Arg.Any<Wallet>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var handler = new TransferMoneyCommandHandler(_walletRepository, _transferService);
        var command = new TransferMoneyCommand(sourceId.Value, targetId.Value, 300m, "USD", "P2P", "Rent");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        source.Balance.Amount.Should().Be(700m);
        target.Balance.Amount.Should().Be(300m);
        await _walletRepository.Received(1).StoreAsync(source, Arg.Any<CancellationToken>());
        await _walletRepository.Received(1).StoreAsync(target, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LockAndUnlock_ShouldUpdateWalletStatus()
    {
        var walletId = WalletId.New();
        var wallet = Wallet.Create(walletId, OwnerId.New(), Currency.USD).Value;

        _walletRepository.LoadAsync(walletId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(wallet)));
        _walletRepository.StoreAsync(wallet, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success()));

        var lockHandler = new LockWalletCommandHandler(_walletRepository);
        var unlockHandler = new UnlockWalletCommandHandler(_walletRepository);

        // Lock
        var lockResult = await lockHandler.Handle(new LockWalletCommand(walletId.Value, "Suspicious"), CancellationToken.None);
        lockResult.IsSuccess.Should().BeTrue();
        wallet.Status.Should().Be(WalletStatus.Locked);

        // Unlock
        var unlockResult = await unlockHandler.Handle(new UnlockWalletCommand(walletId.Value, "Verified"), CancellationToken.None);
        unlockResult.IsSuccess.Should().BeTrue();
        wallet.Status.Should().Be(WalletStatus.Active);
    }
}
