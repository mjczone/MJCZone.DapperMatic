using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase
{
    public virtual async Task<bool> DoesDefaultConstraintExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetDefaultConstraintAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> DoesDefaultConstraintExistOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetDefaultConstraintOnColumnAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> CreateDefaultConstraintIfNotExistsAsync(
        IDbConnection db,
        DxDefaultConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateDefaultConstraintIfNotExistsAsync(
                db,
                constraint.SchemaName,
                constraint.TableName,
                constraint.ColumnName,
                constraint.ConstraintName,
                constraint.Expression,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<bool> CreateDefaultConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required.", nameof(constraintName));

        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression is required.", nameof(expression));

        if (
            await DoesDefaultConstraintExistAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var sql = SqlAlterTableAddDefaultConstraint(
            schemaName,
            tableName,
            columnName,
            constraintName,
            expression
        );

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<DxDefaultConstraint?> GetDefaultConstraintAsync(
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

        var defaultConstraints = await GetDefaultConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        return defaultConstraints.SingleOrDefault();
    }

    public virtual async Task<string?> GetDefaultConstraintNameOnColumnAsync(
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

        var defaultConstraints = await GetDefaultConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        return defaultConstraints
            .FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.ColumnName)
                && c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
            ?.ConstraintName;
    }

    public virtual async Task<List<string>> GetDefaultConstraintNamesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var checkConstraints = await GetDefaultConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return checkConstraints.Select(c => c.ConstraintName).ToList();
    }

    public virtual async Task<DxDefaultConstraint?> GetDefaultConstraintOnColumnAsync(
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

        var defaultConstraints = await GetDefaultConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        return defaultConstraints.FirstOrDefault(c =>
            !string.IsNullOrWhiteSpace(c.ColumnName)
            && c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
        );
    }

    public virtual async Task<List<DxDefaultConstraint>> GetDefaultConstraintsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            return [];

        var filter = string.IsNullOrWhiteSpace(constraintNameFilter)
            ? null
            : ToSafeString(constraintNameFilter);

        return string.IsNullOrWhiteSpace(filter)
            ? table.DefaultConstraints
            : table
                .DefaultConstraints.Where(c => c.ConstraintName.IsWildcardPatternMatch(filter))
                .ToList();
    }

    public virtual async Task<bool> DropDefaultConstraintOnColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var constraintName = await GetDefaultConstraintNameOnColumnAsync(
            db,
            schemaName,
            tableName,
            columnName,
            tx,
            cancellationToken
        );

        if (string.IsNullOrWhiteSpace(constraintName))
            return false;

        var sql = SqlDropDefaultConstraint(schemaName, tableName, columnName, constraintName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> DropDefaultConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var defaultConstraint = await GetDefaultConstraintAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(defaultConstraint?.ColumnName))
            return false;

        var sql = SqlDropDefaultConstraint(
            schemaName,
            tableName,
            defaultConstraint.ColumnName,
            constraintName
        );

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }
}
