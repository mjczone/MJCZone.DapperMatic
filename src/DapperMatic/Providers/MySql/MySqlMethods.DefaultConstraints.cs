using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
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

        var defaultExpression = expression.Trim();
        var addParentheses =
            defaultExpression.Contains(' ')
            && !(defaultExpression.StartsWith("(") && defaultExpression.EndsWith(")"))
            && !(defaultExpression.StartsWith("\"") && defaultExpression.EndsWith("\""))
            && !(defaultExpression.StartsWith("'") && defaultExpression.EndsWith("'"));

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                ALTER COLUMN {columnName} SET DEFAULT {(addParentheses ? $"({defaultExpression})" : defaultExpression)}
        ";

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

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
        var defaultConstraint = await GetDefaultConstraintAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (defaultConstraint == null || string.IsNullOrWhiteSpace(defaultConstraint.ColumnName))
            return false;

        return await DropDefaultConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                defaultConstraint.ColumnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
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

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                ALTER COLUMN {columnName} DROP DEFAULT
        ";

        await ExecuteAsync(db, sql, null, tx: tx).ConfigureAwait(false);

        return true;
    }
}
