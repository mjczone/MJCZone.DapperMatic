using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DxColumnAttribute : Attribute
{
    public DxColumnAttribute(
        string? columnName = null,
        string? providerDataType = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? defaultExpression = null,
        bool isNullable = false,
        bool isPrimaryKey = false,
        bool isAutoIncrement = false,
        bool isUnique = false,
        bool isIndexed = false,
        bool isForeignKey = false,
        string? referencedTableName = null,
        string? referencedColumnName = null,
        DxForeignKeyAction? onDelete = null,
        DxForeignKeyAction? onUpdate = null
    )
    {
        ColumnName = columnName;
        ProviderDataType = providerDataType;
        Length = length;
        Precision = precision;
        Scale = scale;
        DefaultExpression = defaultExpression;
        IsNullable = isNullable;
        IsPrimaryKey = isPrimaryKey;
        IsAutoIncrement = isAutoIncrement;
        IsUnique = isUnique;
        IsIndexed = isIndexed;
        IsForeignKey = isForeignKey;
        ReferencedTableName = referencedTableName;
        ReferencedColumnName = referencedColumnName;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    public string? ColumnName { get; }
    public string? ProviderDataType { get; }
    public int? Length { get; }
    public int? Precision { get; }
    public int? Scale { get; }
    public string? DefaultExpression { get; }
    public bool IsNullable { get; }
    public bool IsPrimaryKey { get; }
    public bool IsAutoIncrement { get; }
    public bool IsUnique { get; }
    public bool IsIndexed { get; }
    public bool IsForeignKey { get; }
    public string? ReferencedTableName { get; }
    public string? ReferencedColumnName { get; }
    public DxForeignKeyAction? OnDelete { get; }
    public DxForeignKeyAction? OnUpdate { get; }
}
