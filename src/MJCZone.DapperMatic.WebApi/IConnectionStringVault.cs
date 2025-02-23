namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides methods to resolve and manage connection strings.
/// </summary>
public interface IConnectionStringVault
{
    /// <summary>
    /// Retrieves the connection string associated with the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection string associated with the specified name, or null if not found.</returns>
    Task<string?> GetConnectionStringAsync(
        string connectionStringName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="connectionString">The connection string to associate with the specified name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetConnectionStringAsync(
        string connectionStringName,
        string connectionString,
        CancellationToken cancellationToken = default
    );
}
