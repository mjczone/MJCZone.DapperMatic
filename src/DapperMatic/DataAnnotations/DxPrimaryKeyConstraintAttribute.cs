using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a primary key constraint on a table.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DxPrimaryKeyConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxPrimaryKeyConstraintAttribute"/> class.
    /// </summary>
    public DxPrimaryKeyConstraintAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxPrimaryKeyConstraintAttribute"/> class with a constraint name.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    public DxPrimaryKeyConstraintAttribute(string constraintName)
    {
        ConstraintName = constraintName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxPrimaryKeyConstraintAttribute"/> class with a constraint name and column names.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="columnNames">The column names that form the primary key constraint.</param>
    public DxPrimaryKeyConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxPrimaryKeyConstraintAttribute"/> class with column names.
    /// </summary>
    /// <param name="columnNames">The column names that form the primary key constraint.</param>
    public DxPrimaryKeyConstraintAttribute(string[] columnNames)
    {
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Gets the name of the constraint.
    /// </summary>
    public string? ConstraintName { get; }

    /// <summary>
    /// Gets the columns that form the primary key constraint.
    /// </summary>
    public DxOrderedColumn[]? Columns { get; }
}
