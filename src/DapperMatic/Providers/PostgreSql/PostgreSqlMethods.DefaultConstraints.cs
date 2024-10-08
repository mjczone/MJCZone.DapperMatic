using System.Data;
using System.Transactions;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    public override async Task<bool> CreateDefaultConstraintIfNotExistsAsync(
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

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        columnName = NormalizeName(columnName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                ALTER COLUMN {columnName} SET DEFAULT {expression}
        ";

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public override async Task<bool> DropDefaultConstraintOnColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name is required.", nameof(columnName));

        var defaultConstraint = await GetDefaultConstraintOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (defaultConstraint == null)
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        // in postgresql, default constraints are not named, so we can't drop them by name
        // we can just assume the column has a default value and we'll set it to null

        await ExecuteAsync(
            db,
            $"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ALTER COLUMN {columnName} DROP DEFAULT",
            tx: tx
        );

        return true;
    }

    public override async Task<bool> DropDefaultConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // in postgresql, default constraints are not named, so we can't drop them by name
        // so we do the reverse, we drop the default value on the column after we find a match based on the constraint name devised in DapperMatic

        // let's make an assumption that the constraint name contains the column name
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required.", nameof(constraintName));

        var defaultConstraints = await GetDefaultConstraintsAsync(
                db,
                schemaName,
                tableName,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        var defaultConstraint = defaultConstraints.FirstOrDefault(c =>
            constraintName.Contains(c.ConstraintName, StringComparison.OrdinalIgnoreCase)
        );

        if (defaultConstraint == null)
            return false;

        var columnName = defaultConstraint.ColumnName;

        // var columnNames = await GetColumnNamesAsync(
        //         db,
        //         schemaName,
        //         tableName,
        //         null,
        //         tx: tx,
        //         cancellationToken: cancellationToken
        //     )
        //     .ConfigureAwait(false);

        // // find the matching column per the constraint name
        // var columnName = columnNames.FirstOrDefault(c =>
        //     constraintName.Contains(c, StringComparison.OrdinalIgnoreCase)
        // );

        if (string.IsNullOrWhiteSpace(columnName))
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        // in postgresql, default constraints are not named, so we can't drop them by name
        // we can just assume the column has a default value and we'll set it to null

        await ExecuteAsync(
            db,
            $"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ALTER COLUMN {columnName} DROP DEFAULT",
            tx: tx
        );

        return true;
    }
}
