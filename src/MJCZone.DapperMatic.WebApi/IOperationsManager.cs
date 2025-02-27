namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Defines the contract for managing database operations.
/// </summary>
public interface IOperationsManager
{
    /// <summary>
    /// Asynchronously retrieves a collection of database operations.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier for filtering operations.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="DatabaseOperation"/>.</returns>
    Task<IEnumerable<DatabaseOperation>> GetOperationsAsync(
        string? tenantIdentifier,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously retrieves a specific database operation by its identifier.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier for filtering operations.</param>
    /// <param name="idOrSlug">The unique identifier or slug of the database operation to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="DatabaseOperation"/>.</returns>
    Task<DatabaseOperation> GetOperationAsync(
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously adds a new database operation.
    /// </summary>
    /// <param name="operation">The database operation to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the addition was successful.</returns>
    Task<bool> AddOperationAsync(
        DatabaseOperation operation,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously updates an existing database operation.
    /// </summary>
    /// <param name="operation">The updated database operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the update was successful.</returns>
    Task<bool> PatchOperationAsync(
        DatabaseOperation operation,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously deletes a database operation by its identifier.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier for filtering operations.</param>
    /// <param name="idOrSlug">The unique identifier or slug of the database operation to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
    Task<bool> DeleteOperationAsync(
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously executes a database operation and returns the result.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier for filtering operations.</param>
    /// <param name="idOrSlug">The unique identifier or slug of the database operation to execute.</param>
    /// <param name="parameters">The database execution parameters for the operation.</param>
    /// /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the query operation.</returns>
    Task<object> ExecuteOperationAsync(
        string? tenantIdentifier,
        string idOrSlug,
        Dictionary<string, object?>? parameters,
        CancellationToken cancellationToken = default
    );
}
