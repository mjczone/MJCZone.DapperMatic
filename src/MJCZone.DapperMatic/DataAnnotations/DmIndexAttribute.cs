using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a database index.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DmIndexAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmIndexAttribute"/> class with the specified constraint name, uniqueness, and column names.
    /// </summary>
    /// <param name="constraintName">The name of the index constraint.</param>
    /// <param name="isUnique">A value indicating whether the index is unique.</param>
    /// <param name="columnNames">The names of the columns included in the index.</param>
    public DmIndexAttribute(string constraintName, bool isUnique, params string[] columnNames)
    {
        IndexName = constraintName;
        IsUnique = isUnique;
        Columns = columnNames.Select(columnName => new DmOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmIndexAttribute"/> class with the specified uniqueness and column names.
    /// </summary>
    /// <param name="isUnique">A value indicating whether the index is unique.</param>
    /// <param name="columnNames">The names of the columns included in the index.</param>
    public DmIndexAttribute(bool isUnique, params string[] columnNames)
    {
        IsUnique = isUnique;
        Columns = columnNames.Select(columnName => new DmOrderedColumn(columnName)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmIndexAttribute"/> class with the specified constraint name, uniqueness, and columns.
    /// </summary>
    /// <param name="constraintName">The name of the index constraint.</param>
    /// <param name="isUnique">A value indicating whether the index is unique.</param>
    /// <param name="columns">The columns included in the index.</param>
    public DmIndexAttribute(string constraintName, bool isUnique, params DmOrderedColumn[] columns)
    {
        IndexName = constraintName;
        IsUnique = isUnique;
        Columns = columns;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmIndexAttribute"/> class with the specified uniqueness and columns.
    /// </summary>
    /// <param name="isUnique">A value indicating whether the index is unique.</param>
    /// <param name="columns">The columns included in the index.</param>
    public DmIndexAttribute(bool isUnique, params DmOrderedColumn[] columns)
    {
        IsUnique = isUnique;
        Columns = columns;
    }

    /// <summary>
    /// Gets the index name.
    /// </summary>
    public string? IndexName { get; }

    /// <summary>
    /// Gets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; }

    /// <summary>
    /// Gets the columns included in the index.
    /// </summary>
    public DmOrderedColumn[]? Columns { get; }
}
