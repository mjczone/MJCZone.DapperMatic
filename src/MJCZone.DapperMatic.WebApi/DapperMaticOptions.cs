namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Represents the options for configuring DapperMatic.
/// </summary>
public class DapperMaticOptions
{
    /// <summary>
    /// Gets the name of the section in the configuration file.
    /// </summary>
    public const string SectionName = "DapperMatic";

    /// <summary>
    /// Gets or sets the API prefix for DapperMatic endpoints.
    /// </summary>
    public string? ApiPrefix { get; set; } = "/api/dappermatic";

    /// <summary>
    /// Gets or sets the encryption key used for connection strings.
    /// Without an ecryption key, connection strings will be stored in plain text.
    /// </summary>
    public string? ConnectionStringEncryptionKey { get; set; }

    /// <summary>
    /// Gets or sets the default database information.
    /// </summary>
    public DatabaseEntry? DefaultDatabase { get; set; }
}
