namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a database operation.
/// </summary>
public class DatabaseOperationResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOperationResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public DatabaseOperationResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOperationResponse"/> class.
    /// </summary>
    /// <param name="data">The database operation.</param>
    public DatabaseOperationResponse(DatabaseOperation data)
    {
        Results = data;
    }

    /// <summary>
    /// Gets or sets the database operation.
    /// </summary>
    public DatabaseOperation? Results { get; set; }
}
