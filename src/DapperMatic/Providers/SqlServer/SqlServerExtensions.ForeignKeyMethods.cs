using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                        WHERE TABLE_SCHEMA = @schemaName AND 
                              TABLE_NAME = @tableName AND 
                              CONSTRAINT_NAME = @foreignKeyName AND
                              CONSTRAINT_TYPE = 'FOREIGN KEY'",
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

            var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

            return 0
                < await ExecuteScalarAsync<int>(
                        db,
                        $@"SELECT COUNT(*) 
                            FROM sys.foreign_keys AS f
                            INNER JOIN sys.foreign_key_columns AS fc
                                ON f.object_id = fc.constraint_object_id
                            WHERE f.parent_object_id = OBJECT_ID('{schemaAndTableName}') AND
                                COL_NAME(fc.parent_object_id, fc.parent_column_id) = @columnName",
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
                $@"
                ALTER TABLE [{schemaName}].[{tableName}] 
                    ADD CONSTRAINT [{foreignKeyName}] 
                    FOREIGN KEY ([{columnName}]) 
                    REFERENCES [{referenceSchemaName}].[{referenceTableName}] ([{referenceColumnName}]) 
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
            $@"select
                    fk.name as constraint_name,
                    schema_name(fk_tab.schema_id) as schema_name,
                    fk_tab.name as table_name,
                    substring(fk_column_names, 1, len(fk_column_names)-1) as [column_name],
                    schema_name(pk_tab.schema_id) as referenced_schema_name,
                    pk_tab.name as referenced_table_name,
                    substring(pk_column_names, 1, len(pk_column_names)-1) as [referenced_column_name],
                    fk.delete_referential_action_desc as delete_rule,
                    fk.update_referential_action_desc  as update_rule
                from sys.foreign_keys fk
                    inner join sys.tables fk_tab on fk_tab.object_id = fk.parent_object_id
                    inner join sys.tables pk_tab on pk_tab.object_id = fk.referenced_object_id
                    cross apply (select col.[name] + ', '
                                    from sys.foreign_key_columns fk_c
                                        inner join sys.columns col
                                            on fk_c.parent_object_id = col.object_id
                                            and fk_c.parent_column_id = col.column_id
                                    where fk_c.parent_object_id = fk_tab.object_id
                                    and fk_c.constraint_object_id = fk.object_id
                                            order by col.column_id
                                            for xml path ('') ) D (fk_column_names)
                    cross apply (select col.[name] + ', '
                                    from sys.foreign_key_columns fk_c
                                        inner join sys.columns col
                                            on fk_c.referenced_object_id = col.object_id
                                            and fk_c.referenced_column_id = col.column_id
                                    where fk_c.referenced_object_id = pk_tab.object_id
                                    and fk_c.constraint_object_id = fk.object_id
                                            order by col.column_id
                                            for xml path ('') ) G (pk_column_names)
            where 1 = 1";
        if (!string.IsNullOrWhiteSpace(schemaName))
            sql += $@" AND schema_name(fk_tab.schema_id) = @schemaName";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND fk_tab.name = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND fk.name LIKE @where";
        sql +=
            $@" 
            order by schema_name(fk_tab.schema_id), fk_tab.name, fk.name";

        var results = await QueryAsync<(
            string constraint_name,
            string schema_name,
            string table_name,
            string column_name,
            string referenced_schema_name,
            string referenced_table_name,
            string referenced_column_name,
            string delete_rule,
            string update_rule
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
            var deleteRule = (r.delete_rule ?? "").Replace('_', ' ') switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "NO ACTION" => ReferentialAction.NoAction,
                _ => ReferentialAction.NoAction
            };
            var updateRule = (r.update_rule ?? "").Replace('_', ' ') switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "NO ACTION" => ReferentialAction.NoAction,
                _ => ReferentialAction.NoAction
            };

            return new ForeignKey(
                r.schema_name,
                r.constraint_name,
                r.table_name,
                r.column_name,
                r.referenced_table_name,
                r.referenced_column_name,
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
            $@"SELECT CONSTRAINT_NAME 
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                    where 
                          CONSTRAINT_TYPE = 'FOREIGN KEY'";
        if (!string.IsNullOrWhiteSpace(schemaName))
            sql += $@" AND TABLE_SCHEMA = @schemaName";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND TABLE_NAME = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND CONSTRAINT_NAME LIKE @where";
        sql += @" ORDER BY CONSTRAINT_NAME";

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
                    $@"ALTER TABLE [{schemaName}].[{tableName}] DROP CONSTRAINT [{foreignKeyName}]",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be specified.", nameof(columnName));

            var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

            // get the name of the foreign key
            var foreignKeyName = await ExecuteScalarAsync<string>(
                    db,
                    $@"SELECT top 1 f.name
                        FROM sys.foreign_keys AS f
                        INNER JOIN sys.foreign_key_columns AS fc
                            ON f.object_id = fc.constraint_object_id
                        WHERE f.parent_object_id = OBJECT_ID('{schemaAndTableName}') AND
                            COL_NAME(fc.parent_object_id, fc.parent_column_id) = @columnName",
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
                        $@"ALTER TABLE [{schemaName}].[{tableName}] DROP CONSTRAINT [{foreignKeyName}]",
                        transaction: tx
                    )
                    .ConfigureAwait(false);
            }
        }

        return true;
    }
}
