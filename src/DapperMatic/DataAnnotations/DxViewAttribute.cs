namespace DapperMatic.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DxViewAttribute : Attribute
{
    public DxViewAttribute() { }

    /// <summary>
    /// A view definition as an attribute.
    /// </summary>
    /// <param name="definition">The SQL definition for the view. Use '{0}' to represent the schema name.</param>
    public DxViewAttribute(string definition)
    {
        Definition = definition;
    }

    /// <summary>
    /// A view definition as an attribute.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="viewName"></param>
    /// <param name="definition">The SQL definition for the view. Use '{0}' to represent the schema name.</param>
    public DxViewAttribute(string? schemaName, string? viewName, string definition)
    {
        SchemaName = schemaName;
        ViewName = viewName;
        Definition = definition;
    }

    public string? SchemaName { get; }
    public string? ViewName { get; }
    public string? Definition { get; }
}
