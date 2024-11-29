using System.Data;
using DapperMatic.Models;

namespace DapperMatic;

public static partial class DbConnectionExtensions
{
    #region IDatabaseIndexMethods

    /// <summary>
    /// Checks if an index exists on a specified column.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the index exists, otherwise false.</returns>
    public static async Task<bool> DoesIndexExistOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DoesIndexExistOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if an index exists.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the index exists, otherwise false.</returns>
    public static async Task<bool> DoesIndexExistAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DoesIndexExistAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates an index if it does not exist.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="index">The index constraint.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the index was created, otherwise false.</returns>
    public static async Task<bool> CreateIndexIfNotExistsAsync(
        this IDbConnection db,
        DxIndex index,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateIndexIfNotExistsAsync(db, index, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates an index if it does not exist.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <param name="columns">The columns in the index.</param>
    /// <param name="isUnique">Whether the index is unique.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the index was created, otherwise false.</returns>
    public static async Task<bool> CreateIndexIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateIndexIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                indexName,
                columns,
                isUnique,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the indexes on a specified column.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of indexes on the column.</returns>
    public static async Task<List<DxIndex>> GetIndexesOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexesOnColumnAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a specified index.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The index if found, otherwise null.</returns>
    public static async Task<DxIndex?> GetIndexAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the indexes on a specified table.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexNameFilter">The index name filter.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of indexes on the table.</returns>
    public static async Task<List<DxIndex>> GetIndexesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexesAsync(db, schemaName, tableName, indexNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the names of indexes on a specified column.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of index names on the column.</returns>
    public static async Task<List<string>> GetIndexNamesOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexNamesOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the names of indexes on a specified table.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexNameFilter">The index name filter.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of index names on the table.</returns>
    public static async Task<List<string>> GetIndexNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexNamesAsync(db, schemaName, tableName, indexNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Drops indexes on a specified column if they exist.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the indexes were dropped, otherwise false.</returns>
    public static async Task<bool> DropIndexesOnColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropIndexesOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Drops a specified index if it exists.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the index was dropped, otherwise false.</returns>
    public static async Task<bool> DropIndexIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropIndexIfExistsAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseIndexMethods
}
