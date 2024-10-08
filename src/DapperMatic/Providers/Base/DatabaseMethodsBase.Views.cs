using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseViewMethods
{
    public virtual async Task<bool> DoesViewExistAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await GetViewAsync(db, schemaName, viewName, tx, cancellationToken)
                .ConfigureAwait(false) != null;
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
            throw new ArgumentException(
                "View definition cannot be null or empty.",
                nameof(definition)
            );
        }

        if (
            await DoesViewExistAsync(db, schemaName, viewName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, viewName, _) = NormalizeNames(schemaName, viewName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, viewName);

        await ExecuteAsync(db, $@"CREATE VIEW {schemaQualifiedTableName} AS {definition}", tx: tx)
            .ConfigureAwait(false);

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
            throw new ArgumentException("View name cannot be null or empty.", nameof(viewName));
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
        return (
            await GetViewsAsync(db, schemaName, viewNameFilter, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            .Select(x => x.ViewName)
            .ToList();
    }

    public abstract Task<List<DxView>> GetViewsAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

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

        (schemaName, viewName, _) = NormalizeNames(schemaName, viewName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, viewName);

        await ExecuteAsync(db, $@"DROP VIEW {schemaQualifiedTableName}", tx: tx)
            .ConfigureAwait(false);

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
