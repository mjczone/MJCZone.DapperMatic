using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

/// <summary>
/// Represents a view in a database.
/// </summary>
[Serializable]
public class DxView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxView"/> class.
    /// Used for deserialization.
    /// </summary>
    public DxView() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxView"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name of the view.</param>
    /// <param name="viewName">The name of the view.</param>
    /// <param name="definition">The definition of the view.</param>
    [SetsRequiredMembers]
    public DxView(string? schemaName, string viewName, string definition)
    {
        SchemaName = schemaName;
        ViewName = viewName;
        Definition = definition;
    }

    /// <summary>
    /// Gets or sets the schema name of the view.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the name of the view.
    /// </summary>
    public required string ViewName { get; set; }

    /// <summary>
    /// Gets or sets the definition of the view.
    /// </summary>
    public required string Definition { get; set; }
}
