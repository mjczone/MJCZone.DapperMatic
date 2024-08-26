using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        var columnList = string.Join(", ", columns);

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
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, _) = NormalizeNames(schema, table, null);

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

        if (string.IsNullOrWhiteSpace(filter))
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
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
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
