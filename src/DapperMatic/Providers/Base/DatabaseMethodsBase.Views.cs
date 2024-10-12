using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase
{
    public virtual async Task<bool> DoesViewExistAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
                await GetViewNamesAsync(db, schemaName, viewName, tx, cancellationToken)
                    .ConfigureAwait(false)
            ).Count == 1;
    }

    public virtual async Task<bool> CreateViewIfNotExistsAsync(
        IDbConnection db,
        DxView view,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateViewIfNotExistsAsync(
                db,
                view.SchemaName,
                view.ViewName,
                view.Definition,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<bool> CreateViewIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        string definition,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(definition))
        {
            throw new ArgumentException("View definition is required.", nameof(definition));
        }

        if (
            await DoesViewExistAsync(db, schemaName, viewName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var sql = SqlCreateView(schemaName, viewName, definition);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<DxView?> GetViewAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(viewName))
        {
            throw new ArgumentException("View name is required.", nameof(viewName));
        }

        return (
            await GetViewsAsync(db, schemaName, viewName, tx, cancellationToken)
                .ConfigureAwait(false)
        ).SingleOrDefault();
    }

    public virtual async Task<List<string>> GetViewNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (sql, parameters) = SqlGetViewNames(schemaName, viewNameFilter);
        return await QueryAsync<string>(db, sql, parameters, tx: tx).ConfigureAwait(false);
    }

    public virtual async Task<List<DxView>> GetViewsAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (sql, parameters) = SqlGetViews(schemaName, viewNameFilter);
        var views = await QueryAsync<DxView>(db, sql, parameters, tx: tx).ConfigureAwait(false);
        foreach (var view in views)
        {
            view.Definition = NormalizeViewDefinition(view.Definition);
        }
        return views;
    }

    public virtual async Task<bool> DropViewIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await DoesViewExistAsync(db, schemaName, viewName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var sql = SqlDropView(schemaName, viewName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> RenameViewIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        string newViewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var view = await GetViewAsync(db, schemaName, viewName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (view == null || string.IsNullOrWhiteSpace(view.Definition))
            return false;

        await DropViewIfExistsAsync(db, schemaName, viewName, tx, cancellationToken)
            .ConfigureAwait(false);

        await CreateViewIfNotExistsAsync(
            db,
            schemaName,
            newViewName,
            view.Definition,
            tx,
            cancellationToken
        );

        return true;
    }
}
