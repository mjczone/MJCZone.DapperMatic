using System.Text.Json;

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
    /// Gets the JSON serializer options for DapperMatic.
    /// </summary>
    internal static readonly JsonSerializerOptions? JsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Gets or sets the API prefix for DapperMatic endpoints.
    /// </summary>
    public string? ApiPrefix { get; set; } = "/api/dappermatic";

    /// <summary>
    /// Gets or sets the encryption key used for connection strings.
    /// Without an ecryption key, connection strings will be stored in plain text.
    /// </summary>
    public string? ConnectionStringEncryptionKey { get; set; } =
        "replace-with-your-encryption-key!";

    /// <summary>
    /// Gets or sets the path to the connection string file.
    /// </summary>
    public string? ConnectionStringsFilePath { get; set; } =
        "../data/dappermatic-connection-strings.json";

    /// <summary>
    /// Gets or sets the connection string for the database registry.
    /// </summary>
    public string? DatabaseRegistryConnectionString { get; set; } =
        "Data Source=../data/dappermatic-databases.db;Version=3;BinaryGUID=False;";

    // "Data Source=../data/dappermatic-databases.db;Version=3;GuidFormat=String;Mode=ReadWriteCreate;";

    /// <summary>
    /// Gets or sets the provider type for the database registry.
    /// </summary>
    public DbProviderType? DatabaseRegistryProviderType { get; set; } = DbProviderType.Sqlite;
}
