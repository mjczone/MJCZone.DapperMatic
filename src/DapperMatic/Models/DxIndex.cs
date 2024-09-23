using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

public class DxIndex
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxIndex() { }

    [SetsRequiredMembers]
    public DxIndex(
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false
    )
    {
        SchemaName = schemaName;
        TableName = tableName;
        IndexName = indexName;
        Columns = columns;
        IsUnique = isUnique;
    }

    public string? SchemaName { get; set; }

    public required string TableName { get; set; }

    public required string IndexName { get; set; }

    public required DxOrderedColumn[] Columns { get; set; }
    public bool IsUnique { get; set; }
}
