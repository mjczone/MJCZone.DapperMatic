using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        var (_, tableName, indexName) = NormalizeNames(schema, table, index);

        // does index exist in MySql table
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                        FROM information_schema.STATISTICS 
                        WHERE TABLE_SCHEMA = DATABASE() AND 
                              TABLE_NAME = @tableName AND 
                              INDEX_NAME = @indexName",
                    new { tableName, indexName },
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
        var (_, tableName, indexName) = NormalizeNames(schema, table, index);

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
                $@"ALTER TABLE `{tableName}`
                    ADD {uniqueString} INDEX `{indexName}` ({columnList})",
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
        var (_, tableName, _) = NormalizeNames(schema, table);

        if (string.IsNullOrWhiteSpace(filter))
        {
            return QueryAsync<string>(
                db,
                $@"SELECT INDEX_NAME 
                    FROM information_schema.STATISTICS 
                    WHERE TABLE_SCHEMA = DATABASE() AND 
                          TABLE_NAME = @tableName
                    ORDER BY INDEX_NAME",
                new { tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"SELECT INDEX_NAME 
                    FROM information_schema.STATISTICS 
                    WHERE TABLE_SCHEMA = DATABASE() AND 
                          TABLE_NAME = @tableName AND 
                          INDEX_NAME LIKE @where
                    ORDER BY INDEX_NAME",
                new { tableName, where },
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
        var (_, tableName, indexName) = NormalizeNames(schema, table, index);

        if (
            !await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        await ExecuteAsync(
            db,
            $@"ALTER TABLE `{tableName}`
                    DROP INDEX `{indexName}`",
            transaction: tx
        );

        return true;
    }
}
