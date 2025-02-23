namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Represents a factory for creating database connections.
/// </summary>
public interface IDatabaseConnectionFactory
{
    /// <summary>
    /// Creates a new database connection for the specified database ID.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier or null.</param>
    /// <param name="databaseIdOrSlug">The unique identifier of the database.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An instance of <see cref="IDbConnection"/> representing the database connection.</returns>
    Task<IDbConnection> OpenConnectionAsync(
        string? tenantIdentifier,
        string databaseIdOrSlug,
        CancellationToken cancellationToken = default
    );
}
