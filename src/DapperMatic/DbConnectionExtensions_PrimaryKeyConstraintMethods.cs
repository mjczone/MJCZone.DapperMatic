using System.Data;
using DapperMatic.Models;

namespace DapperMatic;

public static partial class DbConnectionExtensions
{
    #region IDatabasePrimaryKeyConstraintMethods

    /// <summary>
    /// Checks if a primary key constraint exists in the specified table.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="tx">The transaction to use, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the primary key constraint exists, otherwise false.</returns>
    public static async Task<bool> DoesPrimaryKeyConstraintExistAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DoesPrimaryKeyConstraintExistAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a primary key constraint if it does not exist.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="constraint">The primary key constraint.</param>
    /// <param name="tx">The transaction to use, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the primary key constraint was created, otherwise false.</returns>
    public static async Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        this IDbConnection db,
        DxPrimaryKeyConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreatePrimaryKeyConstraintIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a primary key constraint if it does not exist.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="columns">The columns that make up the primary key.</param>
    /// <param name="tx">The transaction to use, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the primary key constraint was created, otherwise false.</returns>
    public static async Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreatePrimaryKeyConstraintIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                columns,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the primary key constraint for the specified table.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="tx">The transaction to use, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The primary key constraint, or null if it does not exist.</returns>
    public static async Task<DxPrimaryKeyConstraint?> GetPrimaryKeyConstraintAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetPrimaryKeyConstraintAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Drops the primary key constraint if it exists.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="tx">The transaction to use, or null.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the primary key constraint was dropped, otherwise false.</returns>
    public static async Task<bool> DropPrimaryKeyConstraintIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropPrimaryKeyConstraintIfExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion // IDatabasePrimaryKeyConstraintMethods
}
