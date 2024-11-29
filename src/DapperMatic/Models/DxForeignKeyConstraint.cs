using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

/// <summary>
/// Represents a foreign key constraint in a database.
/// </summary>
[Serializable]
public class DxForeignKeyConstraint : DxConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxForeignKeyConstraint"/> class.
    /// Used for deserialization.
    /// </summary>
    public DxForeignKeyConstraint()
        : base(string.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxForeignKeyConstraint"/> class.
    /// </summary>
    /// <param name="schemaName">The name of the schema containing the table with the foreign key constraint. Can be null if not specified, indicating the default schema.</param>
    /// <param name="tableName">The name of the table that contains the foreign key constraint.</param>
    /// <param name="constraintName">The desired name for the new foreign key constraint.</param>
    /// <param name="sourceColumns">An array of DxOrderedColumn objects representing the columns in the source table that are part of the foreign key.</param>
    /// <param name="referencedTableName">The name of the table that is referenced by the foreign key constraint.</param>
    /// <param name="referencedColumns">An array of DxOrderedColumn objects representing the columns in the referenced table that correspond to the source columns.</param>
    /// <param name="onDelete">
    ///     The action to take when a row in the referenced table is deleted. Defaults to <see cref="DxForeignKeyAction.NoAction"/>.
    /// </param>
    /// <param name="onUpdate">
    ///     The action to take when a row in the referenced table is updated. Defaults to <see cref="DxForeignKeyAction.NoAction"/>.
    /// </param>
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
        {
            throw new ArgumentException(
                "SourceColumns and ReferencedColumns must have the same number of columns."
            );
        }

        SchemaName = schemaName;
        TableName = tableName;
        SourceColumns = [.. sourceColumns];
        ReferencedTableName = referencedTableName;
        ReferencedColumns = [.. referencedColumns];
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public required string TableName { get; set; }

    /// <summary>
    /// Gets or sets the source columns.
    /// </summary>
    public required List<DxOrderedColumn> SourceColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the referenced table name.
    /// </summary>
    public required string ReferencedTableName { get; set; }

    /// <summary>
    /// Gets or sets the referenced columns.
    /// </summary>
    public required List<DxOrderedColumn> ReferencedColumns { get; set; } = [];

    /// <summary>
    /// Gets or sets the action on delete.
    /// </summary>
    public DxForeignKeyAction OnDelete { get; set; } = DxForeignKeyAction.NoAction;

    /// <summary>
    /// Gets or sets the action on update.
    /// </summary>
    public DxForeignKeyAction OnUpdate { get; set; } = DxForeignKeyAction.NoAction;

    /// <summary>
    /// Gets the constraint type.
    /// </summary>
    public override DxConstraintType ConstraintType => DxConstraintType.ForeignKey;
}
