using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NeoWallet.Api.Extensions;

public static class OpenTelemetryExtensions
{
    public const string ServiceName = "NeoWallet.Api";
    public const string ActivitySourceName = "NeoWallet.Api";
    public const string MeterName = "NeoWallet.Meter";

    public static IServiceCollection AddNeoWalletOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(ServiceName, serviceVersion: "1.0.0");

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(ActivitySourceName)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
                        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        {
                            opts.Endpoint = new Uri(otlpEndpoint);
                        }
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
