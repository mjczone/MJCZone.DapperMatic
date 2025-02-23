namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Defines the contract for managing query operations.
/// </summary>
public interface IQueryOperationManager
{
    /// <summary>
    /// Asynchronously retrieves a collection of query operations.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="DatabaseOperation"/>.</returns>
    Task<IEnumerable<DatabaseOperation>> GetQueryOperationsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously retrieves a specific query operation by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the query operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="DatabaseOperation"/>.</returns>
    Task<DatabaseOperation> GetQueryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously adds a new query operation.
    /// </summary>
    /// <param name="query">The query operation to add.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the addition was successful.</returns>
    Task<bool> AddQueryAsync(
        DatabaseOperation query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously updates an existing query operation.
    /// </summary>
    /// <param name="id">The unique identifier of the query operation to update.</param>
    /// <param name="query">The updated query operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the update was successful.</returns>
    Task<bool> UpdateQueryOperationAsync(
        Guid id,
        DatabaseOperation query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asynchronously deletes a query operation by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the query operation to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the deletion was successful.</returns>
    Task<bool> DeleteQueryOperationAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes a query operation and returns the result.
    /// </summary>
    /// <param name="id">The unique identifier of the query operation to execute.</param>
    /// <param name="parameters">The query execution parameters for the operation.</param>
    /// /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the query operation.</returns>
    Task<object> ExecuteQueryOperationAsync(
        Guid id,
        Dictionary<string, object?>? parameters,
        CancellationToken cancellationToken = default
    );
}
