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
                        new { schemaName, tableName, foreignKeyName },
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
                        new { schemaName, tableName, columnName },
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

    public Task<IEnumerable<ForeignKey>> GetForeignKeysAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetForeignKeyNamesAsync(
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
                $@"SELECT CONSTRAINT_NAME 
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                    WHERE TABLE_SCHEMA = @schemaName AND 
                          TABLE_NAME = @tableName AND 
                          CONSTRAINT_TYPE = 'FOREIGN KEY' 
                    ORDER BY CONSTRAINT_NAME",
                new { schemaName, tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

            return QueryAsync<string>(
                db,
                $@"SELECT CONSTRAINT_NAME 
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                    WHERE TABLE_SCHEMA = @schemaName AND 
                          TABLE_NAME = @tableName AND 
                          CONSTRAINT_TYPE = 'FOREIGN KEY' AND 
                          CONSTRAINT_NAME LIKE @where 
                    ORDER BY CONSTRAINT_NAME",
                new { schemaName, tableName, where },
                tx
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
                    new { schemaName, tableName, columnName },
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
