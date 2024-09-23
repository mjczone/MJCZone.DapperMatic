using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

public class DxPrimaryKeyConstraint : DxConstraint
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxPrimaryKeyConstraint()
        : base("") { }

    [SetsRequiredMembers]
    public DxPrimaryKeyConstraint(
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

    public override DxConstraintType ConstraintType => DxConstraintType.PrimaryKey;
}
