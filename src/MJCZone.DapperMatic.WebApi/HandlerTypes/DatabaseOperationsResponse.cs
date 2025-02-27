namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response containing a list of databases.
/// </summary>
public class DatabaseOperationsResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOperationsResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public DatabaseOperationsResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseOperationsResponse"/> class.
    /// </summary>
    /// <param name="data">The list of databases.</param>
    public DatabaseOperationsResponse(List<DatabaseOperation> data)
    {
        Results = data;
    }

    /// <summary>
    /// Gets or sets the list of databases.
    /// </summary>
    public List<DatabaseOperation>? Results { get; set; }
}
