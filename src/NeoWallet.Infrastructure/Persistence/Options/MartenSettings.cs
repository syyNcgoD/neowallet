namespace NeoWallet.Infrastructure.Persistence.Options;
public sealed class MartenSettings
{
    public const string SectionName = "Marten";
    public string ConnectionString { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "neowallet";
    public bool AutoCreateSchemaObjects { get; set; } = true;
}
