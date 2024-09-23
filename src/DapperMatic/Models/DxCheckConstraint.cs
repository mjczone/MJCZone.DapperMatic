using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

public class DxCheckConstraint : DxConstraint
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxCheckConstraint()
        : base("") { }

    [SetsRequiredMembers]
    public DxCheckConstraint(
        string? schemaName,
        string tableName,
        string? columnName,
        string constraintName,
        string expression
    )
        : base(constraintName)
    {
        SchemaName = schemaName;
        TableName = string.IsNullOrWhiteSpace(tableName)
            ? throw new ArgumentException("Table name cannot be null or empty")
            : tableName;
        ColumnName = columnName;
        Expression = string.IsNullOrWhiteSpace(expression)
            ? throw new ArgumentException("Expression cannot be null or empty")
            : expression;
    }

    public string? SchemaName { get; set; }
    public required string TableName { get; init; }
    public string? ColumnName { get; set; }
    public required string Expression { get; init; }

    public override DxConstraintType ConstraintType => DxConstraintType.Check;

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(ColumnName))
            return $"{ConstraintType} Constraint on {TableName} with expression: {Expression}";

        return $"{ConstraintType} Constraint on {TableName}.{ColumnName} with expression: {Expression}";
    }
}
