using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseUniqueConstraintMethods
{
    public virtual async Task<bool> DoesUniqueConstraintExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetUniqueConstraintAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> DoesUniqueConstraintExistOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetUniqueConstraintOnColumnAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false) != null;
    }

    public virtual async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        IDbConnection db,
        DxUniqueConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateUniqueConstraintIfNotExistsAsync(
                db,
                constraint.SchemaName,
                constraint.TableName,
                constraint.ConstraintName,
                constraint.Columns,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
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
            await DoesUniqueConstraintExistAsync(
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

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);
        var supportsOrderedKeysInConstraints = await SupportsOrderedKeysInConstraintsAsync(
                db,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {constraintName} 
                    UNIQUE ({string.Join(", ", columns.Select(c => c.ToString(supportsOrderedKeysInConstraints)))})
        ";

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<DxUniqueConstraint?> GetUniqueConstraintAsync(
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

        var uniqueConstraints = await GetUniqueConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return uniqueConstraints.SingleOrDefault();
    }

    public virtual async Task<string?> GetUniqueConstraintNameOnColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await GetUniqueConstraintOnColumnAsync(
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

    public virtual async Task<List<string>> GetUniqueConstraintNamesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var uniqueConstraints = await GetUniqueConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return uniqueConstraints.Select(c => c.ConstraintName).ToList();
    }

    public virtual async Task<DxUniqueConstraint?> GetUniqueConstraintOnColumnAsync(
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

        var uniqueConstraints = await GetUniqueConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return uniqueConstraints.FirstOrDefault(c =>
            c.Columns.Any(sc =>
                sc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    public virtual async Task<List<DxUniqueConstraint>> GetUniqueConstraintsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
        if (table == null)
            return new List<DxUniqueConstraint>();

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var filter = string.IsNullOrWhiteSpace(constraintNameFilter)
            ? null
            : ToSafeString(constraintNameFilter);

        return string.IsNullOrWhiteSpace(filter)
            ? table.UniqueConstraints
            : table
                .UniqueConstraints.Where(c => IsWildcardPatternMatch(c.ConstraintName, filter))
                .ToList();
    }

    public virtual async Task<bool> DropUniqueConstraintIfExistsAsync(
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
                await DoesUniqueConstraintExistAsync(
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

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} 
                    DROP CONSTRAINT {constraintName}",
                tx: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> DropUniqueConstraintOnColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var constraintName = await GetUniqueConstraintNameOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return constraintName != null
            && await DropUniqueConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
    }
}
