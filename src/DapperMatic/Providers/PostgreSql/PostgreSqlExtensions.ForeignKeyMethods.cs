using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> ForeignKeyExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);
        if (!string.IsNullOrWhiteSpace(foreignKey))
        {
            var foreignKeyName = NormalizeName(foreignKey);

            return 0
                < await ExecuteScalarAsync<int>(
                        db,
                        $@"SELECT COUNT(*) 
                        FROM information_schema.table_constraints 
                        WHERE table_schema = @schemaName AND 
                              table_name = @tableName AND 
                              constraint_name = @foreignKeyName AND
                              constraint_type = 'FOREIGN KEY'",
                        new
                        {
                            schemaName,
                            tableName,
                            foreignKeyName
                        },
                        tx
                    )
                    .ConfigureAwait(false);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be specified.", nameof(columnName));

            return 0
                < await ExecuteScalarAsync<int>(
                        db,
                        $@"SELECT COUNT(*) 
                            FROM information_schema.table_constraints 
                            WHERE table_schema = @schemaName AND 
                                  table_name = @tableName AND 
                                  constraint_type = 'FOREIGN KEY' AND 
                                  constraint_name IN (
                                      SELECT constraint_name 
                                      FROM information_schema.key_column_usage 
                                      WHERE table_schema = @schemaName AND 
                                            table_name = @tableName AND 
                                            column_name = @columnName
                                  )",
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
    }

    public async Task<bool> CreateForeignKeyIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schemaName = null,
        string onDelete = "NO ACTION",
        string onUpdate = "NO ACTION",
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(foreignKey))
            throw new ArgumentException("Foreign key name must be specified.", nameof(foreignKey));
        if (string.IsNullOrWhiteSpace(referenceTable))
            throw new ArgumentException(
                "Reference tableName name must be specified.",
                nameof(referenceTable)
            );
        if (string.IsNullOrWhiteSpace(referenceColumn))
            throw new ArgumentException(
                "Reference columnName name must be specified.",
                nameof(referenceColumn)
            );
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name must be specified.", nameof(columnName));
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name must be specified.", nameof(tableName));

        if (
            await ForeignKeyExistsAsync(
                    db,
                    tableName,
                    columnName,
                    foreignKey,
                    schemaName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);
        var (referenceSchemaName, referenceTableName, referenceColumnName) = NormalizeNames(
            schemaName,
            referenceTable,
            referenceColumn
        );

        var foreignKeyName = NormalizeName(foreignKey);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaName}.{tableName} 
                    ADD CONSTRAINT {foreignKeyName} 
                    FOREIGN KEY ({columnName}) 
                    REFERENCES {referenceSchemaName}.{referenceTableName} ({referenceColumnName}) 
                    ON DELETE {onDelete} 
                    ON UPDATE {onUpdate}",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public async Task<IEnumerable<ForeignKey>> GetForeignKeysAsync(
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
            $@"SELECT c.conname AS constraint_name,
                sch.nspname AS schema_name,
                tbl.relname AS table_name,
                string_agg(col.attname, ',' ORDER BY u.attposition) AS column_names,
                f_sch.nspname AS referenced_schema_name,
                f_tbl.relname AS referenced_table_name,
                string_agg(f_col.attname, ',' ORDER BY f_u.attposition) AS referenced_column_names,
                CASE
                        WHEN c.confdeltype = 'c'  THEN 'CASCADE'
                        WHEN c.confdeltype = 'n'  THEN 'SET NULL'
                        WHEN c.confdeltype = 'r'  THEN 'RESTRICT'
                        ELSE  'NO ACTION'
                end as delete_rule,
                CASE
                        WHEN c.confupdtype = 'c'  THEN 'CASCADE'
                        WHEN c.confupdtype = 'n'  THEN 'SET NULL'
                        WHEN c.confupdtype = 'r'  THEN 'RESTRICT'
                        ELSE  'NO ACTION'
                end as update_rule,
                pg_get_constraintdef(c.oid) AS definition
                FROM pg_constraint c
                    LEFT JOIN LATERAL UNNEST(c.conkey) WITH ORDINALITY AS u(attnum, attposition) ON TRUE
                    LEFT JOIN LATERAL UNNEST(c.confkey) WITH ORDINALITY AS f_u(attnum, attposition) ON f_u.attposition = u.attposition
                    JOIN pg_class tbl ON tbl.oid = c.conrelid
                    JOIN pg_namespace sch ON sch.oid = tbl.relnamespace
                    LEFT JOIN pg_attribute col ON (col.attrelid = tbl.oid AND col.attnum = u.attnum)
                    LEFT JOIN pg_class f_tbl ON f_tbl.oid = c.confrelid
                    LEFT JOIN pg_namespace f_sch ON f_sch.oid = f_tbl.relnamespace
                    LEFT JOIN pg_attribute f_col ON (f_col.attrelid = f_tbl.oid AND f_col.attnum = f_u.attnum)
                where c.contype = 'f'";
        if (!string.IsNullOrWhiteSpace(schemaName))
            sql += $@" AND sch.nspname = @schemaName";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND tbl.relname = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND c.conname LIKE @where";
        sql +=
            $@" GROUP BY schema_name, table_name, constraint_name, referenced_schema_name, referenced_table_name, definition, delete_rule, update_rule
                   ORDER BY schema_name, table_name, constraint_name";

        var results = await QueryAsync<(
            string constraint_name,
            string schema_name,
            string table_name,
            string column_names,
            string referenced_schema_name,
            string referenced_table_name,
            string referenced_column_names,
            string delete_rule,
            string update_rule,
            string definition
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

        return results.Select(r =>
        {
            var deleteRule = r.delete_rule switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "NO ACTION" => ReferentialAction.NoAction,
                _ => ReferentialAction.NoAction
            };
            var updateRule = r.update_rule switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "NO ACTION" => ReferentialAction.NoAction,
                _ => ReferentialAction.NoAction
            };
            var columnNames = r.column_names.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            var referencedColumnNames = r.referenced_column_names.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            return new ForeignKey(
                r.schema_name,
                r.constraint_name,
                r.table_name,
                columnNames.First(),
                r.referenced_table_name,
                referencedColumnNames.First(),
                deleteRule,
                updateRule
            );
        });
    }

    public async Task<IEnumerable<string>> GetForeignKeyNamesAsync(
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
            $@"SELECT c.conname 
                    FROM pg_constraint c
                        JOIN pg_class tbl ON tbl.oid = c.conrelid
                        JOIN pg_namespace sch ON sch.oid = tbl.relnamespace
                    where c.contype = 'f'";
        if (!string.IsNullOrWhiteSpace(schemaName))
            sql += $@" AND sch.nspname = @schemaName";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND tbl.relname = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND c.conname LIKE @where";
        sql += @" ORDER BY conname";

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

    public async Task<bool> DropForeignKeyIfExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await ForeignKeyExistsAsync(
                    db,
                    tableName,
                    columnName,
                    foreignKey,
                    schemaName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        if (!string.IsNullOrWhiteSpace(foreignKey))
        {
            var foreignKeyName = NormalizeName(foreignKey);

            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE {schemaName}.{tableName} DROP CONSTRAINT {foreignKeyName}",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be specified.", nameof(columnName));

            // get the name of the postgresql foreign key
            var foreignKeyName = await ExecuteScalarAsync<string>(
                    db,
                    $@"SELECT conname 
                        FROM pg_constraint 
                        WHERE conrelid = '{schemaName}.{tableName}'::regclass 
                        AND contype = 'f' 
                        AND conname IN (
                            SELECT conname 
                            FROM pg_constraint 
                            WHERE conrelid = '{schemaName}.{tableName}'::regclass 
                            AND contype = 'f' 
                            AND conkey[1] = (
                                SELECT attnum 
                                FROM pg_attribute 
                                WHERE attrelid = '{schemaName}.{tableName}'::regclass 
                                AND attname = @columnName
                            )
                        )",
                    new
                    {
                        schemaName,
                        tableName,
                        columnName
                    },
                    tx
                )
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(foreignKeyName))
            {
                await ExecuteAsync(
                        db,
                        $@"ALTER TABLE {schemaName}.{tableName} DROP CONSTRAINT {foreignKeyName}",
                        transaction: tx
                    )
                    .ConfigureAwait(false);
            }
        }

        return true;
    }
}
