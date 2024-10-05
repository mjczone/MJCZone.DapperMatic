using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override Task<List<DxIndex>> GetIndexesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return GetIndexesInternalAsync(db, tableName, indexNameFilter, tx, cancellationToken);
    }

    private async Task<List<DxIndex>> GetIndexesInternalAsync(
        IDbConnection db,
        string? tableNameFilter,
        string? indexNameFilter,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var whereTableLike = string.IsNullOrWhiteSpace(tableNameFilter)
            ? ""
            : ToLikeString(tableNameFilter);

        var whereIndexLike = string.IsNullOrWhiteSpace(indexNameFilter)
            ? ""
            : ToLikeString(indexNameFilter);

        var sql =
            @$"
            SELECT 
                TABLE_SCHEMA as schema_name,
                TABLE_NAME as table_name,
                INDEX_NAME as index_name,
                IF(NON_UNIQUE = 1, 0, 1) AS is_unique,
                GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX ASC) AS columns_csv,
                GROUP_CONCAT(CASE
                    WHEN COLLATION = 'A' THEN 'ASC'
                    WHEN COLLATION = 'D' THEN 'DESC'
                    ELSE 'N/A'
                END ORDER BY SEQ_IN_INDEX ASC) AS columns_desc_csv
            FROM 
                INFORMATION_SCHEMA.STATISTICS stats
            WHERE 
                TABLE_SCHEMA = DATABASE()
                and INDEX_NAME != 'PRIMARY'
                and INDEX_NAME NOT IN (select CONSTRAINT_NAME from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                                    where TABLE_SCHEMA = DATABASE() and 
                                            TABLE_NAME = stats.TABLE_NAME AND
                                            CONSTRAINT_TYPE in ('PRIMARY KEY', 'FOREIGN KEY', 'CHECK'))
                {(tableNameFilter != null ? "and TABLE_NAME LIKE @whereTableLike" : "")}
                {(indexNameFilter != null ? "and INDEX_NAME LIKE @whereIndexLike" : "")}
            GROUP BY 
                TABLE_NAME, INDEX_NAME, NON_UNIQUE
            order by schema_name, table_name, index_name
            ";

        var indexResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string index_name,
            bool is_unique,
            string columns_csv,
            string columns_desc_csv
        )>(db, sql, new { whereTableLike, whereIndexLike }, tx)
            .ConfigureAwait(false);

        var indexes = new List<DxIndex>();

        foreach (var indexResult in indexResults)
        {
            var columnNames = indexResult.columns_csv.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            var columnDirections = indexResult.columns_desc_csv.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            var columns = columnNames
                .Select(
                    (c, i) =>
                        new DxOrderedColumn(
                            c,
                            columnDirections[i].Equals("desc", StringComparison.OrdinalIgnoreCase)
                                ? DxColumnOrder.Descending
                                : DxColumnOrder.Ascending
                        )
                )
                .ToArray();

            indexes.Add(
                new DxIndex(
                    DefaultSchema,
                    indexResult.table_name,
                    indexResult.index_name,
                    columns,
                    indexResult.is_unique
                )
            );
        }

        return indexes;
    }
}
