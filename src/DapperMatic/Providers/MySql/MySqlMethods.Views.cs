using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override async Task<List<DxView>> GetViewsAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"SELECT 
                    TABLE_NAME AS view_name,
                    VIEW_DEFINITION AS view_definition
                FROM 
                    INFORMATION_SCHEMA.VIEWS
                WHERE 
                    TABLE_SCHEMA = DATABASE()
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_NAME";

        var results = await QueryAsync<(string view_name, string view_definition)>(
                db,
                sql,
                new { schemaName, where },
                tx
            )
            .ConfigureAwait(false);

        return results
            .Select(r =>
            {
                return new DxView(DefaultSchema, r.view_name, r.view_definition);
            })
            .ToList();
    }
}
