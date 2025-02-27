using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a list of tables.
/// </summary>
public class TableListResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableListResponse"/> class.
    /// </summary>
    public TableListResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableListResponse"/> class.
    /// </summary>
    /// <param name="tables">The list of tables.</param>
    public TableListResponse(List<DmTable> tables)
    {
        Results = tables;
    }

    /// <summary>
    /// Gets or sets the list of tables.
    /// </summary>
    public List<DmTable>? Results { get; set; }
}
