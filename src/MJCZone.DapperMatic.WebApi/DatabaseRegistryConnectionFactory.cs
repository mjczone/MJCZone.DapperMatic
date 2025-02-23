namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Factory class for creating database connections from a registry.
/// </summary>
public class DatabaseRegistryConnectionFactory : IDatabaseRegistryConnectionFactory
{
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="IDbConnection"/>.</returns>
    /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
    public Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
