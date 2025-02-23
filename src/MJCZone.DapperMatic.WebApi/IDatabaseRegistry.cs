namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides database-related operations.
/// </summary>
public interface IDatabaseRegistry
{
    /// <summary>
    /// Initializes the database registry.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new database.
    /// </summary>
    /// <param name="database">The database to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an updated version of the database that was saved.</returns>
    Task<DatabaseEntry> AddDatabaseAsync(
        DatabaseEntry database,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a database by its ID.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="idOrSlug">The ID or slug of the database to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the database was deleted successfully; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteDatabaseAsync(
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a database by its ID.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="idOrSlug">The ID or slug of the database to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The database with the specified ID, or <c>null</c> if not found.</returns>
    Task<DatabaseEntry?> GetDatabaseAsync(
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a list of all databases.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all databases.</returns>
    Task<IEnumerable<DatabaseEntry>> GetDatabasesAsync(
        string? tenantIdentifier,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates an existing database.
    /// </summary>
    /// <param name="database">The updated database information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated database entry.</returns>
    Task<DatabaseEntry> PatchDatabaseAsync(
        DatabaseEntry database,
        CancellationToken cancellationToken = default
    );
}
