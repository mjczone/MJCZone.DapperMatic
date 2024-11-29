using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

/// <summary>
/// Represents a table in a database.
/// </summary>
[Serializable]
public class DxTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxTable"/> class.
    /// Used for deserialization.
    /// </summary>
    public DxTable() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxTable"/> class.
    /// </summary>
    /// <param name="schemaName">The schema name of the table.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columns">The columns of the table.</param>
    /// <param name="primaryKey">The primary key constraint of the table.</param>
    /// <param name="checkConstraints">The check constraints of the table.</param>
    /// <param name="defaultConstraints">The default constraints of the table.</param>
    /// <param name="uniqueConstraints">The unique constraints of the table.</param>
    /// <param name="foreignKeyConstraints">The foreign key constraints of the table.</param>
    /// <param name="indexes">The indexes of the table.</param>
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

    /// <summary>
    /// Gets or sets the schema name of the table.
    /// </summary>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public required string TableName { get; set; }

    /// <summary>
    /// Gets or sets the columns of the table.
    /// </summary>
    public List<DxColumn> Columns { get; set; } = [];

    /// <summary>
    /// Gets or sets the primary key constraint of the table.
    /// </summary>
    public DxPrimaryKeyConstraint? PrimaryKeyConstraint { get; set; }

    /// <summary>
    /// Gets or sets the check constraints of the table.
    /// </summary>
    public List<DxCheckConstraint> CheckConstraints { get; set; } = [];

    /// <summary>
    /// Gets or sets the default constraints of the table.
    /// </summary>
    public List<DxDefaultConstraint> DefaultConstraints { get; set; } = [];

    /// <summary>
    /// Gets or sets the unique constraints of the table.
    /// </summary>
    public List<DxUniqueConstraint> UniqueConstraints { get; set; } = [];

    /// <summary>
    /// Gets or sets the foreign key constraints of the table.
    /// </summary>
    public List<DxForeignKeyConstraint> ForeignKeyConstraints { get; set; } = [];

    /// <summary>
    /// Gets or sets the indexes of the table.
    /// </summary>
    public List<DxIndex> Indexes { get; set; } = [];
}
