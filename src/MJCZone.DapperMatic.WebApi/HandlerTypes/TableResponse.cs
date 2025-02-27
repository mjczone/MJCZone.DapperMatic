using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a response containing a table.
/// </summary>
public class TableResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableResponse"/> class.
    /// </summary>
    public TableResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableResponse"/> class.
    /// </summary>
    /// <param name="table">The table.</param>
    public TableResponse(DmTable table)
    {
        Results = table;
    }

    /// <summary>
    /// Gets or sets the table results.
    /// </summary>
    public DmTable? Results { get; set; }
}
