using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace NeoWallet.Infrastructure.IntegrationTests.Fixtures;
public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    public string ConnectionString { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("neowallet_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            IsAvailable = true;

            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Postgres"] = ConnectionString,
                    ["Marten:SchemaName"] = "neowallet_test",
                    ["Marten:AutoCreateSchemaObjects"] = "true"
                })
                .Build();

            services.AddLogging();
            services.AddNeoWalletInfrastructure(configuration);

            ServiceProvider = services.BuildServiceProvider();
        }
        catch (Exception)
        {
            // Docker daemon may not be active in this local environment; mark unavailable
            IsAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
