namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a database operation.
/// </summary>
public class DatabaseOperationExecutionResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOperationExecutionResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public DatabaseOperationExecutionResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOperationExecutionResponse"/> class.
    /// </summary>
    /// <param name="data">The database operation.</param>
    public DatabaseOperationExecutionResponse(object data)
    {
        Results = data;
    }

    /// <summary>
    /// Gets or sets the execution results.
    /// </summary>
    public object? Results { get; set; }
}
