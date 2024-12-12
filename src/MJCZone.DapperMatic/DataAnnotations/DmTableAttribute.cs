namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define the table name and schema for a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DmTableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmTableAttribute"/> class.
    /// </summary>
    public DmTableAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmTableAttribute"/> class with a table name.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    public DmTableAttribute(string? tableName)
    {
        TableName = tableName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmTableAttribute"/> class with a schema name and table name.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="tableName">The name of the table.</param>
    public DmTableAttribute(string? schemaName, string? tableName)
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
