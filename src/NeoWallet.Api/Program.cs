using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NeoWallet.Api.Extensions;
using NeoWallet.Api.Hubs;
using NeoWallet.Api.Middlewares;
using NeoWallet.Api.Services;
using NeoWallet.Application;
using NeoWallet.Application.Common.Interfaces;
using NeoWallet.Application.Features.Reconciliation.Workers;
using NeoWallet.Infrastructure;
using NeoWallet.Infrastructure.Authentication.Options;
using OpenTelemetry.Metrics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Application & Infrastructure Services
builder.Services.AddNeoWalletApplication();
builder.Services.AddNeoWalletInfrastructure(builder.Configuration);
builder.Services.AddNeoWalletOpenTelemetry(builder.Configuration);
builder.Services.AddNeoWalletSwagger();

// Real-time SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IWalletNotificationService, SignalRNotificationService>();

// Hosted Background Services
builder.Services.AddHostedService<ReconciliationWorker>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Custom Middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NeoWallet API v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WalletHub>("/hubs/wallets");
app.MapPrometheusScrapingEndpoint();

app.MapGet("/", () => Results.Ok(new
{
    Application = "NeoWallet Enterprise Distributed Wallet",
    Status = "Healthy",
    Version = "1.0.0",
    TimestampUtc = DateTime.UtcNow
}));

app.Run();

public partial class Program;
