using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

[Serializable]
public class DxUniqueConstraint : DxConstraint
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxUniqueConstraint()
        : base("") { }

    [SetsRequiredMembers]
    public DxUniqueConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns
    )
        : base(constraintName)
    {
        SchemaName = schemaName;
        TableName = tableName;
        Columns = columns;
    }

    public string? SchemaName { get; set; }
    public required string TableName { get; set; }
    public required DxOrderedColumn[] Columns { get; set; }

    public override DxConstraintType ConstraintType => DxConstraintType.Unique;
}
