using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

/// <summary>
/// Represents a column in an ordered list of columns.
/// </summary>
[Serializable]
public class DxOrderedColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxOrderedColumn"/> class.
    /// Used for deserialization.
    /// </summary>
    public DxOrderedColumn() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxOrderedColumn"/> class.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="order">Order of the column.</param>
    [SetsRequiredMembers]
    public DxOrderedColumn(string columnName, DxColumnOrder order = DxColumnOrder.Ascending)
    {
        ColumnName = columnName;
        Order = order;
    }

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public required string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the order of the column.
    /// </summary>
    public required DxColumnOrder Order { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => ToString(true);

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <param name="includeOrder">if set to <c>true</c> includes the order in the string.</param>
    /// <returns>A string that represents the current object.</returns>
    public string ToString(bool includeOrder) =>
        $"{ColumnName}{(includeOrder ? Order == DxColumnOrder.Descending ? " DESC" : string.Empty : string.Empty)}";
}
