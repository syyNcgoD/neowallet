using FluentAssertions;
using NeoWallet.Domain.Common;

namespace NeoWallet.Domain.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_ShouldSetIsSuccessTrueAndErrorNone()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldSetIsFailureTrueAndError()
    {
        // Arrange
        var error = Error.Failure("Test.Fail", "A test failure.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void TypedSuccess_ShouldReturnExpectedValue()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void TypedFailure_AccessingValue_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = Error.NotFound("Item.NotFound", "Item not found");
        var result = Result.Failure<int>(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Item.NotFound*");
    }

    [Fact]
    public void Match_ShouldExecuteCorrectBranch()
    {
        var success = Result.Success("ok");
        var fail = Result.Failure<string>(Error.Validation("ERR", "msg"));

        var successMsg = success.Match(val => $"value: {val}", err => $"error: {err.Code}");
        var failMsg = fail.Match(val => $"value: {val}", err => $"error: {err.Code}");

        successMsg.Should().Be("value: ok");
        failMsg.Should().Be("error: ERR");

        var plainSuccess = Result.Success();
        var plainSuccessMsg = plainSuccess.Match(() => "worked", _ => "failed");
        plainSuccessMsg.Should().Be("worked");
    }

    [Fact]
    public void Map_ShouldTransformValueOnSuccess()
    {
        var result = Result.Success(10);
        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(20);

        var fail = Result.Failure<int>(Error.Failure("F", "d"));
        var mappedFail = fail.Map(x => x * 2);
        mappedFail.IsFailure.Should().BeTrue();
        mappedFail.Error.Code.Should().Be("F");
    }

    [Fact]
    public void Bind_ShouldChainResults()
    {
        var result = Result.Success(10);
        var bound = result.Bind(x => Result.Success(x.ToString()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");

        var fail = Result.Failure<int>(Error.Failure("F", "d"));
        var boundFail = fail.Bind(x => Result.Success(x.ToString()));
        boundFail.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Ensure_ShouldValidatePredicate()
    {
        var result = Result.Success(15);
        var ensurePass = result.Ensure(x => x > 10, Error.Validation("TOO_SMALL", "Too small"));
        var ensureFail = result.Ensure(x => x > 20, Error.Validation("TOO_SMALL", "Too small"));

        ensurePass.IsSuccess.Should().BeTrue();
        ensureFail.IsFailure.Should().BeTrue();
        ensureFail.Error.Code.Should().Be("TOO_SMALL");

        var alreadyFailed = Result.Failure<int>(Error.NotFound("404", "Not found"));
        var ensureOnFailed = alreadyFailed.Ensure(x => x > 0, Error.Validation("ERR", "err"));
        ensureOnFailed.Error.Code.Should().Be("404");
    }

    [Fact]
    public void Combine_ShouldReturnFirstFailureOrSuccess()
    {
        var r1 = Result.Success();
        var r2 = Result.Success();
        var r3 = Result.Failure(Error.Conflict("C1", "Conflict 1"));
        var r4 = Result.Failure(Error.Conflict("C2", "Conflict 2"));

        Result.Combine(r1, r2).IsSuccess.Should().BeTrue();
        var combinedFail = Result.Combine(r1, r3, r4);
        combinedFail.IsFailure.Should().BeTrue();
        combinedFail.Error.Code.Should().Be("C1");
    }

    [Fact]
    public void Error_StaticFactoriesAndTypes_ShouldWork()
    {
        var failure = Error.Failure("F", "Failure");
        var validation = Error.Validation("V", "Validation");
        var notFound = Error.NotFound("NF", "NotFound");
        var conflict = Error.Conflict("C", "Conflict");
        var unauthorized = Error.Unauthorized("UA", "Unauthorized");
        var forbidden = Error.Forbidden("FB", "Forbidden");

        failure.Type.Should().Be(ErrorType.Failure);
        validation.Type.Should().Be(ErrorType.Validation);
        notFound.Type.Should().Be(ErrorType.NotFound);
        conflict.Type.Should().Be(ErrorType.Conflict);
        unauthorized.Type.Should().Be(ErrorType.Unauthorized);
        forbidden.Type.Should().Be(ErrorType.Forbidden);

        failure.ToString().Should().Be("[F] Failure");
    }

    [Fact]
    public void ImplicitConversions_ShouldWorkAsExpected()
    {
        Result r1 = Error.Validation("ERR", "msg");
        r1.IsFailure.Should().BeTrue();

        Result<string> r2 = "hello";
        r2.IsSuccess.Should().BeTrue();
        r2.Value.Should().Be("hello");

        Result<string> r3 = (string)null!;
        r3.IsFailure.Should().BeTrue();

        Result<string> r4 = Error.NotFound("N", "d");
        r4.IsFailure.Should().BeTrue();
    }
}
