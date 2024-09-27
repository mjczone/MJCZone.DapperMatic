using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Class,
    Inherited = true,
    AllowMultiple = true
)]
public class DxUniqueConstraintAttribute : Attribute
{
    public DxUniqueConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames?.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public DxUniqueConstraintAttribute(params string[] columnNames)
    {
        Columns = columnNames?.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public DxUniqueConstraintAttribute(string constraintName, params DxOrderedColumn[] columns)
    {
        ConstraintName = constraintName;
        Columns = columns;
    }

    public DxUniqueConstraintAttribute(params DxOrderedColumn[] columns)
    {
        Columns = columns;
    }

    public string? ConstraintName { get; }
    public DxOrderedColumn[]? Columns { get; }
}
