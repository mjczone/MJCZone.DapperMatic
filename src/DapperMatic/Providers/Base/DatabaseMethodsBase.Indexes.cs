using System.Data;
using DapperMatic.Interfaces;
using DapperMatic.Models;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase : IDatabaseIndexMethods
{
    public virtual async Task<bool> DoesIndexExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetIndexAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> DoesIndexExistOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
                await GetIndexesOnColumnAsync(
                        db,
                        schemaName,
                        tableName,
                        columnName,
                        tx,
                        cancellationToken
                    )
                    .ConfigureAwait(false)
            ).Count > 0;
    }

    public virtual async Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        DxIndex index,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateIndexIfNotExistsAsync(
                db,
                index.SchemaName,
                index.TableName,
                index.IndexName,
                index.Columns,
                index.IsUnique,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException("Index name is required.", nameof(indexName));
        }

        if (
            await DoesIndexExistAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            return false;
        }

        var sql = SqlCreateIndex(schemaName, tableName, indexName, columns, isUnique);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<DxIndex?> GetIndexAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(indexName))
            throw new ArgumentException("Index name is required.", nameof(indexName));

        var indexes = await GetIndexesAsync(
                db,
                schemaName,
                tableName,
                indexName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return indexes.SingleOrDefault();
    }

    public virtual async Task<List<DxIndex>> GetIndexesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        return await GetIndexesInternalAsync(
            db,
            schemaName,
            tableName,
            string.IsNullOrWhiteSpace(indexNameFilter) ? null : indexNameFilter,
            tx,
            cancellationToken
        );
    }

    public virtual async Task<List<string>> GetIndexNamesOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await GetIndexesOnColumnAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            .Select(x => x.IndexName)
            .ToList();
    }

    public virtual async Task<List<string>> GetIndexNamesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await GetIndexesAsync(db, schemaName, tableName, indexNameFilter, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            .Select(x => x.IndexName)
            .ToList();
    }

    public virtual async Task<List<DxIndex>> GetIndexesOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name is required.", nameof(columnName));

        var indexes = await GetIndexesAsync(db, schemaName, tableName, null, tx, cancellationToken)
            .ConfigureAwait(false);

        return indexes
            .Where(c =>
                c.Columns.Any(x =>
                    x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
            )
            .ToList();
    }

    public virtual async Task<bool> DropIndexIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await DoesIndexExistAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var sql = SqlDropIndex(schemaName, tableName, indexName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> DropIndexesOnColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var indexNames = await GetIndexNamesOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (indexNames.Count == 0)
            return false;

        foreach (var indexName in indexNames)
        {
            var sql = SqlDropIndex(schemaName, tableName, indexName);
            await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);
        }

        return true;
    }
}
