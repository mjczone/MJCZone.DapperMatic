using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a list of views.
/// </summary>
public class ViewListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewListResponse"/> class.
    /// </summary>
    public ViewListResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewListResponse"/> class.
    /// </summary>
    /// <param name="views">The list of views.</param>
    public ViewListResponse(List<DmView> views)
    {
        Results = views;
    }

    /// <summary>
    /// Gets or sets the list of views.
    /// </summary>
    public List<DmView>? Results { get; set; }
}
