namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response containing a list of databases.
/// </summary>
public class DatabaseResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public DatabaseResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseResponse"/> class.
    /// </summary>
    /// <param name="data">The list of databases.</param>
    public DatabaseResponse(DatabaseEntry data)
    {
        Results = data;
    }

    /// <summary>
    /// Gets or sets the list of databases.
    /// </summary>
    public DatabaseEntry? Results { get; set; }
}
