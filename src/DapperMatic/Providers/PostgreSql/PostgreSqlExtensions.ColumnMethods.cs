using System.Data;
using Dapper;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schemaName AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName",
                    new { schemaName, tableName, columnName }
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
        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var sql =
            $@"ALTER TABLE {schemaName}.{tableName}
                  ADD {columnName} {sqlType} {(nullable ? "NULL" : "NOT NULL")} {(!string.IsNullOrWhiteSpace(defaultValue) ? $"DEFAULT {defaultValue}" : "")} {(unique ? "UNIQUE" : "")}
                    ";
        await ExecuteAsync(db, sql, new { schemaName, tableName, columnName }, tx)
            .ConfigureAwait(false);

        return true;
    }

    public async Task<IEnumerable<string>> GetColumnNamesAsync(
        IDbConnection db,
        string tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName, null);

        return await QueryAsync<string>(
                db,
                $@"SELECT column_name FROM information_schema.columns WHERE table_schema = @schemaName AND table_name = @tableName",
                new { schemaName, tableName },
                tx
            )
            .ConfigureAwait(false);
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

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaName}.{tableName} DROP COLUMN {columnName} CASCADE",
                new { schemaName, tableName, columnName },
                tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
