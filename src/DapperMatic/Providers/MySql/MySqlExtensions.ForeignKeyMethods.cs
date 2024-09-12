using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        if (!string.IsNullOrWhiteSpace(foreignKey))
        {
            var foreignKeyName = NormalizeName(foreignKey);

            return 0
                < await ExecuteScalarAsync<int>(
                        db,
                        $@"SELECT COUNT(*) 
                            FROM information_schema.TABLE_CONSTRAINTS 
                            WHERE TABLE_SCHEMA = DATABASE() AND 
                                  TABLE_NAME = @tableName AND 
                                  CONSTRAINT_NAME = @foreignKeyName AND
                                  CONSTRAINT_TYPE = 'FOREIGN KEY'",
                        new { tableName, foreignKeyName },
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
                            FROM information_schema.KEY_COLUMN_USAGE 
                            WHERE TABLE_SCHEMA = DATABASE() AND 
                                  TABLE_NAME = @tableName AND 
                                  COLUMN_NAME = @columnName AND
                                  REFERENCED_TABLE_NAME IS NOT NULL",
                        new { tableName, columnName },
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

        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);
        var (_, referenceTableName, referenceColumnName) = NormalizeNames(
            schemaName,
            referenceTable,
            referenceColumn
        );

        var foreignKeyName = NormalizeName(foreignKey);

        // add the foreign key to the MySql database tableName
        await ExecuteAsync(
                db,
                $@"ALTER TABLE `{tableName}` 
                    ADD CONSTRAINT `{foreignKeyName}` 
                    FOREIGN KEY (`{columnName}`) 
                    REFERENCES `{referenceTableName}` (`{referenceColumnName}`) 
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
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name must be specified.", nameof(tableName));

        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? ""
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            $@"SELECT 
                        kcu.CONSTRAINT_NAME as constraint_name, 
                        kcu.TABLE_NAME as table_name, 
                        kcu.COLUMN_NAME as column_name, 
                        kcu.REFERENCED_TABLE_NAME as referenced_table_name, 
                        kcu.REFERENCED_COLUMN_NAME as referenced_column_name, 
                        rc.DELETE_RULE as delete_rule, 
                        rc.UPDATE_RULE as update_rule
                    FROM information_schema.KEY_COLUMN_USAGE kcu
                    INNER JOIN information_schema.referential_constraints rc ON kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME 
                    WHERE kcu.TABLE_SCHEMA = DATABASE() AND kcu.REFERENCED_TABLE_NAME IS NOT NULL";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND kcu.TABLE_NAME = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND kcu.CONSTRAINT_NAME LIKE @where";
        sql += " ORDER BY kcu.TABLE_NAME, kcu.CONSTRAINT_NAME";

        var results = await QueryAsync<(
            string constraint_name,
            string table_name,
            string column_name,
            string referenced_table_name,
            string referenced_column_name,
            string delete_rule,
            string update_rule
        )>(db, sql, new { tableName, where }, tx)
            .ConfigureAwait(false);

        return results.Select(r =>
        {
            var deleteRule = r.delete_rule switch
            {
                "NO ACTION" => ReferentialAction.NoAction,
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                _ => ReferentialAction.NoAction
            };
            var updateRule = r.update_rule switch
            {
                "NO ACTION" => ReferentialAction.NoAction,
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                _ => ReferentialAction.NoAction
            };
            return new ForeignKey(
                null,
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
        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            $@"SELECT CONSTRAINT_NAME 
                    FROM information_schema.TABLE_CONSTRAINTS 
                    WHERE TABLE_SCHEMA = DATABASE() AND 
                          CONSTRAINT_TYPE = 'FOREIGN KEY'"
            + (string.IsNullOrWhiteSpace(tableName) ? "" : " AND TABLE_NAME = @tableName")
            + (string.IsNullOrWhiteSpace(where) ? "" : " AND CONSTRAINT_NAME LIKE @where")
            + @" ORDER BY CONSTRAINT_NAME";

        return await QueryAsync<string>(db, sql, new { tableName, where }, tx)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// In SQLite, to drop a foreign key, you must re-create the tableName without the foreign key,
    /// and then re-insert the data. It's a costly operation.
    /// </summary>
    /// <remarks>
    /// Example: https://www.techonthenet.com/sqlite/foreign_keys/drop.php
    /// </remarks>
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
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name must be specified.", nameof(tableName));
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name must be specified.", nameof(columnName));

        var fkExists = await ForeignKeyExistsAsync(
                db,
                tableName,
                columnName,
                foreignKey,
                schemaName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (!fkExists)
            return false;

        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        if (!string.IsNullOrWhiteSpace(foreignKey))
        {
            var foreignKeyName = NormalizeName(foreignKey);
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{foreignKeyName}`",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be specified.", nameof(columnName));

            // get the name of the foreign key for the columnName
            var foreignKeyName = await ExecuteScalarAsync<string>(
                    db,
                    $@"SELECT CONSTRAINT_NAME 
                        FROM information_schema.KEY_COLUMN_USAGE 
                        WHERE TABLE_SCHEMA = DATABASE() AND 
                              TABLE_NAME = @tableName AND 
                              COLUMN_NAME = @columnName AND
                              REFERENCED_TABLE_NAME IS NOT NULL",
                    new { tableName, columnName },
                    tx
                )
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(foreignKeyName))
            {
                await ExecuteAsync(
                        db,
                        $@"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{foreignKeyName}`",
                        transaction: tx
                    )
                    .ConfigureAwait(false);
            }
        }

        return true;
    }
}
