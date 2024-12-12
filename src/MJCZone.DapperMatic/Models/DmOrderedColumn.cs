using System.Diagnostics.CodeAnalysis;

namespace MJCZone.DapperMatic.Models;

/// <summary>
/// Represents a column in an ordered list of columns.
/// </summary>
[Serializable]
public class DmOrderedColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmOrderedColumn"/> class.
    /// Used for deserialization.
    /// </summary>
    public DmOrderedColumn() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmOrderedColumn"/> class.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="order">Order of the column.</param>
    [SetsRequiredMembers]
    public DmOrderedColumn(string columnName, DmColumnOrder order = DmColumnOrder.Ascending)
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
    public required DmColumnOrder Order { get; set; }

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
        $"{ColumnName}{(includeOrder ? Order == DmColumnOrder.Descending ? " DESC" : string.Empty : string.Empty)}";
}
