using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> UniqueConstraintExistsAsync(
        IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, uniqueConstraintName) = NormalizeNames(
            schemaName,
            tableName,
            uniqueConstraintName
        );

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                        FROM information_schema.TABLE_CONSTRAINTS 
                        WHERE TABLE_SCHEMA = DATABASE() AND 
                              TABLE_NAME = @tableName AND 
                              CONSTRAINT_NAME = @uniqueConstraintName AND
                              CONSTRAINT_TYPE = 'UNIQUE'",
                    new { tableName, uniqueConstraintName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string[] columnNames,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, uniqueConstraintName) = NormalizeNames(
            schemaName,
            tableName,
            uniqueConstraintName
        );

        if (columnNames == null || columnNames.Length == 0)
            throw new ArgumentException(
                "At least one columnName must be specified.",
                nameof(columnNames)
            );

        if (
            await UniqueConstraintExistsAsync(
                    db,
                    tableName,
                    uniqueConstraintName,
                    schemaName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var columnList = string.Join(", ", columnNames);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE `{tableName}`
                    ADD CONSTRAINT `{uniqueConstraintName}` UNIQUE ({columnList})",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public Task<IEnumerable<string>> GetUniqueConstraintsAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return QueryAsync<string>(
                db,
                $@"
                    SELECT CONSTRAINT_NAME 
                        FROM information_schema.TABLE_CONSTRAINTS 
                        WHERE TABLE_SCHEMA = DATABASE() AND 
                              TABLE_NAME = @tableName AND 
                              CONSTRAINT_TYPE = 'UNIQUE'",
                new { tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"
                    SELECT CONSTRAINT_NAME
                        FROM information_schema.TABLE_CONSTRAINTS 
                        WHERE TABLE_SCHEMA = DATABASE() AND 
                              TABLE_NAME = @tableName AND 
                              CONSTRAINT_TYPE = 'UNIQUE' AND 
                              CONSTRAINT_NAME LIKE @where",
                new { tableName, where },
                tx
            );
        }
    }

    public async Task<bool> DropUniqueConstraintIfExistsAsync(
        IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await UniqueConstraintExistsAsync(
                    db,
                    tableName,
                    uniqueConstraintName,
                    schemaName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        (_, tableName, uniqueConstraintName) = NormalizeNames(
            schemaName,
            tableName,
            uniqueConstraintName
        );

        // drop unique constraint in MySql 5.7
        await ExecuteAsync(
                db,
                $@"ALTER TABLE `{tableName}`
                    DROP INDEX `{uniqueConstraintName}`",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
