using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a unique constraint on a table.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DxUniqueConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxUniqueConstraintAttribute"/> class with a constraint name and column names.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="columnNames">The column names that form the unique constraint.</param>
    public DxUniqueConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxUniqueConstraintAttribute"/> class with column names.
    /// </summary>
    /// <param name="columnNames">The column names that form the unique constraint.</param>
    public DxUniqueConstraintAttribute(params string[] columnNames)
    {
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxUniqueConstraintAttribute"/> class with a constraint name and ordered columns.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="columns">The ordered columns that form the unique constraint.</param>
    public DxUniqueConstraintAttribute(string constraintName, params DxOrderedColumn[] columns)
    {
        ConstraintName = constraintName;
        Columns = columns;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxUniqueConstraintAttribute"/> class with ordered columns.
    /// </summary>
    /// <param name="columns">The ordered columns that form the unique constraint.</param>
    public DxUniqueConstraintAttribute(params DxOrderedColumn[] columns)
    {
        Columns = columns;
    }

    /// <summary>
    /// Gets the name of the constraint.
    /// </summary>
    public string? ConstraintName { get; }

    /// <summary>
    /// Gets the columns that form the unique constraint.
    /// </summary>
    public DxOrderedColumn[]? Columns { get; }
}
