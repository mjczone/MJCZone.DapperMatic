namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Factory class for creating database connections.
/// </summary>
public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly IConnectionStringVault _connectionStringVault;
    private readonly IDatabaseRegistry _databaseRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionStringVault">The vault containing the connection strings.</param>
    /// <param name="databaseRegistry">The database registry containing databases.</param>
    public DatabaseConnectionFactory(
        IConnectionStringVault connectionStringVault,
        IDatabaseRegistry databaseRegistry
    )
    {
        this._connectionStringVault = connectionStringVault;
        this._databaseRegistry = databaseRegistry;
    }

    /// <summary>
    /// Creates a new database connection based on the provided database identifier.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier or null.</param>
    /// <param name="databaseIdOrSlug">The unique identifier of the database.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="IDbConnection"/>.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public async Task<IDbConnection> OpenConnectionAsync(
        string? tenantIdentifier,
        string databaseIdOrSlug,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(databaseIdOrSlug))
        {
            throw new ArgumentException("Database ID or slug cannot be null or empty.");
        }

        var database = await _databaseRegistry
            .GetDatabaseAsync(tenantIdentifier, databaseIdOrSlug, cancellationToken)
            .ConfigureAwait(false);
        if (database == null)
        {
            throw new ArgumentException($"Database with ID or Slug {databaseIdOrSlug} not found.");
        }

        if (string.IsNullOrWhiteSpace(database.ConnectionStringName))
        {
            throw new ArgumentException("Connection string name cannot be null or empty.");
        }

        var connectionString = await this
            ._connectionStringVault.GetConnectionStringAsync(
                database.ConnectionStringName,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                $"Connection string for {database.ConnectionStringName} not found."
            );
        }

        var providerType = database.ProviderType;

        System.Data.Common.DbConnection connection = GetDbConnection(
            connectionString,
            providerType
        );

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }

    /// <summary>
    /// Creates a new database connection based on the provided connection string and provider type.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="providerType">The database provider type.</param>
    /// <returns>A database connection.</returns>
    internal static System.Data.Common.DbConnection GetDbConnection(
        string connectionString,
        DbProviderType? providerType
    )
    {
        return providerType switch
        {
            DbProviderType.SqlServer
                => new Microsoft.Data.SqlClient.SqlConnection(connectionString),
            DbProviderType.MySql => new MySql.Data.MySqlClient.MySqlConnection(connectionString),
            DbProviderType.PostgreSql => new Npgsql.NpgsqlConnection(connectionString),
            DbProviderType.Sqlite => GetSQLiteConnection(connectionString),
            _
                => throw new NotSupportedException(
                    $"The provider type {providerType} is not supported."
                ),
        };
    }

    private static System.Data.SQLite.SQLiteConnection GetSQLiteConnection(string connectionString)
    {
        var sqliteConnectionStringBuilder = new System.Data.SQLite.SQLiteConnectionStringBuilder(
            connectionString
        );
        var ds = sqliteConnectionStringBuilder.DataSource;
        if (!File.Exists(ds))
        {
            var dir = Path.GetDirectoryName(ds)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            System.Data.SQLite.SQLiteConnection.CreateFile(ds);
        }
        sqliteConnectionStringBuilder.ForeignKeys = true;
        sqliteConnectionStringBuilder.BinaryGUID = false;
        sqliteConnectionStringBuilder.DateTimeFormat = System.Data.SQLite.SQLiteDateFormats.ISO8601;
        sqliteConnectionStringBuilder.JournalMode = System.Data.SQLite.SQLiteJournalModeEnum.Wal;
        sqliteConnectionStringBuilder.SyncMode = System.Data.SQLite.SynchronizationModes.Full;
        sqliteConnectionStringBuilder.CacheSize = 10000;
        sqliteConnectionStringBuilder.PageSize = 4096;
        sqliteConnectionStringBuilder.LegacyFormat = false;
        sqliteConnectionStringBuilder.Pooling = true;
        sqliteConnectionStringBuilder.DefaultTimeout = 30;
        sqliteConnectionStringBuilder.FailIfMissing = false;
        sqliteConnectionStringBuilder.ReadOnly = false;
        sqliteConnectionStringBuilder.UseUTF16Encoding = false;
        return new System.Data.SQLite.SQLiteConnection(connectionString);
    }
}
