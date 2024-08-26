using System.Data;
using System.Data.Common;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> SchemaExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, _, _) = NormalizeNames(schema);

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
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (await SchemaExistsAsync(db, schema, tx, cancellationToken).ConfigureAwait(false))
            return false;

        var (schemaName, _, _) = NormalizeNames(schema);
        await ExecuteAsync(db, $"CREATE SCHEMA IF NOT EXISTS {schemaName}").ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<string>> GetSchemasAsync(
        IDbConnection db,
        string? filter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(filter))
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
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
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
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await SchemaExistsAsync(db, schema, tx, cancellationToken).ConfigureAwait(false))
            return false;

        var (schemaName, _, _) = NormalizeNames(schema);
        await ExecuteAsync(db, $"DROP SCHEMA IF EXISTS {schemaName} CASCADE").ConfigureAwait(false);
        return true;
    }
}
