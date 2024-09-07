namespace DapperMatic.Models;

public class ForeignKey
{
    public ForeignKey(
        string? schemaName,
        string foreignKeyName,
        string tableName,
        string columnName,
        string referenceTable,
        string referenceColumn,
        ReferentialAction onDelete,
        ReferentialAction onUpdate
    )
    {
        SchemaName = schemaName;
        ForeignKeyName = foreignKeyName;
        TableName = tableName;
        ColumnName = columnName;
        ReferenceTableName = referenceTable;
        ReferenceColumnName = referenceColumn;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    public string? SchemaName { get; set; }
    public string ForeignKeyName { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string ReferenceTableName { get; set; }
    public string ReferenceColumnName { get; set; }
    public ReferentialAction OnDelete { get; set; } = ReferentialAction.NoAction;
    public ReferentialAction OnUpdate { get; set; } = ReferentialAction.NoAction;
}
