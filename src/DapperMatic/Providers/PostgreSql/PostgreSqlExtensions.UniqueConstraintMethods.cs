using System.Data;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"
                    SELECT COUNT(*)
                    FROM information_schema.table_constraints
                    WHERE table_schema = @schemaName AND
                          table_name = @tableName AND
                          constraint_name = @uniqueConstraintName AND
                          constraint_type = 'UNIQUE'
                    ",
                    new
                    {
                        schemaName,
                        tableName,
                        uniqueConstraintName
                    },
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

        var columnList = string.Join(", ", columnNames);

        await ExecuteAsync(
                db,
                $@"
                ALTER TABLE {schemaName}.{tableName}
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

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return QueryAsync<string>(
                db,
                $@"
                SELECT constraint_name
                FROM information_schema.table_constraints
                WHERE table_schema = @schemaName AND
                      table_name = @tableName AND
                      constraint_type = 'UNIQUE'
                ORDER BY constraint_name
                ",
                new { schemaName, tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"
                SELECT constraint_name
                FROM information_schema.table_constraints
                WHERE table_schema = @schemaName AND
                      table_name = @tableName AND
                      constraint_type = 'UNIQUE' AND
                      constraint_name LIKE @where
                ORDER BY constraint_name
                ",
                new
                {
                    schemaName,
                    tableName,
                    where
                },
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

        await ExecuteAsync(
                db,
                $@"
                ALTER TABLE {schemaName}.{tableName}
                DROP CONSTRAINT {uniqueConstraintName}
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
