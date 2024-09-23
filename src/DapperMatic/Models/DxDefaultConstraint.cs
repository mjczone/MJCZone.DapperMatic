using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

public class DxDefaultConstraint : DxConstraint
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxDefaultConstraint()
        : base("") { }

    [SetsRequiredMembers]
    public DxDefaultConstraint(
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
            ? throw new ArgumentException("Table name cannot be null or empty")
            : tableName;
        ColumnName = string.IsNullOrWhiteSpace(columnName)
            ? throw new ArgumentException("Column name cannot be null or empty")
            : columnName;
        Expression = string.IsNullOrWhiteSpace(expression)
            ? throw new ArgumentException("Expression cannot be null or empty")
            : expression;
    }

    public string? SchemaName { get; set; }
    public required string TableName { get; init; }
    public required string ColumnName { get; init; }
    public required string Expression { get; init; }

    public override DxConstraintType ConstraintType => DxConstraintType.Default;

    public override string ToString()
    {
        return $"{ConstraintType} Constraint on {TableName}.{ColumnName} with expression: {Expression}";
    }
}
