using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

/// <summary>
/// Represents a unique constraint on a table.
/// </summary>
[Serializable]
public class DxUniqueConstraint : DxConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxUniqueConstraint"/> class.
    /// Used for deserialization.
    /// </summary>
    public DxUniqueConstraint()
        : base(string.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxUniqueConstraint"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="columns">The columns.</param>
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
    public required List<DxOrderedColumn> Columns { get; set; } = [];

    /// <summary>
    /// Gets the type of the constraint.
    /// </summary>
    public override DxConstraintType ConstraintType => DxConstraintType.Unique;
}
