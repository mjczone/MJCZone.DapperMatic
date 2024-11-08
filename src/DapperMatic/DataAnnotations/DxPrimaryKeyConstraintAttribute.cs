using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public class DxPrimaryKeyConstraintAttribute : Attribute
{
    public DxPrimaryKeyConstraintAttribute() { }

    public DxPrimaryKeyConstraintAttribute(string constraintName)
    {
        ConstraintName = constraintName;
    }

    public DxPrimaryKeyConstraintAttribute(string constraintName, params string[] columnNames)
    {
        ConstraintName = constraintName;
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public DxPrimaryKeyConstraintAttribute(string[] columnNames)
    {
        Columns = columnNames.Select(columnName => new DxOrderedColumn(columnName)).ToArray();
    }

    public string? ConstraintName { get; }
    public DxOrderedColumn[]? Columns { get; }
}
