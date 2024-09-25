using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseIndexMethods
{
    public virtual async Task<bool> IndexExistsAsync(
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

    public virtual async Task<bool> IndexesExistOnColumnAsync(
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
        DxIndex constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateIndexIfNotExistsAsync(
                db,
                constraint.SchemaName,
                constraint.TableName,
                constraint.IndexName,
                constraint.Columns,
                constraint.IsUnique,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public abstract Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

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

    public abstract Task<List<DxIndex>> GetIndexesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

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
            !await IndexExistsAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        var compoundTableName = await SupportsSchemasAsync(db, tx, cancellationToken)
            .ConfigureAwait(false)
            ? $"{schemaName}.{tableName}"
            : tableName;

        // drop index
        await ExecuteAsync(db, $@"DROP INDEX {indexName} ON {compoundTableName}", transaction: tx)
            .ConfigureAwait(false);

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
            await DropIndexIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    indexName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        return true;
    }
}
