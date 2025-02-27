namespace MJCZone.DapperMatic.WebApi.Options;

/// <summary>
/// Represents the options for configuring the database registry.
/// </summary>
public class DatabaseRegistryOptions
{
    /// <summary>
    /// Gets or sets the provider type for the database registry.
    /// </summary>
    public DbProviderType? ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the connection string for the database registry.
    /// </summary>
    public string? ConnectionString { get; set; }
}
