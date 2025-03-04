namespace MJCZone.DapperMatic.WebApi.Vaults;

/// <summary>
/// Provides methods to resolve and manage connection strings.
/// </summary>
public interface IConnectionStringsVault
{
    /// <summary>
    /// Gets the name of the connection string vault.
    /// </summary>
    /// <value>The name of the connection string vault.</value>
    /// <remarks>
    /// The name of the connection string vault is used to identify the vault in which a connection string is stored at runtime.
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the connection string vault is read-only.
    /// </summary>
    /// <value><c>true</c> if the connection string vault is read-only; otherwise, <c>false</c>.</value>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets the roles associated with the connection string vault.
    /// </summary>
    /// <returns>An array of role names associated with the connection string vault.</returns>
    string[] GetRoles();

    /// <summary>
    /// Retrieves the connection string associated with the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection string associated with the specified name, or null if not found.</returns>
    Task<string?> GetConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="connectionString">The connection string to associate with the specified name.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    Task SetConnectionStringAsync(
        string connectionStringName,
        string connectionString,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to delete.</param>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Clears the cache of connection strings.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    Task ClearCacheAsync();
}
