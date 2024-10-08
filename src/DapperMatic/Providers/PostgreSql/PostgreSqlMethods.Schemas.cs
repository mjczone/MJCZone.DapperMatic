using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    private static string _defaultSchema = "public";

    public static void SetDefaultSchema(string schema)
    {
        _defaultSchema = schema;
    }

    protected override string DefaultSchema => _defaultSchema;

    public override async Task<IEnumerable<string>> GetSchemaNamesAsync(
        IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var where = string.IsNullOrWhiteSpace(schemaNameFilter)
            ? ""
            : ToLikeString(schemaNameFilter);

        var sql =
            $@"
            SELECT DISTINCT nspname
            FROM pg_catalog.pg_namespace
            {(string.IsNullOrWhiteSpace(where) ? "" : $"WHERE lower(nspname) LIKE @where")}
            ORDER BY nspname";

        return await QueryAsync<string>(db, sql, new { where }, tx: tx).ConfigureAwait(false);
    }

    public override async Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await DoesSchemaExistAsync(db, schemaName, tx, cancellationToken).ConfigureAwait(false)
        )
            return false;

        schemaName = NormalizeSchemaName(schemaName);

        await ExecuteAsync(db, $"DROP SCHEMA IF EXISTS {schemaName} CASCADE").ConfigureAwait(false);

        return true;
    }
}
