using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a unique constraint on a table.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DmUniqueConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmUniqueConstraintAttribute"/> class with a constraint name and column names.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="columnNames">The column names that form the unique constraint.</param>
    public DmUniqueConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames.Select(columnName => new DmOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmUniqueConstraintAttribute"/> class with column names.
    /// </summary>
    /// <param name="columnNames">The column names that form the unique constraint.</param>
    public DmUniqueConstraintAttribute(params string[] columnNames)
    {
        Columns = columnNames.Select(columnName => new DmOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmUniqueConstraintAttribute"/> class with a constraint name and ordered columns.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="columns">The ordered columns that form the unique constraint.</param>
    public DmUniqueConstraintAttribute(string constraintName, params DmOrderedColumn[] columns)
    {
        ConstraintName = constraintName;
        Columns = columns;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmUniqueConstraintAttribute"/> class with ordered columns.
    /// </summary>
    /// <param name="columns">The ordered columns that form the unique constraint.</param>
    public DmUniqueConstraintAttribute(params DmOrderedColumn[] columns)
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
    public DmOrderedColumn[]? Columns { get; }
}
