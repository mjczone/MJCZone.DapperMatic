namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a database view.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DmViewAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmViewAttribute"/> class.
    /// </summary>
    public DmViewAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmViewAttribute"/> class.
    /// </summary>
    /// <param name="definition">The SQL definition for the view. Use '{0}' to represent the schema name.</param>
    public DmViewAttribute(string definition)
    {
        Definition = definition;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmViewAttribute"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="viewName">The view name.</param>
    /// <param name="definition">The SQL definition for the view. Use '{0}' to represent the schema name.</param>
    public DmViewAttribute(string? schemaName, string? viewName, string definition)
    {
        SchemaName = schemaName;
        ViewName = viewName;
        Definition = definition;
    }

    /// <summary>
    /// Gets the schema name.
    /// </summary>
    public string? SchemaName { get; }

    /// <summary>
    /// Gets the view name.
    /// </summary>
    public string? ViewName { get; }

    /// <summary>
    /// Gets the SQL definition for the view.
    /// </summary>
    public string? Definition { get; }
}
