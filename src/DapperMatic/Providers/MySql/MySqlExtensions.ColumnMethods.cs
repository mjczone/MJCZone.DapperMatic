using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
                    $@"SELECT COUNT(*) FROM information_schema.COLUMNS 
                        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName",
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

        // create MySql column
        await ExecuteAsync(
                db,
                $@"ALTER TABLE `{tableName}`
                    ADD COLUMN `{columnName}` {sqlType} {(nullable ? "NULL" : "NOT NULL")} {(!string.IsNullOrWhiteSpace(defaultValue) ? $"DEFAULT {defaultValue}" : "")} {(unique ? "UNIQUE" : "")}",
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
            return await QueryAsync<string>(
                    db,
                    $@"SELECT COLUMN_NAME FROM information_schema.COLUMNS 
                        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName
                        ORDER BY ORDINAL_POSITION",
                    new { tableName },
                    tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    $@"SELECT COLUMN_NAME FROM information_schema.COLUMNS 
                        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName
                        AND COLUMN_NAME LIKE @where
                        ORDER BY ORDINAL_POSITION",
                    new { tableName, where },
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

        await ExecuteAsync(
                db,
                $@"ALTER TABLE `{tableName}` DROP COLUMN `{columnName}`",
                new { tableName, columnName },
                tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
