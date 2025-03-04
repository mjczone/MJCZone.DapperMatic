using JsonFlatFileDataStore;
using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Vaults;

/// <summary>
/// Provides functionality to resolve and manage connection strings from a file.
/// </summary>
/// <remarks>
/// Leverages the <see cref="DataStore"/> class from the JsonFlatFileDataStore library.
/// See: https://ttu.github.io/json-flatfile-datastore/#/2.4.2/?id=json-flat-file-data-store.
/// </remarks>
public class ConnectionStringsFileVault : ConnectionStringsVaultBase
{
    private readonly string _fileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsFileVault"/> class.
    /// </summary>
    /// <param name="name">The name of the connection string vault.</param>
    /// <param name="vaultOptions">The options for the connection string vault.</param>
    public ConnectionStringsFileVault(string name, ConnectionStringsVaultOptions vaultOptions)
        : base(name, vaultOptions)
    {
        ArgumentNullException.ThrowIfNull(vaultOptions.Settings);

        if (
            !vaultOptions
                .Settings.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value)
                .TryGetValue("filename", out var fileName)
            || string.IsNullOrWhiteSpace(fileName?.ToString())
        )
        {
            throw new ArgumentException("FileName is required for FileVault.");
        }

        _fileName = fileName.ToString()!;
    }

    /// <summary>
    /// Gets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The encrypted connection string if found; otherwise, null.</returns>
    protected override async Task<string?> GetEncryptedConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Yield();

        if (string.IsNullOrWhiteSpace(_fileName))
        {
            throw new ArgumentException("Connection strings file path cannot be null or empty.");
        }

        using var store = new DataStore(EnsureFileDirectoryAndReturnFullFilePath(_fileName));
        var encryptedConnectionStrings = store.GetCollection("connection_strings");

        return encryptedConnectionStrings
            .AsQueryable()
            .FirstOrDefault(RecordPredicate(connectionStringName, tenantIdentifier))
            ?.value?.ToString();
    }

    /// <summary>
    /// Sets the connection string for the specified name and updates the dynamic configuration file.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="encryptedConnectionString">The connection string value to set.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task SetEncryptedConnectionStringAsync(
        string connectionStringName,
        string encryptedConnectionString,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(_fileName))
        {
            throw new ArgumentException("Connection strings file path cannot be null or empty.");
        }

        using var store = new DataStore(EnsureFileDirectoryAndReturnFullFilePath(_fileName));
        var collection = store.GetCollection("connection_strings");

        if (
            !await collection
                .UpdateOneAsync(
                    RecordPredicate(connectionStringName, tenantIdentifier),
                    new
                    {
                        value = encryptedConnectionString,
                        tenant_identifier = string.IsNullOrWhiteSpace(tenantIdentifier)
                            ? null
                            : tenantIdentifier,
                    }
                )
                .ConfigureAwait(false)
        )
        {
            await collection
                .InsertOneAsync(
                    new
                    {
                        name = connectionStringName,
                        value = encryptedConnectionString,
                        tenant_identifier = string.IsNullOrWhiteSpace(tenantIdentifier)
                            ? null
                            : tenantIdentifier,
                    }
                )
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to delete.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task DeleteEncryptedConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(_fileName))
        {
            throw new ArgumentException("Connection strings file path cannot be null or empty.");
        }

        using var store = new DataStore(EnsureFileDirectoryAndReturnFullFilePath(_fileName));
        var collection = store.GetCollection("connection_strings");

        await collection
            .DeleteOneAsync(RecordPredicate(connectionStringName, tenantIdentifier))
            .ConfigureAwait(false);
    }

    private static string EnsureFileDirectoryAndReturnFullFilePath(string filePathAndName)
    {
        if (!File.Exists(filePathAndName))
        {
            var directory = Path.GetDirectoryName(filePathAndName);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Path.GetDirectoryName(Path.GetFullPath(filePathAndName));
            }

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            filePathAndName = Path.Combine(directory, Path.GetFileName(filePathAndName));
        }

        return Path.GetFullPath(filePathAndName);
    }

    private static Predicate<dynamic> RecordPredicate(
        string connectionStringName,
        string? tenantIdentifier
    )
    {
        return string.IsNullOrWhiteSpace(tenantIdentifier)
            ? (
                c =>
                    c.name.ToString()
                        .Equals(connectionStringName, StringComparison.OrdinalIgnoreCase)
                    && c.tenant_identifier == null
            )
            : (
                c =>
                    c.name.ToString()
                        .Equals(connectionStringName, StringComparison.OrdinalIgnoreCase)
                    && c.tenant_identifier != null
                    && c.tenant_identifier.ToString()
                        .Equals(tenantIdentifier, StringComparison.OrdinalIgnoreCase)
            );
    }
}
