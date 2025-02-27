namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a request to update a view with various modifications.
/// </summary>
public class UpdateViewRequest
{
    /// <summary>
    /// Gets or sets the new name for the view.
    /// </summary>
    public string? RenameViewTo { get; set; }

    /// <summary>
    /// Gets or sets the new SQL query for the view.
    /// </summary>
    public string? Definition { get; set; }
}
