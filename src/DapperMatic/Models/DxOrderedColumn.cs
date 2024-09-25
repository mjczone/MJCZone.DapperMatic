using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

[Serializable]
public class DxOrderedColumn
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxOrderedColumn() { }

    [SetsRequiredMembers]
    public DxOrderedColumn(string columnName, DxColumnOrder order = DxColumnOrder.Ascending)
    {
        ColumnName = columnName;
        Order = order;
    }

    public required string ColumnName { get; set; }
    public required DxColumnOrder Order { get; set; }

    public override string ToString() =>
        $"{ColumnName}{(Order == DxColumnOrder.Descending ? " DESC" : "")}";
}
