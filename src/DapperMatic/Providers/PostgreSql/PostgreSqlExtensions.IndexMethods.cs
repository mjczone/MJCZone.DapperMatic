using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"
                    SELECT COUNT(*)
                    FROM pg_indexes
                    WHERE schemaname = @schemaName AND
                          tablename = @tableName AND
                          indexname = @indexName
                    ",
                    new
                    {
                        schemaName,
                        tableName,
                        indexName
                    },
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
        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

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
                CREATE {uniqueString} INDEX {indexName} ON {schemaName}.{tableName} ({columnList})
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
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            @$"select
                s.nspname as schema_name,
                t.relname as table_name,
                i.relname as index_name,
                a.attname as column_name,
                ix.indisunique as is_unique,
                idx.indexdef as index_sql
            from pg_class t
            join pg_index ix on t.oid = ix.indrelid 
            join pg_class i on i.oid = ix.indexrelid
            join pg_attribute a on a.attrelid = t.oid and a.attnum = ANY(ix.indkey)
            join pg_namespace s on s.oid = t.relnamespace
            join pg_indexes idx on idx.schemaname = s.nspname and idx.tablename = t.relname and idx.indexname = i.relname
            where
                t.relkind = 'r'"
            + (string.IsNullOrWhiteSpace(schemaName) ? "" : " AND s.nspname = @schemaName")
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND t.relname = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND i.relname LIKE @where")
            + @" ORDER BY s.nspname, t.relname, i.relname, array_position(ix.indkey, a.attnum)";

        var results = await QueryAsync<(
            string schema_name,
            string table_name,
            string index_name,
            string column_name,
            bool is_unique,
            string index_sql
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
            r => (r.is_unique, r.column_name, r.index_sql)
        );

        var indexes = new List<TableIndex>();
        foreach (var group in grouped)
        {
            var (schema_name, table_name, index_name) = group.Key;
            var (is_unique, column_name, index_sql) = group.First();
            var index = new TableIndex(
                schema_name,
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
                is_unique
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
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            @$"SELECT indexname 
               FROM pg_indexes
               WHERE 
                schemaname NOT IN ('pg_catalog', 'information_schema')"
            + (string.IsNullOrWhiteSpace(schemaName) ? "" : " AND schemaname = @schemaName")
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND tablename = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND indexname LIKE @where")
            + @" ORDER BY indexname";

        return await QueryAsync<string>(
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
        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (
            !await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        await ExecuteAsync(
            db,
            $@"
                DROP INDEX {schemaName}.{indexName}
                ",
            transaction: tx
        );

        return true;
    }
}
