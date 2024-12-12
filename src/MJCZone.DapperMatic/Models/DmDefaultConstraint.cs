using System.Diagnostics.CodeAnalysis;

namespace MJCZone.DapperMatic.Models;

/// <summary>
/// Represents a default constraint on a table.
/// </summary>
[Serializable]
public class DmDefaultConstraint : DmConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmDefaultConstraint"/> class.
    /// Used for deserialization.
    /// </summary>
    public DmDefaultConstraint()
        : base(string.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmDefaultConstraint"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="expression">The expression.</param>
    [SetsRequiredMembers]
    public DmDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression
    )
        : base(constraintName)
    {
        SchemaName = schemaName;
        TableName = string.IsNullOrWhiteSpace(tableName)
            ? throw new ArgumentException("Table name is required")
            : tableName;
        ColumnName = string.IsNullOrWhiteSpace(columnName)
            ? throw new ArgumentException("Column name is required")
            : columnName;
        Expression = string.IsNullOrWhiteSpace(expression)
            ? throw new ArgumentException("Expression is required")
            : expression;
    }

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets the table name.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets the column name.
    /// </summary>
    public required string ColumnName { get; init; }

    /// <summary>
    /// Gets the expression.
    /// </summary>
    public required string Expression { get; init; }

    /// <summary>
    /// Gets the constraint type.
    /// </summary>
    public override DmConstraintType ConstraintType => DmConstraintType.Default;

    /// <summary>
    /// Returns a string representation of the constraint.
    /// </summary>
    /// <returns>A string representation of the constraint.</returns>
    public override string ToString()
    {
        return $"{ConstraintType} Constraint on {TableName}.{ColumnName} with expression: {Expression}";
    }
}
