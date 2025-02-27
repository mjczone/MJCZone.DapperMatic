namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response containing a boolean.
/// </summary>
public class BoolResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoolResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is required for deserialization.
    /// </remarks>
    public BoolResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoolResponse"/> class.
    /// </summary>
    /// <param name="data">The result.</param>
    public BoolResponse(bool data)
    {
        Results = data;
    }

    /// <summary>
    /// Gets or sets the result.
    /// </summary>
    /// <value>The result.</value>
    public bool? Results { get; set; }
}
