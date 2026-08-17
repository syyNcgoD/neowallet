using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NeoWallet.Application.Common.Interfaces;
using Resend;

namespace NeoWallet.Infrastructure.Services;

public sealed class ResendEmailService : IEmailService
{
    private readonly IResend? _resend;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly string _fromEmail;

    public ResendEmailService(IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _logger = logger;
        var apiKey = configuration["Resend:ApiKey"] 
            ?? Environment.GetEnvironmentVariable("RESEND_API_KEY")
            ?? configuration["RESEND_API_KEY"];
        
        _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _resend = ResendClient.Create(apiKey);
        }
        else
        {
            _logger.LogWarning("Resend API Key is not configured. Email dispatch will be simulated.");
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        var targetRecipient = string.IsNullOrWhiteSpace(toEmail) ? "moh.maghsoudii@gmail.com" : toEmail;

        if (_resend == null)
        {
            _logger.LogInformation("[Simulated Email] To: {To} | Subject: {Subject}", targetRecipient, subject);
            return true;
        }

        try
        {
            var message = new EmailMessage
            {
                From = _fromEmail,
                To = targetRecipient,
                Subject = subject,
                HtmlBody = htmlContent
            };

            var response = await _resend.EmailSendAsync(message, cancellationToken);
            _logger.LogInformation("Resend email sent successfully to {To} with Subject {Subject}. Response: {Id}", targetRecipient, subject, response?.Content);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Resend to {To}", toEmail);
            return false;
        }
    }
}
