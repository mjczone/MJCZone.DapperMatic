using Microsoft.Extensions.Options;

using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Factory class for creating database connections from a registry.
/// </summary>
public class DatabaseRegistryConnectionFactory : IDatabaseRegistryConnectionFactory
{
    private readonly IOptionsMonitor<DapperMaticOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRegistryConnectionFactory"/> class.
    /// </summary>
    /// <param name="options">The options for configuring DapperMatic.</param>
    public DatabaseRegistryConnectionFactory(IOptionsMonitor<DapperMaticOptions> options)
    {
        _options = options;
    }

    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="IDbConnection"/>.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public async Task<IDbConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connectionString = _options.CurrentValue.DatabaseRegistry?.ConnectionString;
        var providerType = _options.CurrentValue.DatabaseRegistry?.ProviderType;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.");
        }

        if (providerType == null)
        {
            throw new ArgumentException("Provider type cannot be null or empty.");
        }

        var connection = DatabaseConnectionFactory.GetDbConnection(connectionString, providerType);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
