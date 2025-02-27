using Dapper;

using MJCZone.DapperMatic.Models;
using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Vaults;

/// <summary>
/// Provides functionality to resolve and manage connection strings from a database.
/// </summary>
public class ConnectionStringsDatabaseVault : ConnectionStringsVaultBase
{
    private readonly string _connectionString;
    private readonly DbProviderType _providerType;
    private readonly string _tableName = "web_connection_strings";
    private readonly string _nameColumn = "name";
    private readonly string _valueColumn = "value";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsDatabaseVault"/> class.
    /// </summary>
    /// <param name="name">The name of the connection string vault.</param>
    /// <param name="vaultOptions">The options for the connection string vault.</param>
    public ConnectionStringsDatabaseVault(string name, ConnectionStringsVaultOptions vaultOptions)
        : base(name, vaultOptions)
    {
        ArgumentNullException.ThrowIfNull(vaultOptions.Settings);

        if (
            !vaultOptions
                .Settings.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value)
                .TryGetValue("connectionstring", out var connectionstring)
            || string.IsNullOrWhiteSpace(connectionstring?.ToString())
        )
        {
            throw new ArgumentException("ConnectionString is required for DatabaseVault.");
        }

        if (
            !vaultOptions
                .Settings.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value)
                .TryGetValue("providertype", out var providertype)
            || string.IsNullOrWhiteSpace(providertype?.ToString())
        )
        {
            throw new ArgumentException("ProviderType is required for DatabaseVault.");
        }

        if (
            vaultOptions
                .Settings.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value)
                .TryGetValue("tablename", out var tablename)
            && !string.IsNullOrWhiteSpace(tablename?.ToString())
        )
        {
            _tableName = tablename.ToString()!;
        }

        _connectionString = connectionstring?.ToString()!;
        _providerType = Enum.Parse<DbProviderType>(providertype.ToString()!, true);

        if (
            vaultOptions
                .Settings.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value)
                .TryGetValue("namecolumn", out var namecolumn)
            && !string.IsNullOrWhiteSpace(namecolumn?.ToString())
        )
        {
            _nameColumn = namecolumn.ToString()!;
        }

        if (
            vaultOptions
                .Settings.ToDictionary(k => k.Key.ToLowerInvariant(), v => v.Value)
                .TryGetValue("valuecolumn", out var valuecolumn)
            && !string.IsNullOrWhiteSpace(valuecolumn?.ToString())
        )
        {
            _valueColumn = valuecolumn.ToString()!;
        }

        // initialize the database
        InitializeDatabase();
    }

    /// <summary>
    /// Gets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The encrypted connection string if found; otherwise, null.</returns>
    protected override async Task<string?> GetEncryptedConnectionStringAsync(
        string connectionStringName,
        CancellationToken cancellationToken
    )
    {
        using var connection = DatabaseConnectionFactory.GetDbConnection(
            _connectionString,
            _providerType
        );

        return await connection
            .QueryFirstOrDefaultAsync<string?>(
                $"SELECT {_valueColumn} FROM {_tableName} WHERE {_nameColumn} = @name",
                new { name = connectionStringName }
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the connection string for the specified name and updates the dynamic configuration file.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="encryptedConnectionString">The connection string value to set.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task SetEncryptedConnectionStringAsync(
        string connectionStringName,
        string encryptedConnectionString,
        CancellationToken cancellationToken
    )
    {
        using var connection = DatabaseConnectionFactory.GetDbConnection(
            _connectionString,
            _providerType
        );

        var updates = await connection
            .ExecuteAsync(
                $"UPDATE {_tableName} SET {_valueColumn} = @value WHERE {_nameColumn} = @name",
                new { name = connectionStringName, value = encryptedConnectionString }
            )
            .ConfigureAwait(false);

        if (updates == 0)
        {
            await connection
                .ExecuteAsync(
                    $"INSERT INTO {_tableName} ({_nameColumn}, {_valueColumn}) VALUES (@name, @value)",
                    new { name = connectionStringName, value = encryptedConnectionString }
                )
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Deletes the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task DeleteEncryptedConnectionStringAsync(
        string connectionStringName,
        CancellationToken cancellationToken
    )
    {
        using var connection = DatabaseConnectionFactory.GetDbConnection(
            _connectionString,
            _providerType
        );

        await connection
            .ExecuteAsync(
                $"DELETE FROM {_tableName} WHERE {_nameColumn} = @name",
                new { name = connectionStringName }
            )
            .ConfigureAwait(false);
    }

    private void InitializeDatabase()
    {
        using var connection = DatabaseConnectionFactory.GetDbConnection(
            _connectionString,
            _providerType
        );
        try
        {
            connection.Open();
            connection
                .CreateTableIfNotExistsAsync(
                    null,
                    _tableName,
                    [
                        new DmColumn(
                            null,
                            _tableName,
                            _nameColumn,
                            typeof(string),
                            length: 256,
                            isPrimaryKey: true,
                            isNullable: false
                        ),
                        new DmColumn(
                            null,
                            _tableName,
                            _valueColumn,
                            typeof(string),
                            length: 4096,
                            isNullable: false
                        ),
                    ]
                )
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Failed to open connection to database.", ex);
        }
        finally
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                connection?.Close();
                connection?.Dispose();
            }
            catch
            { /* ignore error */
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
