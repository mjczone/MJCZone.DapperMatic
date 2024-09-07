using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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

        // does indexName exist in MySql tableName
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                        FROM information_schema.STATISTICS 
                        WHERE TABLE_SCHEMA = DATABASE() AND 
                              TABLE_NAME = @tableName AND 
                              INDEX_NAME = @indexName",
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
                $@"ALTER TABLE `{tableName}`
                    ADD {uniqueString} INDEX `{indexName}` ({columnList})",
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

        var sql1 =
            $@"SELECT * FROM information_schema.STATISTICS WHERE TABLE_SCHEMA = DATABASE()"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND TABLE_NAME = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND INDEX_NAME LIKE @where")
            + " ORDER BY TABLE_NAME, INDEX_NAME";
        var results1 = await QueryAsync<object>(db, sql1, new { tableName, where }, tx)
            .ConfigureAwait(false);

        var sql =
            $@"SELECT
                        TABLE_NAME AS table_name,
                        INDEX_NAME AS index_name,
                        COLUMN_NAME AS column_name,
                        NON_UNIQUE AS non_unique,
                        INDEX_TYPE AS index_type,
                        SEQ_IN_INDEX AS seq_in_index,
                        COLLATION AS collation
                    FROM information_schema.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND TABLE_NAME = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND INDEX_NAME LIKE @where")
            + @" ORDER BY TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX";

        var results =
            await QueryAsync<(string table_name, string index_name, string column_name, int non_unique, string index_type, int seq_in_index, string collation)>(
                    db,
                    sql,
                    new { tableName, where },
                    tx
                )
                .ConfigureAwait(false);

        var grouped = results.GroupBy(
            r => (r.table_name, r.index_name),
            r => (r.non_unique, r.column_name, r.index_type, r.seq_in_index, r.collation)
        );

        var indexes = new List<TableIndex>();
        foreach (var group in grouped)
        {
            var (table_name, index_name) = group.Key;
            var (non_unique, column_name, index_type, seq_in_index, collation) = group.First();
            var columnNames = group
                    .Select(
                        g =>
                        {
                            var col = g.column_name;
                            var direction = g.collation.Equals(
                                "D",
                                StringComparison.OrdinalIgnoreCase
                            )
                              ? "DESC"
                              : "ASC";
                            return $"{col} {direction}";
                        }
                    )
                    .ToArray();
            var index = new TableIndex(
                null,
                table_name,
                index_name,
                columnNames,
                non_unique != 1
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
            @$"SELECT INDEX_NAME FROM information_schema.STATISTICS 
                    WHERE TABLE_SCHEMA = DATABASE()"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND TABLE_NAME = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND INDEX_NAME LIKE @where")
            + @" ORDER BY INDEX_NAME";

        return await QueryAsync<string>(db, sql, new { tableName, where }, tx)
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
        (_, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (
            !await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        await ExecuteAsync(
            db,
            $@"ALTER TABLE `{tableName}`
                    DROP INDEX `{indexName}`",
            transaction: tx
        );

        return true;
    }
}
