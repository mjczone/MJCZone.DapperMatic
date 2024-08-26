using System.Data;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> UniqueConstraintExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, uniqueConstraintName) = NormalizeNames(
            schema,
            table,
            uniqueConstraint
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
        string table,
        string uniqueConstraint,
        string[] columns,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, uniqueConstraintName) = NormalizeNames(
            schema,
            table,
            uniqueConstraint
        );

        if (columns == null || columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await UniqueConstraintExistsAsync(
                    db,
                    table,
                    uniqueConstraint,
                    schema,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var columnList = string.Join(", ", columns);

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
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, _) = NormalizeNames(schema, table, null);

        if (string.IsNullOrWhiteSpace(filter))
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
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
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
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await UniqueConstraintExistsAsync(
                    db,
                    table,
                    uniqueConstraint,
                    schema,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var (schemaName, tableName, uniqueConstraintName) = NormalizeNames(
            schema,
            table,
            uniqueConstraint
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
