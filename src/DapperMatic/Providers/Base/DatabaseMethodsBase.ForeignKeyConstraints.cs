using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseForeignKeyConstraintMethods
{
    public virtual async Task<bool> ForeignKeyConstraintExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetForeignKeyConstraintAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> ForeignKeyConstraintExistsOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetForeignKeyConstraintOnColumnAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> CreateForeignKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        DxForeignKeyConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateForeignKeyConstraintIfNotExistsAsync(
                db,
                constraint.SchemaName,
                constraint.TableName,
                constraint.ConstraintName,
                constraint.SourceColumns,
                constraint.ReferencedTableName,
                constraint.ReferencedColumns,
                constraint.OnDelete,
                constraint.OnUpdate,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public abstract Task<bool> CreateForeignKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] sourceColumns,
        string referencedTableName,
        DxOrderedColumn[] referencedColumns,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    public virtual async Task<DxForeignKeyConstraint?> GetForeignKeyConstraintAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required.", nameof(constraintName));

        var foreignKeyConstraints = await GetForeignKeyConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return foreignKeyConstraints.SingleOrDefault();
    }

    public virtual async Task<string?> GetForeignKeyConstraintNameOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await GetForeignKeyConstraintOnColumnAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )?.ConstraintName;
    }

    public virtual async Task<List<string>> GetForeignKeyConstraintNamesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var foreignKeyConstraints = await GetForeignKeyConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return foreignKeyConstraints.Select(c => c.ConstraintName).ToList();
    }

    public virtual async Task<DxForeignKeyConstraint?> GetForeignKeyConstraintOnColumnAsync(
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

        var foreignKeyConstraints = await GetForeignKeyConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return foreignKeyConstraints.FirstOrDefault(c =>
            c.SourceColumns.Any(sc =>
                sc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    public abstract Task<List<DxForeignKeyConstraint>> GetForeignKeyConstraintsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    public virtual async Task<bool> DropForeignKeyConstraintOnColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var constraintName = await GetForeignKeyConstraintNameOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return constraintName != null
            && await DropForeignKeyConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
    }

    public virtual async Task<bool> DropForeignKeyConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !(
                await ForeignKeyConstraintExistsAsync(
                        db,
                        schemaName,
                        tableName,
                        constraintName,
                        tx,
                        cancellationToken
                    )
                    .ConfigureAwait(false)
            )
        )
            return false;

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        if (await SupportsSchemasAsync(db, tx, cancellationToken).ConfigureAwait(false))
        {
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE {schemaName}.{tableName} 
                    DROP CONSTRAINT {constraintName}",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE {tableName} 
                    DROP CONSTRAINT {constraintName}",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }

        return true;
    }
}