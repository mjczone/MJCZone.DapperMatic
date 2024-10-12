using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Class,
    AllowMultiple = true
)]
public class DxIndexAttribute : Attribute
{
    public DxIndexAttribute(string constraintName, bool isUnique, params string[] columnNames)
    {
        IndexName = constraintName;
        IsUnique = isUnique;
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public DxIndexAttribute(bool isUnique, params string[] columnNames)
    {
        IsUnique = isUnique;
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public DxIndexAttribute(string constraintName, bool isUnique, params DxOrderedColumn[] columns)
    {
        IndexName = constraintName;
        IsUnique = isUnique;
        Columns = columns;
    }

    public DxIndexAttribute(bool isUnique, params DxOrderedColumn[] columns)
    {
        IsUnique = isUnique;
        Columns = columns;
    }

    public string? IndexName { get; }
    public bool IsUnique { get; }
    public DxOrderedColumn[]? Columns { get; }
}
