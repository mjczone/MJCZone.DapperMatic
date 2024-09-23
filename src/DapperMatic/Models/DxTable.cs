using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

public class DxTable
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxTable() { }

    [SetsRequiredMembers]
    public DxTable(
        string? schemaName,
        string tableName,
        DxColumn[]? columns = null,
        DxPrimaryKeyConstraint? primaryKey = null,
        DxCheckConstraint[]? checkConstraints = null,
        DxDefaultConstraint[]? defaultConstraints = null,
        DxUniqueConstraint[]? uniqueConstraints = null,
        DxForeignKeyConstraint[]? foreignKeyConstraints = null,
        DxIndex[]? indexes = null
    )
    {
        SchemaName = schemaName;
        TableName = tableName;
        Columns = columns == null ? [] : [.. columns];
        PrimaryKeyConstraint = primaryKey;
        CheckConstraints = checkConstraints == null ? [] : [.. checkConstraints];
        DefaultConstraints = defaultConstraints == null ? [] : [.. defaultConstraints];
        UniqueConstraints = uniqueConstraints == null ? [] : [.. uniqueConstraints];
        ForeignKeyConstraints = foreignKeyConstraints == null ? [] : [.. foreignKeyConstraints];
        Indexes = indexes == null ? [] : [.. indexes];
    }

    public string? SchemaName { get; set; }
    public required string TableName { get; set; }
    public List<DxColumn> Columns { get; set; } = [];
    public DxPrimaryKeyConstraint? PrimaryKeyConstraint { get; set; }
    public List<DxCheckConstraint> CheckConstraints { get; set; } = [];
    public List<DxDefaultConstraint> DefaultConstraints { get; set; } = [];
    public List<DxUniqueConstraint> UniqueConstraints { get; set; } = [];
    public List<DxForeignKeyConstraint> ForeignKeyConstraints { get; set; } = [];
    public List<DxIndex> Indexes { get; set; } = [];
}
