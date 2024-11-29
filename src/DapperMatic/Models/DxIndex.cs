using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

/// <summary>
/// Represents an index on a table.
/// </summary>
[Serializable]
public class DxIndex
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxIndex"/> class.
    /// Used for deserialization.
    /// </summary>
    public DxIndex() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxIndex"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <param name="columns">The columns in the index.</param>
    /// <param name="isUnique">Indicates whether the index is unique.</param>
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
        Columns = [.. columns];
        IsUnique = isUnique;
    }

    /// <summary>
    /// Gets or sets the schema name.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the table name.
    /// </summary>
    public required string TableName { get; set; }

    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public required string IndexName { get; set; }

    /// <summary>
    /// Gets or sets the columns.
    /// </summary>
    public required List<DxOrderedColumn> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique { get; set; }
}
