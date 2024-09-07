using System.Data;
using System.Data.Common;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> SchemaExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT count(*) as SchemaCount FROM pg_catalog.pg_namespace WHERE nspname = @schemaName",
                    new { schemaName }
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (await SchemaExistsAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false))
            return false;

        schemaName = NormalizeSchemaName(schemaName);
        await ExecuteAsync(db, $"CREATE SCHEMA IF NOT EXISTS {schemaName}").ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<string>> GetSchemasAsync(
        IDbConnection db,
        string? nameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT nspname FROM pg_catalog.pg_namespace ORDER BY nspname"
                )
                .ConfigureAwait(false);
            ;
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT nspname FROM pg_catalog.pg_namespace WHERE nspname LIKE @where ORDER BY nspname",
                    new { where }
                )
                .ConfigureAwait(false);
        }
    }

    public async Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await SchemaExistsAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false))
            return false;

        schemaName = NormalizeSchemaName(schemaName);
        await ExecuteAsync(db, $"DROP SCHEMA IF EXISTS {schemaName} CASCADE").ConfigureAwait(false);
        return true;
    }
}
