using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeoWallet.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddNeoWalletSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NeoWallet Enterprise API",
                Version = "v1",
                Description = "High-performance distributed event-sourced digital wallet API."
            });

            // Bearer JWT definition
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token: `Bearer {token}`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

            // API Key definition
            var apiKeySecurityScheme = new OpenApiSecurityScheme
            {
                Name = "X-API-Key",
                Description = "Enter raw integration API key (e.g. `nw_live_...`)",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Reference = new OpenApiReference
                {
                    Id = "ApiKey",
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition("ApiKey", apiKeySecurityScheme);

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtSecurityScheme, Array.Empty<string>() },
                { apiKeySecurityScheme, Array.Empty<string>() }
            });
        });

        return services;
    }
}
