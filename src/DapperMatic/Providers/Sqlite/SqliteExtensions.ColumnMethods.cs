using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> ColumnExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (_, tableName, columnName) = NormalizeNames(schema, table, column);
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    @$"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name = @columnName",
                    new { tableName, columnName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateColumnIfNotExistsAsync(
        IDbConnection db,
        string table,
        string column,
        Type dotnetType,
        string? type = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? schema = null,
        string? defaultValue = null,
        bool nullable = true,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await ColumnExistsAsync(db, table, column, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var sqlType = type ?? GetSqlTypeString(dotnetType, length, precision, scale);
        var (_, tableName, columnName) = NormalizeNames(schema, table, column);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {tableName} 
                    ADD COLUMN {columnName} {sqlType} {(nullable ? "NULL" : "NOT NULL")} {(!string.IsNullOrWhiteSpace(defaultValue) ? $"DEFAULT {defaultValue}" : "")} {(unique ? "UNIQUE" : "")}",
                new { tableName, columnName },
                tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public async Task<IEnumerable<string>> GetColumnsAsync(
        IDbConnection db,
        string table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (_, tableName, _) = NormalizeNames(schema, table, null);

        if (string.IsNullOrWhiteSpace(filter))
        {
            // return await QueryAsync<string>(db, $@"PRAGMA table_info({tableName})", tx)
            //     .ConfigureAwait(false);
            return await QueryAsync<string>(
                    db,
                    $@"select name from pragma_table_info('{tableName}')",
                    tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    $@"select name from pragma_table_info('{tableName}') where name like @where",
                    new { where },
                    tx
                )
                .ConfigureAwait(false);
        }
    }

    public async Task<bool> DropColumnIfExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await ColumnExistsAsync(db, table, column, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var (_, tableName, columnName) = NormalizeNames(schema, table, column);

        // drop column
        await ExecuteAsync(db, $@"ALTER TABLE {tableName} DROP COLUMN {columnName}", tx)
            .ConfigureAwait(false);

        return true;
    }
}
