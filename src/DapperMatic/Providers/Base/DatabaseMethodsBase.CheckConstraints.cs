using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseCheckConstraintMethods
{
    public virtual async Task<bool> DoesCheckConstraintExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await SupportsCheckConstraintsAsync(db, tx, cancellationToken).ConfigureAwait(false))
            return false;

        return await GetCheckConstraintAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> DoesCheckConstraintExistOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await SupportsCheckConstraintsAsync(db, tx, cancellationToken).ConfigureAwait(false))
            return false;

        return await GetCheckConstraintOnColumnAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> CreateCheckConstraintIfNotExistsAsync(
        IDbConnection db,
        DxCheckConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await SupportsCheckConstraintsAsync(db, tx, cancellationToken).ConfigureAwait(false))
            return false;

        return await CreateCheckConstraintIfNotExistsAsync(
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

    public virtual async Task<bool> CreateCheckConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnName,
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

        if (!await SupportsCheckConstraintsAsync(db, tx, cancellationToken).ConfigureAwait(false))
            return false;

        if (
            await DoesCheckConstraintExistAsync(
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

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {constraintName} CHECK ({expression})
        ";

        await ExecuteAsync(db, sql, transaction: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<DxCheckConstraint?> GetCheckConstraintAsync(
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

        var checkConstraints = await GetCheckConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        return checkConstraints.SingleOrDefault();
    }

    public virtual async Task<string?> GetCheckConstraintNameOnColumnAsync(
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

        var checkConstraints = await GetCheckConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        return checkConstraints
            .FirstOrDefault(c =>
                !string.IsNullOrWhiteSpace(c.ColumnName)
                && c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
            ?.ConstraintName;
    }

    public virtual async Task<List<string>> GetCheckConstraintNamesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var checkConstraints = await GetCheckConstraintsAsync(
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

    public virtual async Task<DxCheckConstraint?> GetCheckConstraintOnColumnAsync(
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

        var checkConstraints = await GetCheckConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        return checkConstraints.FirstOrDefault(c =>
            !string.IsNullOrWhiteSpace(c.ColumnName)
            && c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
        );
    }

    public virtual async Task<List<DxCheckConstraint>> GetCheckConstraintsAsync(
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

        if (!await SupportsCheckConstraintsAsync(db, tx, cancellationToken).ConfigureAwait(false))
            return [];

        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            return [];

        var filter = string.IsNullOrWhiteSpace(constraintNameFilter)
            ? null
            : ToSafeString(constraintNameFilter);

        return string.IsNullOrWhiteSpace(filter)
            ? table.CheckConstraints
            : table
                .CheckConstraints.Where(c => IsWildcardPatternMatch(c.ConstraintName, filter))
                .ToList();
    }

    public virtual async Task<bool> DropCheckConstraintOnColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var constraintName = await GetCheckConstraintNameOnColumnAsync(
            db,
            schemaName,
            tableName,
            columnName,
            tx,
            cancellationToken
        );
        if (string.IsNullOrWhiteSpace(constraintName))
            return false;

        return await DropCheckConstraintIfExistsAsync(
            db,
            schemaName,
            tableName,
            constraintName,
            tx,
            cancellationToken
        );
    }

    public virtual async Task<bool> DropCheckConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required.", nameof(constraintName));

        if (!await SupportsCheckConstraintsAsync(db, tx, cancellationToken).ConfigureAwait(false))
            return false;

        if (
            !await DoesCheckConstraintExistAsync(
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

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                DROP CONSTRAINT {constraintName}
        ";

        await ExecuteAsync(db, sql, transaction: tx).ConfigureAwait(false);

        return true;
    }
}
