using System.Diagnostics.CodeAnalysis;

namespace MJCZone.DapperMatic.Models;

/// <summary>
/// Represents a primary key constraint on a table.
/// </summary>
[Serializable]
public class DmPrimaryKeyConstraint : DmConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmPrimaryKeyConstraint"/> class.
    /// Used for deserialization.
    /// </summary>
    public DmPrimaryKeyConstraint()
        : base(string.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmPrimaryKeyConstraint"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="columns">The columns that make up the primary key.</param>
    [SetsRequiredMembers]
    public DmPrimaryKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DmOrderedColumn[] columns
    )
        : base(constraintName)
    {
        SchemaName = schemaName;
        TableName = tableName;
        Columns = [.. columns];
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
    /// Gets or sets the columns.
    /// </summary>
    public required List<DmOrderedColumn> Columns { get; set; } = [];

    /// <summary>
    /// Gets the type of the constraint.
    /// </summary>
    public override DmConstraintType ConstraintType => DmConstraintType.PrimaryKey;
}
