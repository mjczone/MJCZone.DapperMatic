using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    public override async Task<List<DxView>> GetViewsAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"
            select 
                v.schemaname as schema_name,
                v.viewname as view_name,
                v.definition as view_definition
            from pg_views as v
            where 
                v.schemaname not like 'pg_%' and v.schemaname != 'information_schema'             
                and lower(v.schemaname) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? "" : " AND lower(v.viewname) LIKE @where")}
            order by schema_name, view_name";

        var results = await QueryAsync<(
            string schema_name,
            string view_name,
            string view_definition
        )>(db, sql, new { schemaName, where }, tx)
            .ConfigureAwait(false);

        // view definitions in Postgres don't store the AS keyword, just the SELECT statement
        return results
            .Select(r =>
            {
                return new DxView(r.schema_name, r.view_name, r.view_definition);
            })
            .ToList();
    }
}
