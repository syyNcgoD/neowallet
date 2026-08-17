using Microsoft.AspNetCore.Mvc;
using NeoWallet.Api.Common;
using NeoWallet.Application.Common.Interfaces;

namespace NeoWallet.Api.Controllers;

[Route("api/market")]
public sealed class MarketController : ApiController
{
    private readonly IMarketService _marketService;
    private readonly IEmailService _emailService;

    public MarketController(IMarketService marketService, IEmailService emailService)
    {
        _marketService = marketService;
        _emailService = emailService;
    }

    [HttpGet("crypto")]
    public async Task<IActionResult> GetCryptoPrices(CancellationToken ct)
    {
        var prices = await _marketService.GetLiveCryptoPricesAsync(ct);
        return Ok(prices);
    }

    [HttpGet("stocks")]
    public async Task<IActionResult> GetStockQuotes(CancellationToken ct)
    {
        var stocks = await _marketService.GetLiveStockQuotesAsync(ct);
        return Ok(stocks);
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string? email, CancellationToken ct)
    {
        var recipient = string.IsNullOrWhiteSpace(email) ? "moh.maghsoudii@gmail.com" : email;
        var html = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                <h2 style='color: #0f172a;'>🎉 NeoWallet Live Notification Test</h2>
                <p>Hello <strong>{recipient}</strong>,</p>
                <p>Congrats on sending your <strong>first transactional email</strong> with Resend and NeoWallet!</p>
                <p style='color: #64748b; font-size: 14px;'>This verifies that the Resend API integration is 100% active and functioning in production.</p>
                <hr style='border: none; border-top: 1px solid #f1f5f9; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #94a3b8;'>NeoWallet Enterprise Event-Sourced System &copy; 2026</p>
            </div>";

        var success = await _emailService.SendEmailAsync(recipient, "NeoWallet System Notification — Live Verification", html, ct);
        return Ok(new { success, recipient, sentAt = DateTime.UtcNow });
    }
}
