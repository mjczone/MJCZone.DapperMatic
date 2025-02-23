namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides a factory interface for creating the database connection
/// to store database entries.
/// </summary>
public interface IDatabaseRegistryConnectionFactory
{
    /// <summary>
    /// Creates and returns a new database connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="IDbConnection"/> representing the database connection.</returns>
    Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
