using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Class,
    Inherited = false,
    AllowMultiple = true
)]
public class DxPrimaryKeyConstraintAttribute : Attribute
{
    public DxPrimaryKeyConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames?.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public DxPrimaryKeyConstraintAttribute(params string[] columnNames)
    {
        Columns = columnNames?.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public string? ConstraintName { get; }
    public DxOrderedColumn[]? Columns { get; }
}
