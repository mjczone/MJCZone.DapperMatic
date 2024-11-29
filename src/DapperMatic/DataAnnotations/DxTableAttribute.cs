namespace DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define the table name and schema for a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DxTableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxTableAttribute"/> class.
    /// </summary>
    public DxTableAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxTableAttribute"/> class with a table name.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    public DxTableAttribute(string? tableName)
    {
        TableName = tableName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxTableAttribute"/> class with a schema name and table name.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    public DxTableAttribute(string? schemaName, string? tableName)
    {
        SchemaName = schemaName;
        TableName = tableName;
    }

    /// <summary>
    /// Gets the name of the schema.
    /// </summary>
    public string? SchemaName { get; }

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string? TableName { get; }
}
