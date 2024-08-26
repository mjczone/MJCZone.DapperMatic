using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> ForeignKeyExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string? foreignKey = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (_, tableName, columnName) = NormalizeNames(schema, table, column);

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
                throw new ArgumentException("Column name must be specified.", nameof(column));

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
        string table,
        string column,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schema = null,
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
                "Reference table name must be specified.",
                nameof(referenceTable)
            );
        if (string.IsNullOrWhiteSpace(referenceColumn))
            throw new ArgumentException(
                "Reference column name must be specified.",
                nameof(referenceColumn)
            );
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("Column name must be specified.", nameof(column));
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name must be specified.", nameof(table));

        if (
            await ForeignKeyExistsAsync(
                    db,
                    table,
                    column,
                    foreignKey,
                    schema,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var (_, tableName, columnName) = NormalizeNames(schema, table, column);
        var (_, referenceTableName, referenceColumnName) = NormalizeNames(
            schema,
            referenceTable,
            referenceColumn
        );

        var foreignKeyName = NormalizeName(foreignKey);

        // add the foreign key to the MySql database table
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

    public Task<IEnumerable<string>> GetForeignKeysAsync(
        IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name must be specified.", nameof(table));

        var (_, tableName, _) = NormalizeNames(schema, table);

        if (string.IsNullOrWhiteSpace(filter))
        {
            return QueryAsync<string>(
                db,
                $@"SELECT CONSTRAINT_NAME 
                    FROM information_schema.TABLE_CONSTRAINTS 
                    WHERE TABLE_SCHEMA = DATABASE() AND 
                          TABLE_NAME = @tableName AND 
                          CONSTRAINT_TYPE = 'FOREIGN KEY'
                    ORDER BY CONSTRAINT_NAME",
                new { tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"SELECT CONSTRAINT_NAME
                    FROM information_schema.TABLE_CONSTRAINTS 
                    WHERE TABLE_SCHEMA = DATABASE() AND 
                          TABLE_NAME = @tableName AND 
                          CONSTRAINT_TYPE = 'FOREIGN KEY' AND 
                          CONSTRAINT_NAME LIKE @where
                    ORDER BY CONSTRAINT_NAME",
                new { tableName, where },
                tx
            );
        }
    }

    /// <summary>
    /// In SQLite, to drop a foreign key, you must re-create the table without the foreign key,
    /// and then re-insert the data. It's a costly operation.
    /// </summary>
    /// <remarks>
    /// Example: https://www.techonthenet.com/sqlite/foreign_keys/drop.php
    /// </remarks>
    public async Task<bool> DropForeignKeyIfExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string? foreignKey = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name must be specified.", nameof(table));
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("Column name must be specified.", nameof(column));

        var fkExists = await ForeignKeyExistsAsync(
                db,
                table,
                column,
                foreignKey,
                schema,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (!fkExists)
            return false;

        var (_, tableName, columnName) = NormalizeNames(schema, table, column);

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
                throw new ArgumentException("Column name must be specified.", nameof(column));

            // get the name of the foreign key for the column
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
