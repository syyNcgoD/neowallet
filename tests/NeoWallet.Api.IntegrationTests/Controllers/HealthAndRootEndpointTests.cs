using System.Net;
using FluentAssertions;
using NeoWallet.Api.IntegrationTests.Common;
using Xunit;

namespace NeoWallet.Api.IntegrationTests.Controllers;

public sealed class HealthAndRootEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthAndRootEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootEndpoint_ShouldReturnHealthyStatus()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
        content.Should().Contain("NeoWallet");
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ShouldPropagateCorrelationIdHeader()
    {
        var customCorrelationId = "test-corr-id-123456";
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-Id", customCorrelationId);

        var response = await _client.SendAsync(request);

        response.Headers.Should().ContainKey("X-Correlation-Id");
        response.Headers.GetValues("X-Correlation-Id").First().Should().Be(customCorrelationId);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_WhenMissingHeader_ShouldGenerateNewCorrelationId()
    {
        var response = await _client.GetAsync("/");

        response.Headers.Should().ContainKey("X-Correlation-Id");
        var generatedId = response.Headers.GetValues("X-Correlation-Id").First();
        generatedId.Should().NotBeNullOrWhiteSpace();
    }
}
