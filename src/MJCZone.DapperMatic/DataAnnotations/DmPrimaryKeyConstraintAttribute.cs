using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a primary key constraint on a table.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DmPrimaryKeyConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmPrimaryKeyConstraintAttribute"/> class.
    /// </summary>
    public DmPrimaryKeyConstraintAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmPrimaryKeyConstraintAttribute"/> class with a constraint name.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    public DmPrimaryKeyConstraintAttribute(string constraintName)
    {
        ConstraintName = constraintName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmPrimaryKeyConstraintAttribute"/> class with a constraint name and column names.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="columnNames">The column names that form the primary key constraint.</param>
    public DmPrimaryKeyConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames.Select(columnName => new DmOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmPrimaryKeyConstraintAttribute"/> class with column names.
    /// </summary>
    /// <param name="columnNames">The column names that form the primary key constraint.</param>
    public DmPrimaryKeyConstraintAttribute(string[] columnNames)
    {
        Columns = columnNames.Select(columnName => new DmOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Gets the name of the constraint.
    /// </summary>
    public string? ConstraintName { get; }

    /// <summary>
    /// Gets the columns that form the primary key constraint.
    /// </summary>
    public DmOrderedColumn[]? Columns { get; }
}
