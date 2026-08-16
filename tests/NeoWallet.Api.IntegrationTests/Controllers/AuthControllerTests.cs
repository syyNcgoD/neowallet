using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NeoWallet.Api.Controllers;
using NeoWallet.Api.IntegrationTests.Common;
using Xunit;

namespace NeoWallet.Api.IntegrationTests.Controllers;

public sealed class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400BadRequest()
    {
        var request = new AuthController.RegisterRequest("invalid-email-string", "StrongPass@123");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyFields_ShouldReturn400BadRequest()
    {
        var request = new AuthController.LoginRequest("", "");

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ShouldReturn400BadRequest()
    {
        var request = new AuthController.RefreshTokenRequest(Guid.NewGuid(), "");

        var response = await _client.PostAsJsonAsync("/api/auth/refresh-token", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
