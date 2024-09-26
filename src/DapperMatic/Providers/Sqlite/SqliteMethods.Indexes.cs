using System.Data;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    public override async Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException("Index name is required.", nameof(indexName));
        }

        if (
            await DoesIndexExistAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            return false;
        }

        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        var createIndexSql =
            $"CREATE {(isUnique ? "UNIQUE INDEX" : "INDEX")} {indexName} ON {tableName} ({string.Join(", ", columns.Select(c => c.ToString()))})";

        Logger.LogDebug(
            "Generated index definition SQL: {sql} for index '{indexName}' ON {tableName}",
            createIndexSql,
            indexName,
            tableName
        );

        await ExecuteAsync(db, createIndexSql, transaction: tx).ConfigureAwait(false);

        return true;
    }

    public override async Task<List<DxIndex>> GetIndexesAsync(
        IDbConnection db,
        string? schemaName,
        // allow this to be empty to query all indexes
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(indexNameFilter)
            ? null
            : ToAlphaNumericString(indexNameFilter).Replace('*', '%');

        var whereStatement =
            (string.IsNullOrWhiteSpace(tableName) ? "" : " AND m.name = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? null : " AND il.name LIKE @where");
        var whereParams = new { tableName, where };

        return await GetIndexesInternalAsync(
                db,
                schemaName,
                whereStatement,
                whereParams,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    private async Task<List<DxIndex>> GetIndexesInternalAsync(
        IDbConnection db,
        string? schemaName,
        string? whereStatement,
        object? whereParams,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var sql =
            $@"
                SELECT DISTINCT 
                    m.name AS table_name, 
                    il.name AS index_name,
                    il.""unique"" AS is_unique,	
                    ii.name AS column_name,
                    ii.DESC AS is_descending
                FROM sqlite_schema AS m,
                    pragma_index_list(m.name) AS il,
                    pragma_index_xinfo(il.name) AS ii
                WHERE m.type='table' 
                    and ii.name IS NOT NULL 
                    AND il.origin = 'c'                    
                    "
            + (whereStatement ?? "")
            + $@" ORDER BY m.name, il.name, ii.seqno";
        var results = await QueryAsync<(
            string table_name,
            string index_name,
            bool is_unique,
            string column_name,
            bool is_descending
        )>(db, sql, whereParams, transaction: tx)
            .ConfigureAwait(false);

        var indexes = new List<DxIndex>();

        foreach (
            var group in results.GroupBy(r => new
            {
                r.table_name,
                r.index_name,
                r.is_unique
            })
        )
        {
            var index = new DxIndex
            {
                SchemaName = null,
                TableName = group.Key.table_name,
                IndexName = group.Key.index_name,
                IsUnique = group.Key.is_unique,
                Columns = group
                    .Select(r => new DxOrderedColumn(
                        r.column_name,
                        r.is_descending ? DxColumnOrder.Descending : DxColumnOrder.Ascending
                    ))
                    .ToArray()
            };
            indexes.Add(index);
        }

        return indexes;
    }

    private async Task<List<string>> GetCreateIndexSqlStatementsForTable(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var getSqlCreateIndexStatements =
            @"        
                SELECT DISTINCT
                    m.sql
                FROM sqlite_schema AS m,
                    pragma_index_list(m.name) AS il,
                    pragma_index_xinfo(il.name) AS ii
                WHERE m.type='table' 
                    AND ii.name IS NOT NULL 
                    AND il.origin = 'c'
                    AND m.name = @tableName
                    AND m.sql IS NOT NULL
                 ORDER BY m.name, il.name, ii.seqno
        ";
        return (
            await QueryAsync<string>(
                    db,
                    getSqlCreateIndexStatements,
                    new { tableName },
                    transaction: tx
                )
                .ConfigureAwait(false)
        )
            .Select(sql =>
            {
                return sql.Contains("IF NOT EXISTS", StringComparison.OrdinalIgnoreCase)
                    ? sql
                    : sql.Replace(
                            "CREATE INDEX",
                            "CREATE INDEX IF NOT EXISTS",
                            StringComparison.OrdinalIgnoreCase
                        )
                        .Replace(
                            "CREATE UNIQUE INDEX",
                            "CREATE UNIQUE INDEX IF NOT EXISTS",
                            StringComparison.OrdinalIgnoreCase
                        )
                        .Trim();
            })
            .ToList();
    }

    public override async Task<bool> DropIndexIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await DoesIndexExistAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        indexName = NormalizeName(indexName);

        // drop index
        await ExecuteAsync(db, $@"DROP INDEX {indexName}", transaction: tx).ConfigureAwait(false);

        return true;
    }
}
