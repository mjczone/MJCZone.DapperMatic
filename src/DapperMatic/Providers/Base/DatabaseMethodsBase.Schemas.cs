using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase : IDatabaseSchemaMethods
{
    public virtual async Task<bool> DoesSchemaExistAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!SupportsSchemas)
            return false;

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
        if (!SupportsSchemas)
            return false;

        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name is required.", nameof(schemaName));

        if (await DoesSchemaExistAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false))
            return false;

        var sql = SqlCreateSchema(schemaName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<List<string>> GetSchemaNamesAsync(
        IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    ) {
        if (!SupportsSchemas)
            return [];

        var (sql, parameters) = SqlGetSchemaNames(schemaNameFilter);

        return await QueryAsync<string>(db, sql, parameters, tx: tx).ConfigureAwait(false);
    }

    public virtual async Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!SupportsSchemas)
            return false;

        if (string.IsNullOrWhiteSpace(schemaName))
            throw new ArgumentException("Schema name is required.", nameof(schemaName));

        if (
            !await DoesSchemaExistAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false)
        )
            return false;

        var sql = SqlDropSchema(schemaName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }
}
