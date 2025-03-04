namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a request to add or update a connection strings entry.
/// </summary>
public class ConnectionStringsEntryRequest
{
    /// <summary>
    /// Gets or sets the name of the connection strings entry.
    /// </summary>
    /// <value>The name of the connection strings entry.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    /// <value>The connection string.</value>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant identifier for this connection string.
    /// This will guarantee that the connection string is only used for the specified tenant.
    /// </summary>
    /// <value>The tenant identifier.</value>
    public string? TenantIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the name of the vault.
    /// </summary>
    /// <value>The name of the vault.</value>
    public string? Vault { get; set; }
}
