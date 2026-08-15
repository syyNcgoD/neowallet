using FluentAssertions;
using FluentValidation;
using MediatR;
using NeoWallet.Application.Common.Behaviors;
using NeoWallet.Domain.Common;

namespace NeoWallet.Application.UnitTests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record TestRequest(string Name) : IRequest<Result>;

    public sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithErrorCode("Name.Empty").WithMessage("Name is required.");
        }
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCallNextDelegate()
    {
        var validator = new TestRequestValidator();
        var behavior = new ValidationBehavior<TestRequest, Result>([validator]);
        var request = new TestRequest("Valid Name");
        var nextCalled = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenInvalid_ShouldReturnValidationFailureWithoutInvokingNext()
    {
        var validator = new TestRequestValidator();
        var behavior = new ValidationBehavior<TestRequest, Result>([validator]);
        var request = new TestRequest(""); // Invalid empty name
        var nextCalled = false;

        RequestHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Name.Empty");
        result.Error.Description.Should().Be("Name is required.");
    }
}
