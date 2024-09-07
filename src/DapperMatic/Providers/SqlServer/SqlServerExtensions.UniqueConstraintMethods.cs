using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        (schemaName, tableName, uniqueConstraintName) = NormalizeNames(
            schemaName,
            tableName,
            uniqueConstraintName
        );
        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                        AND name = @uniqueConstraintName and is_primary_key = 0 and is_unique_constraint = 1",
                    new { schemaAndTableName, uniqueConstraintName },
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
        (schemaName, tableName, uniqueConstraintName) = NormalizeNames(
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

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        var columnList = string.Join(", ", columnNames);

        await ExecuteAsync(
                db,
                $@"
                ALTER TABLE {schemaAndTableName}
                ADD CONSTRAINT {uniqueConstraintName} UNIQUE ({columnList})
                ",
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
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName, null);

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return QueryAsync<string>(
                db,
                $@"
                    SELECT name FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                            and is_primary_key = 0 and is_unique_constraint = 1",
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"
                    SELECT name FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                            and name LIKE @where
                            and is_primary_key = 0 and is_unique_constraint = 1",
                new { schemaAndTableName, where },
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

        (schemaName, tableName, uniqueConstraintName) = NormalizeNames(
            schemaName,
            tableName,
            uniqueConstraintName
        );
        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

        await ExecuteAsync(
                db,
                $@"
                ALTER TABLE {schemaAndTableName}
                DROP CONSTRAINT {uniqueConstraintName}
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
