using FluentAssertions;
using NeoWallet.Api.Common;
using NeoWallet.Domain.Common;
using NetArchTest.Rules;
using Xunit;

namespace NeoWallet.ArchitectureTests;

public sealed class ArchitectureTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(IDomainEvent).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(NeoWallet.Application.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(NeoWallet.Infrastructure.DependencyInjection).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(ApiController).Assembly;

    [Fact]
    public void Domain_Should_Not_DependOn_OtherLayers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "NeoWallet.Application",
                "NeoWallet.Infrastructure",
                "NeoWallet.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_DependOn_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "NeoWallet.Infrastructure",
                "NeoWallet.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_DependOn_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("NeoWallet.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_Should_Implement_IDomainEvent()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ResideInNamespace("NeoWallet.Domain.Events")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(IDomainEvent))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_Should_Inherit_From_ApiController()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .ResideInNamespace("NeoWallet.Api.Controllers")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("Controller")
            .Should()
            .Inherit(typeof(ApiController))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
