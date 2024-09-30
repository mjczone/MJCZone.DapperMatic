using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

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

    public abstract Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

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
        if (primaryKeyConstraint is null)
            return false;

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} 
                    DROP CONSTRAINT {primaryKeyConstraint.ConstraintName}",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
