using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
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

// HSTS configuration for production
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Strict CORS for Production & Frontend Domains
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://www.maniiai.ir",
            "https://maniiai.ir",
            "https://frontend-khaki-eta-q0o1goip7w.vercel.app",
            "https://frontend-kn99r28rz-manis-projects-3bb0f689.vercel.app",
            "https://neowallet-five.vercel.app",
            "http://localhost:3000",
            "http://localhost:3001"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Built-in .NET 8 Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth-limit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 15;
        opt.QueueLimit = 0;
    });

    options.AddSlidingWindowLimiter("tx-limit", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 10;
        opt.SegmentsPerWindow = 2;
        opt.QueueLimit = 0;
    });
});

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

// Enable HSTS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Global Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

// Enable CORS
app.UseCors();

// Rate Limiter Middleware
app.UseRateLimiter();

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
app.MapHub<WalletHub>("/hubs/wallet");
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
