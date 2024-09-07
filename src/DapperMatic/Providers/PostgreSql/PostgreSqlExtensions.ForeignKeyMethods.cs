using System.Data;

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

    public Task<IEnumerable<string>> GetForeignKeysAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return QueryAsync<string>(
                db,
                $@"SELECT conname 
                    FROM pg_constraint 
                    WHERE conrelid = '{schemaName}.{tableName}'::regclass 
                    AND contype = 'f'",
                transaction: tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"SELECT conname 
                    FROM pg_constraint 
                    WHERE conrelid = '{schemaName}.{tableName}'::regclass 
                    AND contype = 'f' 
                    AND conname LIKE @where",
                new
                {
                    schemaName,
                    tableName,
                    where
                },
                transaction: tx
            );
        }
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
