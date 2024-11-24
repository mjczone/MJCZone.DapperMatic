using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DapperMatic.Models;

[Serializable]
public class DxColumn
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxColumn() { }

    [SetsRequiredMembers]
    public DxColumn(
        string? schemaName,
        string tableName,
        string columnName,
        Type dotnetType,
        Dictionary<DbProviderType, string>? providerDataTypes = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? checkExpression = null,
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
        SchemaName = schemaName;
        TableName = tableName;
        ColumnName = columnName;
        DotnetType = dotnetType;
        ProviderDataTypes = providerDataTypes ?? [];
        Length = length;
        Precision = precision;
        Scale = scale;
        CheckExpression = checkExpression;
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

    public string? SchemaName { get; set; }
    public required string TableName { get; init; }
    public required string ColumnName { get; init; }
    public required Type DotnetType { get; init; }

    /// <summary>
    /// The FULL native provider data type. This is the data type that the provider uses to
    /// store the data (e.g. "INTEGER", "DECIMAL(14,3)", "VARCHAR(255)", "TEXT", "BLOB", etc.)
    /// </summary>
    /// <remarks>
    /// The provider data type should include the length, precision, and scale if applicable.
    /// </remarks>
    public Dictionary<DbProviderType, string> ProviderDataTypes { get; } = new();
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public string? CheckExpression { get; set; }
    public string? DefaultExpression { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public bool IsUnicode { get; set; }
    public bool IsFixedLength { get; set; }

    /// <summary>
    /// Is either part of a single column unique constraint or a single column unique index.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Is part of an index
    /// </summary>
    public bool IsIndexed { get; set; }

    /// <summary>
    /// Is a foreign key to a another referenced table. This is the MANY side of a ONE-TO-MANY relationship.
    /// </summary>
    public bool IsForeignKey { get; set; }
    public string? ReferencedTableName { get; set; }
    public string? ReferencedColumnName { get; set; }
    public DxForeignKeyAction? OnDelete { get; set; }
    public DxForeignKeyAction? OnUpdate { get; set; }

    public bool IsNumeric()
    {
        return DotnetType == typeof(byte)
            || DotnetType == typeof(sbyte)
            || DotnetType == typeof(short)
            || DotnetType == typeof(ushort)
            || DotnetType == typeof(int)
            || DotnetType == typeof(uint)
            || DotnetType == typeof(long)
            || DotnetType == typeof(ulong)
            || DotnetType == typeof(float)
            || DotnetType == typeof(double)
            || DotnetType == typeof(decimal);
    }

    public bool IsText()
    {
        return DotnetType == typeof(string)
            || DotnetType == typeof(char)
            || DotnetType == typeof(char[]);
    }

    public bool IsDateTime()
    {
        return DotnetType == typeof(DateTime) || DotnetType == typeof(DateTimeOffset);
    }

    public bool IsBoolean()
    {
        return DotnetType == typeof(bool);
    }

    public bool IsBinary()
    {
        return DotnetType == typeof(byte[]);
    }

    public bool IsGuid()
    {
        return DotnetType == typeof(Guid);
    }

    public bool IsEnum()
    {
        return DotnetType.IsEnum;
    }

    public bool IsArray()
    {
        return DotnetType.IsArray;
    }

    public bool IsDictionary()
    {
        return DotnetType.IsGenericType
            && DotnetType.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    public bool IsEnumerable()
    {
        return typeof(IEnumerable<>).IsAssignableFrom(DotnetType);
    }

    public string GetTypeCategory()
    {
        if (IsNumeric())
            return "Numeric";
        if (IsText())
            return "Text";
        if (IsDateTime())
            return "DateTime";
        if (IsBoolean())
            return "Boolean";
        if (IsBinary())
            return "Binary";
        if (IsGuid())
            return "Guid";
        if (IsEnum())
            return "Enum";
        if (IsArray())
            return "Array";
        if (IsDictionary())
            return "Dictionary";
        if (IsEnumerable())
            return "Enumerable";
        return "Unknown";
    }

    // ToString override to display column definition
    public override string ToString()
    {
        return $"{ColumnName} ({JsonSerializer.Serialize(ProviderDataTypes)}) {(IsNullable ? "NULL" : "NOT NULL")}"
            + $"{(IsPrimaryKey ? " PRIMARY KEY" : "")}"
            + $"{(IsUnique ? " UNIQUE" : "")}"
            + $"{(IsIndexed ? " INDEXED" : "")}"
            + $"{(IsForeignKey ? $" FOREIGN KEY({ReferencedTableName ?? ""}) REFERENCES({ReferencedColumnName ?? ""})" : "")}"
            + $"{(IsAutoIncrement ? " AUTOINCREMENT" : "")}"
            + $"{(!string.IsNullOrWhiteSpace(CheckExpression) ? $" CHECK ({CheckExpression})" : "")}"
            + $"{(!string.IsNullOrWhiteSpace(DefaultExpression) ? $" DEFAULT {(DefaultExpression.Contains(' ') ? $"({DefaultExpression})" : DefaultExpression)}" : "")}";
    }

    public string? GetProviderDataType(DbProviderType providerType)
    {
        return ProviderDataTypes.TryGetValue(providerType, out var providerDataType)
            ? providerDataType
            : null;
    }

    public DxColumn SetProviderDataType(DbProviderType providerType, string providerDataType)
    {
        ProviderDataTypes[providerType] = providerDataType;
        return this;
    }
}
