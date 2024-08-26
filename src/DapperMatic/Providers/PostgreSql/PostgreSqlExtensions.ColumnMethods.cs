using System.Data;
using Dapper;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        var (schemaName, tableName, columnName) = NormalizeNames(schema, table, column);
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schemaName AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName",
                    new
                    {
                        schemaName,
                        tableName,
                        columnName
                    }
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
        var (schemaName, tableName, columnName) = NormalizeNames(schema, table, column);

        var sql =
            $@"ALTER TABLE {schemaName}.{tableName}
                  ADD {columnName} {sqlType} {(nullable ? "NULL" : "NOT NULL")} {(!string.IsNullOrWhiteSpace(defaultValue) ? $"DEFAULT {defaultValue}" : "")} {(unique ? "UNIQUE" : "")}
                    ";
        await ExecuteAsync(
                db,
                sql,
                new
                {
                    schemaName,
                    tableName,
                    columnName
                },
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
        var (schemaName, tableName, _) = NormalizeNames(schema, table, null);

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

        var (schemaName, tableName, columnName) = NormalizeNames(schema, table, column);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaName}.{tableName} DROP COLUMN {columnName} CASCADE",
                new
                {
                    schemaName,
                    tableName,
                    columnName
                },
                tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
