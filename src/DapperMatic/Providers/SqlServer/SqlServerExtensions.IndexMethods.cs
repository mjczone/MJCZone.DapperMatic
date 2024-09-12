using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                        AND name = @indexName and is_primary_key = 0 and is_unique_constraint = 0",
                    new { schemaAndTableName, indexName },
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

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columnNames);
        await ExecuteAsync(
                db,
                $@"
                CREATE {uniqueString} INDEX {indexName} ON {schemaAndTableName} ({columnList})
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
               WHERE ind.is_primary_key = 0 AND ind.is_unique_constraint = 0 AND t.is_ms_shipped = 0"
            + (
                string.IsNullOrWhiteSpace(schemaName)
                    ? ""
                    : " AND SCHEMA_NAME(t.schema_id) = @schemaName"
            )
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND t.name = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND ind.name LIKE @where")
            + @" ORDER BY schema_name, table_name, index_name, key_ordinal";

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

        var indexes = new List<TableIndex>();
        foreach (var group in grouped)
        {
            var (schema_name, table_name, index_name) = group.Key;
            var (is_unique, column_name, key_ordinal, is_descending_key) = group.First();
            var index = new TableIndex(
                schema_name,
                table_name,
                index_name,
                group
                    .Select(g =>
                    {
                        var col = g.column_name;
                        var direction = g.is_descending_key == 1 ? "DESC" : "ASC";
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
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            @$"SELECT ind.name 
                FROM sys.indexes ind
                INNER JOIN sys.tables t ON ind.object_id = t.object_id 
               WHERE ind.is_primary_key = 0 and ind.is_unique_constraint = 0 AND t.is_ms_shipped = 0"
            + (
                string.IsNullOrWhiteSpace(schemaName)
                    ? ""
                    : " AND SCHEMA_NAME(t.schema_id) = @schemaName"
            )
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND t.name = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND ind.name LIKE @where")
            + @" ORDER BY ind.name";

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
                DROP INDEX [{schemaName}].[{tableName}].[{indexName}]
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
