using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods
{
    public override async Task<List<DxIndex>> GetIndexesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(indexNameFilter)
            ? null
            : ToLikeString(indexNameFilter);

        var sql =
            @$"SELECT 
                    SCHEMA_NAME(t.schema_id) as schema_name,
                    t.name as table_name,
                    ind.name as index_name,
                    col.name as column_name,
                    ind.is_unique as is_unique,
                    ic.key_ordinal as key_ordinal,
                    ic.is_descending_key as is_descending_key
                FROM sys.indexes ind
                    INNER JOIN sys.tables t ON ind.object_id = t.object_id 
                    INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id
                    INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
                WHERE 
                    ind.is_primary_key = 0 AND ind.is_unique_constraint = 0 AND t.is_ms_shipped = 0
                    {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND SCHEMA_NAME(t.schema_id) = @schemaName")}
                    {(string.IsNullOrWhiteSpace(tableName) ? "" : " AND t.name = @tableName")}
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND ind.name LIKE @where")}
                ORDER BY schema_name, table_name, index_name, key_ordinal";

        var results = await QueryAsync<(
            string schema_name,
            string table_name,
            string index_name,
            string column_name,
            int is_unique,
            string key_ordinal,
            int is_descending_key
        )>(
                db,
                sql,
                new
                {
                    schemaName,
                    tableName,
                    where
                },
                tx
            )
            .ConfigureAwait(false);

        var grouped = results.GroupBy(
            r => (r.schema_name, r.table_name, r.index_name),
            r => (r.is_unique, r.column_name, r.key_ordinal, r.is_descending_key)
        );

        var indexes = new List<DxIndex>();
        foreach (var group in grouped)
        {
            var (schema_name, table_name, index_name) = group.Key;
            var (is_unique, column_name, key_ordinal, is_descending_key) = group.First();
            var index = new DxIndex(
                schema_name,
                table_name,
                index_name,
                group
                    .Select(g =>
                    {
                        return new DxOrderedColumn(
                            g.column_name,
                            g.is_descending_key == 1
                                ? DxColumnOrder.Descending
                                : DxColumnOrder.Ascending
                        );
                    })
                    .ToArray(),
                is_unique == 1
            );
            indexes.Add(index);
        }

        return indexes;
    }
}
