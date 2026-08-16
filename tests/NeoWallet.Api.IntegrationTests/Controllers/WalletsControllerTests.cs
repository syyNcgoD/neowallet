using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NeoWallet.Api.Controllers;
using NeoWallet.Api.IntegrationTests.Common;
using NeoWallet.Application.DTOs.Wallet;
using NeoWallet.Domain.Common;
using NeoWallet.Domain.Enums;
using NSubstitute;
using Xunit;

namespace NeoWallet.Api.IntegrationTests.Controllers;

public sealed class WalletsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public WalletsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSummary_WhenWalletExists_ShouldReturn200WithWalletSummaryDto()
    {
        var walletId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var summaryDto = new WalletSummaryDto(
            walletId,
            ownerId,
            1250.50m,
            "USD",
            WalletStatus.Active,
            3,
            DateTime.UtcNow);

        _factory.MockWalletReadService.GetWalletSummaryAsync(walletId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(summaryDto)));

        var response = await _client.GetAsync($"/api/wallets/{walletId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<WalletSummaryDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(walletId);
        result.Balance.Should().Be(1250.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetSummary_WhenWalletNotFound_ShouldReturn404ProblemDetails()
    {
        var walletId = Guid.NewGuid();
        _factory.MockWalletReadService.GetWalletSummaryAsync(walletId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<WalletSummaryDto>(Error.NotFound("Wallet.NotFound", "Wallet not found."))));

        var response = await _client.GetAsync($"/api/wallets/{walletId}/summary");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateWallet_WithInvalidCurrency_ShouldReturn400ValidationProblemDetails()
    {
        var request = new WalletsController.CreateWalletRequest(Guid.NewGuid(), "INVALID_CURRENCY_CODE");

        var response = await _client.PostAsJsonAsync("/api/wallets", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
