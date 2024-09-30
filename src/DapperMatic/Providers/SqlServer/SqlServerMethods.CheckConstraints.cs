using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods
{
    public override async Task<bool> CreateCheckConstraintIfNotExistsAsync(
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

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

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
        {
            return false;
        }

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        var sql =
            @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {constraintName} CHECK ({expression})
        ";

        await ExecuteAsync(db, sql, transaction: tx).ConfigureAwait(false);

        return true;
    }

    public override async Task<bool> DropCheckConstraintIfExistsAsync(
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

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

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
        {
            return false;
        }

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
