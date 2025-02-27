using Dapper;

namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a list of strings.
/// </summary>
public class StringListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringListResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public StringListResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringListResponse"/> class.
    /// </summary>
    /// <param name="data">The list of strings.</param>
    public StringListResponse(IEnumerable<string> data)
    {
        Results = data.AsList();
    }

    /// <summary>
    /// Gets or sets the list of strings.
    /// </summary>
    /// <value>The list of strings.</value>
    public List<string>? Results { get; set; }
}
