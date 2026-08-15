using FluentAssertions;
using MediatR;
using NeoWallet.Application.Common.Abstractions.Messaging;
using NeoWallet.Application.Common.Behaviors;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Domain.Common;
using NSubstitute;

namespace NeoWallet.Application.UnitTests.Common.Behaviors;

public sealed class IdempotencyBehaviorTests
{
    public sealed record TestIdempotentCommand(string IdempotencyKey, string Data) : IRequest<Result>, IIdempotentCommand;

    [Fact]
    public async Task Handle_FirstCall_ShouldInvokeHandlerAndStoreResult()
    {
        var store = Substitute.For<IIdempotencyStore>();
        store.GetResultAsync<Result>("key-123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result?>(null));

        var behavior = new IdempotencyBehavior<TestIdempotentCommand, Result>(store);
        var command = new TestIdempotentCommand("key-123", "SomeData");
        var executionCount = 0;

        RequestHandlerDelegate<Result> next = () =>
        {
            executionCount++;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(command, next, CancellationToken.None);

        executionCount.Should().Be(1);
        result.IsSuccess.Should().BeTrue();
        await store.Received(1).StoreResultAsync("key-123", Arg.Any<Result>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateCall_ShouldReturnCachedResultWithoutInvokingHandler()
    {
        var store = Substitute.For<IIdempotencyStore>();
        var cachedSuccess = Result.Success();
        store.GetResultAsync<Result>("key-123", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result?>(cachedSuccess));

        var behavior = new IdempotencyBehavior<TestIdempotentCommand, Result>(store);
        var command = new TestIdempotentCommand("key-123", "SomeData");
        var executionCount = 0;

        RequestHandlerDelegate<Result> next = () =>
        {
            executionCount++;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(command, next, CancellationToken.None);

        executionCount.Should().Be(0); // Handler NOT invoked!
        result.IsSuccess.Should().BeTrue();
        await store.DidNotReceive().StoreResultAsync(Arg.Any<string>(), Arg.Any<Result>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
    }
}
