namespace NeoWallet.Infrastructure.Persistence.Options;

/// <summary>
/// Configuration options for Marten Event Store and Document Store.
/// </summary>
public sealed class MartenSettings
{
    public const string SectionName = "Marten";

    /// <summary>
    /// PostgreSQL connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Database schema name dedicated for NeoWallet event streams and projections.
    /// </summary>
    public string SchemaName { get; set; } = "neowallet";

    /// <summary>
    /// Controls whether Marten automatically generates database schema objects on startup.
    /// </summary>
    public bool AutoCreateSchemaObjects { get; set; } = true;
}
