using Microsoft.Extensions.Options;

namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides functionality to resolve and manage connection strings from a file.
/// </summary>
public class ConnectionStringFileVault : IConnectionStringVault
{
    private readonly IOptionsMonitor<DapperMaticOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringFileVault"/> class.
    /// </summary>
    /// <param name="options">The options for configuring DapperMatic.</param>
    public ConnectionStringFileVault(IOptionsMonitor<DapperMaticOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// Gets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection string if found; otherwise, null.</returns>
    public async Task<string?> GetConnectionStringAsync(
        string connectionStringName,
        CancellationToken cancellationToken = default
    )
    {
        var encryptionKey = _options.CurrentValue.ConnectionStringEncryptionKey;
        if (string.IsNullOrWhiteSpace(_options.CurrentValue.ConnectionStringsFilePath))
        {
            throw new ArgumentException("Connection strings file path cannot be null or empty.");
        }
        var filePath = EnsureFile(_options.CurrentValue.ConnectionStringsFilePath);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        var connectionStrings = System.Text.Json.JsonSerializer.Deserialize<
            Dictionary<string, string>
        >(json, DapperMaticOptions.JsonSerializerOptions);

        if (
            connectionStrings != null
            && connectionStrings.TryGetValue(connectionStringName, out var connectionString)
        )
        {
            if (!string.IsNullOrWhiteSpace(encryptionKey))
            {
                connectionString = Crypto.Decrypt(connectionString, encryptionKey);
            }
            return connectionString;
        }

        return null;
    }

    /// <summary>
    /// Sets the connection string for the specified name and updates the dynamic configuration file.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="connectionString">The connection string value to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetConnectionStringAsync(
        string connectionStringName,
        string connectionString,
        CancellationToken cancellationToken = default
    )
    {
        var encryptionKey = _options.CurrentValue.ConnectionStringEncryptionKey;
        if (string.IsNullOrWhiteSpace(_options.CurrentValue.ConnectionStringsFilePath))
        {
            throw new ArgumentException("Connection strings file path cannot be null or empty.");
        }
        var filePath = EnsureFile(_options.CurrentValue.ConnectionStringsFilePath);

        var json = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        var connectionStrings = System.Text.Json.JsonSerializer.Deserialize<
            Dictionary<string, string>
        >(json, DapperMaticOptions.JsonSerializerOptions);

        connectionStrings ??= [];

        if (!string.IsNullOrWhiteSpace(encryptionKey))
        {
            connectionString = Crypto.Encrypt(connectionString, encryptionKey);
        }

        connectionStrings[connectionStringName] = connectionString;
        json = System.Text.Json.JsonSerializer.Serialize(connectionStrings);
        await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
    }

    private static string EnsureFile(string path)
    {
        var filePath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("Invalid file path.");
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "{}");
        }

        return filePath;
    }
}
