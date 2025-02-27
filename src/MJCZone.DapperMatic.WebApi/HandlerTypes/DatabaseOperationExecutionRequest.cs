namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a request for executing a database operation.
/// </summary>
public class DatabaseOperationExecutionRequest
{
    /// <summary>
    /// Gets or sets the name of the database operation.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = [];

    /// <summary>
    /// Converts the <see cref="DatabaseOperationExecutionRequest"/> to a <see cref="DatabaseOperationExecution"/>.
    /// </summary>
    /// <returns>The <see cref="DatabaseOperationExecution"/>.</returns>
    internal DatabaseOperationExecution ToDatabaseOperationExecution()
    {
        throw new NotImplementedException();
    }
}
