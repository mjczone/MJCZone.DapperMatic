namespace DapperMatic.Models;

public class TableIndex
{
    public TableIndex(string? schemaName, string tableName, string indexName, string[] columnNames, bool unique = false)
    {
        SchemaName = schemaName;
        TableName = tableName;
        IndexName = indexName;
        ColumnNames = columnNames;
        Unique = unique;
    }

    public string? SchemaName { get; set; }

    public string TableName { get; set; }

    public string IndexName { get; set; }

    /// <summary>
    /// Column names (optionally, appended with ` ASC` or ` DESC`)
    /// </summary>
    public string[] ColumnNames { get; set; }
    public bool Unique { get; set; }
}
