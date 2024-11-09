using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

[Serializable]
public class DxForeignKeyConstraint : DxConstraint
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxForeignKeyConstraint()
        : base("") { }

    [SetsRequiredMembers]
    public DxForeignKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] sourceColumns,
        string referencedTableName,
        DxOrderedColumn[] referencedColumns,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
        : base(constraintName)
    {
        if (sourceColumns.Length != referencedColumns.Length)
            throw new ArgumentException(
                "SourceColumns and ReferencedColumns must have the same number of columns."
            );

        SchemaName = schemaName;
        TableName = tableName;
        SourceColumns = sourceColumns;
        ReferencedTableName = referencedTableName;
        ReferencedColumns = referencedColumns;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    public string? SchemaName { get; set; }
    public required string TableName { get; set; }
    public required DxOrderedColumn[] SourceColumns { get; set; }
    public required string ReferencedTableName { get; set; }
    public required DxOrderedColumn[] ReferencedColumns { get; set; }
    public DxForeignKeyAction OnDelete { get; set; } = DxForeignKeyAction.NoAction;
    public DxForeignKeyAction OnUpdate { get; set; } = DxForeignKeyAction.NoAction;

    public override DxConstraintType ConstraintType => DxConstraintType.ForeignKey;
}
