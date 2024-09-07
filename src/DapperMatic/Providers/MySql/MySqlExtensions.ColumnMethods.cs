using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> ColumnExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

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
        string tableName,
        string columnName,
        Type dotnetType,
        string? type = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? schemaName = null,
        string? defaultValue = null,
        bool nullable = true,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await ColumnExistsAsync(db, tableName, columnName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var sqlType = type ?? GetSqlTypeString(dotnetType, length, precision, scale);
        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        // create MySql columnName
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
        string tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        if (string.IsNullOrWhiteSpace(nameFilter))
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
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
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
        string tableName,
        string columnName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await ColumnExistsAsync(db, tableName, columnName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

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
