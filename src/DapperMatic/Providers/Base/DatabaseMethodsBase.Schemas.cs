using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseSchemaMethods
{
    protected abstract string DefaultSchema { get; }
    public virtual bool SupportsSchemas => !string.IsNullOrWhiteSpace(DefaultSchema);

    protected virtual string GetSchemaQualifiedTableName(string schemaName, string tableName)
    {
        return SupportsSchemas && string.IsNullOrWhiteSpace(schemaName)
            ? $"{schemaName.ToQuotedIdentifier(QuoteChars)}.{tableName.ToQuotedIdentifier(QuoteChars)}"
            : tableName.ToQuotedIdentifier(QuoteChars);
    }

    public virtual async Task<bool> DoesSchemaExistAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name is required.", nameof(schemaName));

        return (
                await GetSchemaNamesAsync(db, schemaName, tx, cancellationToken)
                    .ConfigureAwait(false)
            ).Count() > 0;
    }

    public virtual async Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name is required.", nameof(schemaName));

        if (await DoesSchemaExistAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false))
            return false;

        schemaName = NormalizeSchemaName(schemaName);

        var sql = $"CREATE SCHEMA {schemaName}";

        await ExecuteAsync(db, sql, transaction: tx).ConfigureAwait(false);

        return true;
    }

    public abstract Task<IEnumerable<string>> GetSchemaNamesAsync(
        IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    public virtual async Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name is required.", nameof(schemaName));

        if (
            !await DoesSchemaExistAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false)
        )
            return false;

        schemaName = NormalizeSchemaName(schemaName);

        var sql = $"DROP SCHEMA {schemaName}";

        await ExecuteAsync(db, sql, transaction: tx).ConfigureAwait(false);

        return true;
    }
}
