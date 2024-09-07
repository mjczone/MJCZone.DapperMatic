using System.Data;

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

    public Task<IEnumerable<string>> GetForeignKeysAsync(
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

        if (string.IsNullOrWhiteSpace(nameFilter))
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
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
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
