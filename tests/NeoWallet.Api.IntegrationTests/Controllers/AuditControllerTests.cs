using System.Net;
using FluentAssertions;
using NeoWallet.Api.IntegrationTests.Common;
using NeoWallet.Domain.Common;
using NSubstitute;
using Xunit;

namespace NeoWallet.Api.IntegrationTests.Controllers;

public sealed class AuditControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AuditControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task VerifyChain_WhenIntact_ShouldReturn200WithTrue()
    {
        _factory.MockAuditStore.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(true)));

        var response = await _client.GetAsync("/api/audit/verify-chain");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
