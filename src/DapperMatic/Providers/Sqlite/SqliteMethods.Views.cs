using System.Data;
using System.Data.Common;
using System.Text;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    public override async Task<bool> CreateViewIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        string definition,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await DoesViewExistAsync(db, schemaName, viewName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (_, viewName, _) = NormalizeNames(schemaName, viewName, null);

        var sql = new StringBuilder();
        sql.AppendLine($"CREATE VIEW {viewName} AS");
        sql.AppendLine(definition);

        await ExecuteAsync(db, sql.ToString(), transaction: tx).ConfigureAwait(false);

        return true;
    }

    public override async Task<bool> DoesViewExistAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, viewName, _) = NormalizeNames(schemaName, viewName, null);

        return await ExecuteScalarAsync<int>(
                    db,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'view' AND name = @viewName",
                    new { viewName },
                    transaction: tx
                )
                .ConfigureAwait(false) > 0;
    }

    public override async Task<List<string>> GetViewNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter)
            ? null
            : $"{ToAlphaNumericString(viewNameFilter)}".Replace("*", "%");

        var sql = new StringBuilder();
        sql.AppendLine(
            @"SELECT name
                FROM sqlite_master
                WHERE TYPE = 'view' AND name NOT LIKE 'sqlite_%'"
        );
        if (!string.IsNullOrWhiteSpace(where))
            sql.AppendLine(" AND name LIKE @where");
        sql.AppendLine("ORDER BY name");

        return await QueryAsync<string>(db, sql.ToString(), new { where }, transaction: tx)
            .ConfigureAwait(false);
    }

    public override async Task<List<DxView>> GetViewsAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter)
            ? null
            : $"{ToAlphaNumericString(viewNameFilter)}".Replace("*", "%");

        var sql = new StringBuilder();
        sql.AppendLine(
            @"SELECT m.name AS view_name, m.SQL AS view_sql
                FROM sqlite_master AS m
                WHERE m.TYPE = 'view' AND name NOT LIKE 'sqlite_%'"
        );
        if (!string.IsNullOrWhiteSpace(where))
            sql.AppendLine(" AND m.name LIKE @where");
        sql.AppendLine("ORDER BY m.name");

        var results = await QueryAsync<(string view_name, string view_sql)>(
                db,
                sql.ToString(),
                new { where },
                transaction: tx
            )
            .ConfigureAwait(false);

        var views = new List<DxView>();
        foreach (var result in results)
        {
            var viewName = result.view_name;
            var viewSql = result.view_sql;

            // split the view by the first AS keyword surrounded by whitespace
            string? viewDefinition = null;
            var whiteSpaceCharacters = new[] { ' ', '\t', '\n', '\r' };
            for (var i = 0; i < viewSql.Length; i++)
            {
                if (
                    i > 0
                    && viewSql[i] == 'A'
                    && viewSql[i + 1] == 'S'
                    && whiteSpaceCharacters.Contains(viewSql[i - 1])
                    && whiteSpaceCharacters.Contains(viewSql[i + 2])
                )
                {
                    viewDefinition = viewSql[(i + 3)..].Trim();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(viewDefinition))
            {
                Logger?.LogWarning(
                    "Could not parse view definition for view {viewName}: {sql}",
                    viewName,
                    viewSql
                );
                continue;
            }
            views.Add(new DxView(null, viewName, viewDefinition));
        }
        return views;
    }
}
