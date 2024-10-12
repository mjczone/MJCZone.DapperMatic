using System.Data;
using DapperMatic.Interfaces;
using DapperMatic.Models;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase : IDatabasePrimaryKeyConstraintMethods
{
    public virtual async Task<bool> DoesPrimaryKeyConstraintExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetPrimaryKeyConstraintAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        DxPrimaryKeyConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreatePrimaryKeyConstraintIfNotExistsAsync(
            db,
            constraint.SchemaName,
            constraint.TableName,
            constraint.ConstraintName,
            constraint.Columns,
            tx,
            cancellationToken
        );
    }

    public virtual async Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required.", nameof(constraintName));

        if (columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await DoesPrimaryKeyConstraintExistAsync(
                    db,
                    schemaName,
                    tableName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var supportsOrderedKeysInConstraints = await SupportsOrderedKeysInConstraintsAsync(
                db,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        var sql = SqlAlterTableAddPrimaryKeyConstraint(
            schemaName,
            tableName,
            constraintName,
            columns,
            supportsOrderedKeysInConstraints
        );

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<DxPrimaryKeyConstraint?> GetPrimaryKeyConstraintAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
            
        if (table?.PrimaryKeyConstraint is null)
            return null;

        return table.PrimaryKeyConstraint;
    }

    public virtual async Task<bool> DropPrimaryKeyConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var primaryKeyConstraint = await GetPrimaryKeyConstraintAsync(
            db,
            schemaName,
            tableName,
            tx,
            cancellationToken
        );

        if (string.IsNullOrWhiteSpace(primaryKeyConstraint?.ConstraintName))
            return false;

        var sql = SqlDropPrimaryKeyConstraint(schemaName, tableName, primaryKeyConstraint.ConstraintName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }
}
