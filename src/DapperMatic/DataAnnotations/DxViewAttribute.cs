namespace DapperMatic.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DxViewAttribute : Attribute
{
    public DxViewAttribute() { }

    public DxViewAttribute(string definition)
    {
        Definition = definition;
    }

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
