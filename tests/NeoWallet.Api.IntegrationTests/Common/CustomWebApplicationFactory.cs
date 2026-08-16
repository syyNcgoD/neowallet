using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NeoWallet.Application.Common.Interfaces;
using NSubstitute;

namespace NeoWallet.Api.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public IWalletReadService MockWalletReadService { get; } = Substitute.For<IWalletReadService>();
    public IAuditStore MockAuditStore { get; } = Substitute.For<IAuditStore>();
    public IReconciliationService MockReconciliationService { get; } = Substitute.For<IReconciliationService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Replace external database-bound read services with test doubles
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IWalletReadService));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }
            services.AddScoped(_ => MockWalletReadService);

            var auditDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuditStore));
            if (auditDescriptor is not null)
            {
                services.Remove(auditDescriptor);
            }
            services.AddScoped(_ => MockAuditStore);

            var reconDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IReconciliationService));
            if (reconDescriptor is not null)
            {
                services.Remove(reconDescriptor);
            }
            services.AddScoped(_ => MockReconciliationService);
        });
    }
}
