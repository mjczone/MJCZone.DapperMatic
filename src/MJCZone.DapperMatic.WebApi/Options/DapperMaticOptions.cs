using System.Data.SQLite;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace MJCZone.DapperMatic.WebApi.Options;

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
    /// Gets the default API prefix for DapperMatic endpoints.
    /// </summary>
    public const string DefaultApiPrefix = "/api/dappermatic";

    /// <summary>
    /// Gets the default name of the connection strings vault.
    /// </summary>
    public const string DefaultDapperMaticConnectionStringsVaultName = "LocalFile";

    /// <summary>
    /// Gets the default name of the connection strings vault file.
    /// </summary>
    public const string DefaultDapperMaticConnectionStringsVaultFileName =
        "../data/dappermatic-connection-strings.json";

    private static readonly Lazy<JsonSerializerOptions> LazyJsonSerializerOptions =
        new Lazy<JsonSerializerOptions>(() =>
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true,
            };
            options.Converters.Add(new JsonConverterForType());
            return options;
        });

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperMaticOptions"/> class.
    /// </summary>
    public DapperMaticOptions()
    {
        // Populate the default connection strings vault options
        ConnectionStringsVaults = new Dictionary<string, ConnectionStringsVaultOptions?>
        {
            {
                "LocalFile",
                new ConnectionStringsVaultOptions
                {
                    FactoryName = "FileVault",
                    IsReadOnly = false,
                    Roles = "Admin;Admins;Administrator;Administrators",
                    EncryptionKey = "replace-with-your-encryption-key!",
                    Settings = new Dictionary<string, object?>
                    {
                        {
                            "FileName",
                            PathUtils.NormalizePath(
                                DefaultDapperMaticConnectionStringsVaultFileName
                            )
                        },
                    },
                }
            },
            {
                "LocalDatabase",
                new ConnectionStringsVaultOptions
                {
                    FactoryName = "DatabaseVault",
                    IsReadOnly = false,
                    Roles = "Admin;Admins;Administrator;Administrators",
                    EncryptionKey = "replace-with-your-encryption-key!",
                    Settings = new Dictionary<string, object?>
                    {
                        { "ProviderType", "Sqlite" },
                        {
                            "ConnectionString",
                            new SQLiteConnectionStringBuilder
                            {
                                DataSource = PathUtils.NormalizePath(
                                    "../data/dappermatic-connection-strings.db"
                                ),
                                Version = 3,
                                SyncMode = SynchronizationModes.Full,
                                DateTimeFormat = SQLiteDateFormats.ISO8601,
                                DateTimeKind = DateTimeKind.Utc,
                                FailIfMissing = false,
                                JournalMode = SQLiteJournalModeEnum.Wal,
                                Pooling = true,
                                ReadOnly = false,
                                ForeignKeys = true,
                                BinaryGUID = false,
                            }.ToString()
                        },
                        { "TableName", "web_connection_strings" },
                        { "NameColumn", "name" },
                        { "ValueColumn", "value" },
                    },
                }
            },
        };
        DefaultConnectionStringsVaultName = DefaultDapperMaticConnectionStringsVaultName;

        // Populate the default database registry options
        DatabaseRegistry = new DatabaseRegistryOptions
        {
            ProviderType = DbProviderType.Sqlite,
            ConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = PathUtils.NormalizePath("../data/dappermatic-databases.db"),
                Version = 3,
                SyncMode = SynchronizationModes.Full,
                DateTimeFormat = SQLiteDateFormats.ISO8601,
                DateTimeKind = DateTimeKind.Utc,
                FailIfMissing = false,
                JournalMode = SQLiteJournalModeEnum.Wal,
                Pooling = true,
                ReadOnly = false,
                ForeignKeys = true,
                BinaryGUID = false,
            }.ToString(),
        };
    }

    /// <summary>
    /// Gets the JSON serializer options for DapperMatic.
    /// </summary>
    public static JsonSerializerOptions JsonSerializerOptions => LazyJsonSerializerOptions.Value;

    /// <summary>
    /// Gets or sets the API prefix for DapperMatic endpoints.
    /// </summary>
    public string? ApiPrefix { get; set; } = DefaultApiPrefix;

    /// <summary>
    /// Gets or sets the name of the default connection string vault.
    /// </summary>
    public string? DefaultConnectionStringsVaultName { get; set; }

    /// <summary>
    /// Gets or sets the options for configuring connection string vaults.
    /// </summary>
    public Dictionary<string, ConnectionStringsVaultOptions?>? ConnectionStringsVaults { get; set; }

    /// <summary>
    /// Gets or sets the options for configuring the database registry.
    /// </summary>
    public DatabaseRegistryOptions? DatabaseRegistry { get; set; }
}

/// <summary>
/// Represents the options for configuring a connection string vault.
/// </summary>
public static class DapperMaticOptionsExtensible
{
    /// <summary>
    /// Gets the API prefix for DapperMatic endpoints.
    /// </summary>
    /// <param name="options">The <see cref="DapperMaticOptions"/>.</param>
    /// <returns>The API prefix for DapperMatic endpoints.</returns>
    public static string GetApiPrefix(this DapperMaticOptions? options)
    {
        return $"/{options?.ApiPrefix?.Trim('/') ?? DapperMaticOptions.DefaultApiPrefix.Trim('/')}";
    }

    /// <summary>
    /// Gets the API prefix for DapperMatic endpoints.
    /// </summary>
    /// <param name="optionsMonitor">The <see cref="DapperMaticOptions"/>.</param>
    /// <returns>The API prefix for DapperMatic endpoints.</returns>
    public static string GetApiPrefix(this IOptionsMonitor<DapperMaticOptions> optionsMonitor)
    {
        return optionsMonitor.CurrentValue.GetApiPrefix();
    }
}
