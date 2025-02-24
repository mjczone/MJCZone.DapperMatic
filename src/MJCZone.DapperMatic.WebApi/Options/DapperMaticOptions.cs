using System;
using System.Data.SQLite;
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
    /// Gets the default name of the connection strings vault.
    /// </summary>
    public const string DefaultDapperMaticConnectionStringsVaultName = "LocalFile";

    /// <summary>
    /// Gets the default name of the connection strings vault file.
    /// </summary>
    public const string DefaultDapperMaticConnectionStringsVaultFileName =
        "../data/dappermatic-connection-strings.json";

    /// <summary>
    /// Gets the JSON serializer options for DapperMatic.
    /// </summary>
    internal static readonly JsonSerializerOptions? JsonSerializerOptions =
        new() { PropertyNameCaseInsensitive = true, IncludeFields = true };

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
                        { "FileName", DefaultDapperMaticConnectionStringsVaultFileName },
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
                                DataSource = "../data/dappermatic-connection-strings.db",
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
                DataSource = "../data/dappermatic-databases.db",
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
    /// Gets or sets the API prefix for DapperMatic endpoints.
    /// </summary>
    public string? ApiPrefix { get; set; } = "/api/dappermatic";

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
