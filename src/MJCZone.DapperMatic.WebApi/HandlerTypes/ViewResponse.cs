using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response containing a view.
/// </summary>
public class ViewResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewResponse"/> class.
    /// </summary>
    public ViewResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewResponse"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    public ViewResponse(DmView view)
    {
        Results = view;
    }

    /// <summary>
    /// Gets or sets the view results.
    /// </summary>
    public DmView? Results { get; set; }
}
