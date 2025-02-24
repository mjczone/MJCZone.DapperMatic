namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response containing a list of databases.
/// </summary>
public class DatabasesResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabasesResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public DatabasesResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabasesResponse"/> class.
    /// </summary>
    /// <param name="data">The list of databases.</param>
    public DatabasesResponse(List<DatabaseEntry> data)
    {
        Results = data;
    }

    /// <summary>
    /// Gets or sets the list of databases.
    /// </summary>
    public List<DatabaseEntry>? Results { get; set; }
}
