using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> IndexExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (string.IsNullOrWhiteSpace(indexName))
            throw new ArgumentException(
                "Index name must be specified in SQLite.",
                nameof(indexName)
            );

        // this is the query to get all indexes for a tableName in SQLite
        // for DEBUGGING purposes
        // var fks = (
        //     await db.QueryAsync($@"select * from pragma_index_list('{tableName}')", tx)
        //         .ConfigureAwait(false)
        // )
        //     .Cast<IDictionary<string, object?>>()
        //     .ToArray();
        // var fksJson = JsonConvert.SerializeObject(fks);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                            FROM pragma_index_list('{tableName}')
                            WHERE ""origin"" = 'c' and ""name"" = @indexName",
                    new { tableName, indexName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string[] columnNames,
        string? schemaName = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (columnNames == null || columnNames.Length == 0)
            throw new ArgumentException(
                "At least one columnName must be specified.",
                nameof(columnNames)
            );

        if (
            await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columnNames);
        await ExecuteAsync(
                db,
                $@"
                CREATE {uniqueString} INDEX {indexName} ON {tableName} ({columnList})
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public async Task<IEnumerable<TableIndex>> GetIndexesAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            @$"SELECT 
                    tbl.name AS table_name,
                    idx.name AS index_name,
                    idc.name AS column_name,
                    idx.[unique] AS is_unique,
                    idx_master.sql AS index_sql
                FROM sqlite_master AS tbl
                LEFT JOIN pragma_index_list(tbl.name) AS idx
                LEFT JOIN pragma_index_info(idx.name) AS idc
                JOIN sqlite_master AS idx_master ON tbl.name = idx_master.tbl_name AND idx.name = idx_master.name
                WHERE 
                    tbl.type = 'table' 
                    AND idx.origin = 'c'
                    AND idx_master.type = 'index' 
                    AND idx_master.sql IS NOT NULL"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND tbl.name = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND idx.name LIKE @where")
            + @" ORDER BY tbl.name, idx.name";

        var results = await QueryAsync<(
            string table_name,
            string index_name,
            string column_name,
            int is_unique,
            string index_sql
        )>(db, sql, new { tableName, where }, tx)
            .ConfigureAwait(false);

        var grouped = results.GroupBy(
            r => (r.table_name, r.index_name),
            r => (r.is_unique, r.column_name, r.index_sql)
        );
        
        var indexes = new List<TableIndex>();
        foreach (var group in grouped)
        {
            var (table_name, index_name) = group.Key;
            var (is_unique, column_name, index_sql) = group.First();
            var index = new TableIndex(
                null,
                table_name,
                index_name,
                group
                    .Select(g =>
                    {
                        var col = g.column_name;
                        var sql = g.index_sql.ToLowerInvariant().Replace("[", "").Replace("]", "");
                        var direction = sql.ToLowerInvariant()
                            .Contains($"{col} desc", StringComparison.OrdinalIgnoreCase)
                            ? "DESC"
                            : "ASC";
                        return $"{col} {direction}";
                    })
                    .ToArray(),
                is_unique == 1
            );
            indexes.Add(index);
        }

        return indexes;
    }

    public async Task<IEnumerable<string>> GetIndexNamesAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            @$"SELECT name FROM sqlite_master WHERE type = 'index'"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND tbl_name = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND name LIKE @where")
            + @" ORDER BY name";

        return await QueryAsync<string>(db, sql, new { tableName, where }, tx).ConfigureAwait(false);
    }

    public async Task<bool> DropIndexIfExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        indexName = NormalizeName(indexName);

        await ExecuteAsync(
                db,
                $@"
                DROP INDEX {indexName}
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
