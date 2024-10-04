using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods
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
            SELECT
                SCHEMA_NAME(v.schema_id) AS schema_name,
                v.[name] AS view_name,
                m.definition AS view_definition
            FROM sys.objects v
                INNER JOIN sys.sql_modules m ON v.object_id = m.object_id
            WHERE
                v.[type] = 'V'
                AND v.is_ms_shipped = 0                
                AND SCHEMA_NAME(v.schema_id) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? "" : " AND v.[name] LIKE @where")}
            ORDER BY
                SCHEMA_NAME(v.schema_id),
                v.name";

        var results = await QueryAsync<(
            string schema_name,
            string view_name,
            string view_definition
        )>(db, sql, new { schemaName, where }, tx)
            .ConfigureAwait(false);

        var whiteSpaceCharacters = new char[] { ' ', '\t', '\n', '\r' };
        return results
            .Select(r =>
            {
                // strip off the CREATE VIEW statement ending with the AS
                var indexOfAs = -1;
                for (var i = 0; i < r.view_definition.Length; i++)
                {
                    if (i == 0)
                        continue;
                    if (i == r.view_definition.Length - 2)
                        break;

                    if (
                        whiteSpaceCharacters.Contains(r.view_definition[i - 1])
                        && char.ToUpperInvariant(r.view_definition[i]) == 'A'
                        && char.ToUpperInvariant(r.view_definition[i + 1]) == 'S'
                        && whiteSpaceCharacters.Contains(r.view_definition[i + 2])
                    )
                    {
                        indexOfAs = i;
                        break;
                    }
                }
                if (indexOfAs == -1)
                    throw new Exception("Could not find AS in view definition");

                var definition = r.view_definition[(indexOfAs + 3)..].Trim();

                return new DxView(r.schema_name, r.view_name, definition);
            })
            .ToList();
    }
}
