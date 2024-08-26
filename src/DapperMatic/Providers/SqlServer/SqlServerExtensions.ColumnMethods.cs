using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
                    },
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
        var (schemaName, tableName, columnName) = NormalizeNames(schema, table, column);
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaName}.{tableName}
                  ADD {columnName} {sqlType} {(nullable ? "NULL" : "NOT NULL")} {(!string.IsNullOrWhiteSpace(defaultValue) ? $"DEFAULT {defaultValue}" : "")} {(unique ? "UNIQUE" : "")}
                    ",
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
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schemaName AND TABLE_NAME = @tableName",
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

        // get foreign keys for the column
        var foreignKeys = await QueryAsync<string>(
                db,
                $@"
                SELECT
                    fk.name
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc
                    ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c
                    ON fkc.parent_object_id = c.object_id
                    AND fkc.parent_column_id = c.column_id
                WHERE c.object_id = OBJECT_ID('[{schemaName}].[{tableName}]')
                AND c.name = @columnName",
                new { columnName },
                tx
            )
            .ConfigureAwait(false);

        // drop foreign keys
        foreach (var fk in foreignKeys)
        {
            await ExecuteAsync(
                    db,
                    $@"
                    ALTER TABLE [{schemaName}].[{tableName}]
                    DROP CONSTRAINT {fk}",
                    tx
                )
                .ConfigureAwait(false);
        }

        // get indexes for the column (indexes and unique constraints)
        var indexes = await QueryAsync<(string, bool)>(
                db,
                $@"
                SELECT
                    i.name, i.is_unique_constraint
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic
                    ON i.object_id = ic.object_id
                    AND i.index_id = ic.index_id
                INNER JOIN sys.columns c
                    ON ic.object_id = c.object_id
                    AND ic.column_id = c.column_id
                WHERE c.object_id = OBJECT_ID('[{schemaName}].[{tableName}]')
                AND is_primary_key = 0
                AND c.name = @columnName",
                new { columnName },
                tx
            )
            .ConfigureAwait(false);

        // drop indexes
        foreach (var index in indexes)
        {
            if (index.Item2 == true)
            {
                await ExecuteAsync(
                        db,
                        $@"
                        ALTER TABLE [{schemaName}].[{tableName}]
                        DROP CONSTRAINT {index.Item1}",
                        tx
                    )
                    .ConfigureAwait(false);
                continue;
            }
            else
            {
                await ExecuteAsync(
                        db,
                        $@"
                        DROP INDEX [{schemaName}].[{tableName}].[{index}]",
                        tx
                    )
                    .ConfigureAwait(false);
            }
        }

        // get default constraints for the column
        var defaultConstraints = await QueryAsync<string>(
                db,
                $@"
                SELECT
                    dc.name
                FROM sys.default_constraints dc
                INNER JOIN sys.columns c
                    ON dc.parent_object_id = c.object_id
                    AND dc.parent_column_id = c.column_id
                WHERE c.object_id = OBJECT_ID('[{schemaName}].[{tableName}]')
                AND c.name = @columnName",
                new { columnName },
                tx
            )
            .ConfigureAwait(false);

        // drop default constraints
        foreach (var dc in defaultConstraints)
        {
            await ExecuteAsync(
                    db,
                    $@"
                    ALTER TABLE [{schemaName}].[{tableName}]
                    DROP CONSTRAINT {dc}",
                    tx
                )
                .ConfigureAwait(false);
        }

        // drop column
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaName}.{tableName} DROP COLUMN {columnName}",
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
