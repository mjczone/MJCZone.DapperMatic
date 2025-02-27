namespace MJCZone.DapperMatic.WebApi.Options;

/// <summary>
/// Represents the options for configuring a connection string vault.
/// </summary>
public class ConnectionStringsVaultOptions
{
    /// <summary>
    /// Gets or sets the name of the connection string vault factory.
    /// </summary>
    public string? FactoryName { get; set; }

    /// <summary>
    /// Gets or sets the roles that have access to manage the connection string vault.
    /// </summary>
    public string? Roles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection string vault is read-only.
    /// </summary>
    public bool? IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the encryption key used for connection strings.
    /// </summary>
    public string? EncryptionKey { get; set; }

    /// <summary>
    /// Gets or sets the settings for the connection string vault.
    /// </summary>
    public Dictionary<string, object?>? Settings { get; set; }
}
