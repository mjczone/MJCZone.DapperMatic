namespace DapperMatic.DataAnnotations;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class DxTableAttribute : Attribute
{
    public DxTableAttribute() { }

    public DxTableAttribute(string? tableName)
    {
        TableName = tableName;
    }

    public DxTableAttribute(string? schemaName, string? tableName)
    {
        SchemaName = schemaName;
        TableName = tableName;
    }

    public string? SchemaName { get; }
    public string? TableName { get; }
}
