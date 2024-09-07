using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> IndexExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        // does indexName exist in MySql tableName
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
        string tableName,
        string indexName,
        string[] columnNames,
        string? schemaName = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (columnNames == null || columnNames.Length == 0)
            throw new ArgumentException(
                "At least one columnName must be specified.",
                nameof(columnNames)
            );

        if (
            await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columnNames);
        await ExecuteAsync(
                db,
                $@"ALTER TABLE `{tableName}`
                    ADD {uniqueString} INDEX `{indexName}` ({columnList})",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public Task<IEnumerable<TableIndex>> GetIndexesAsync(
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

    public Task<IEnumerable<string>> GetIndexNamesAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        if (string.IsNullOrWhiteSpace(nameFilter))
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
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
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
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (
            !await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
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
