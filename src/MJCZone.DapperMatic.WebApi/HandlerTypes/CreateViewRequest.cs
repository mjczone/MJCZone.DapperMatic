namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a request to create a table.
/// </summary>
public class CreateViewRequest
{
    /// <summary>
    /// Gets or sets the name of the view.
    /// </summary>
    public required string ViewName { get; set; }

    /// <summary>
    /// Gets or sets the sql definition of the view.
    /// </summary>
    public required string Definition { get; set; }
}
