using System.Data;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> IndexExistsAsync(
        IDbConnection db,
        string table,
        string index,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, indexName) = NormalizeNames(schema, table, index);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"
                    SELECT COUNT(*)
                    FROM pg_indexes
                    WHERE schemaname = @schemaName AND
                          tablename = @tableName AND
                          indexname = @indexName
                    ",
                    new
                    {
                        schemaName,
                        tableName,
                        indexName
                    },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        string table,
        string index,
        string[] columns,
        string? schema = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, indexName) = NormalizeNames(schema, table, index);

        if (columns == null || columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columns);

        await ExecuteAsync(
                db,
                $@"
                CREATE {uniqueString} INDEX {indexName} ON {schemaName}.{tableName} ({columnList})
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public Task<IEnumerable<string>> GetIndexesAsync(
        IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, _) = NormalizeNames(schema, table);
        if (string.IsNullOrWhiteSpace(filter))
        {
            // get all indexes in postgresql table
            return QueryAsync<string>(
                db,
                $@"
                SELECT indexname
                FROM pg_indexes
                WHERE schemaname = @schemaName AND
                      tablename = @tableName
                ORDER BY indexname
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
                SELECT indexname
                FROM pg_indexes
                WHERE schemaname = @schemaName AND
                      tablename = @tableName AND
                      indexname LIKE @where
                ORDER BY indexname
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

    public async Task<bool> DropIndexIfExistsAsync(
        IDbConnection db,
        string table,
        string index,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, indexName) = NormalizeNames(schema, table, index);

        if (
            !await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        await ExecuteAsync(
            db,
            $@"
                DROP INDEX {schemaName}.{indexName}
                ",
            transaction: tx
        );

        return true;
    }
}
