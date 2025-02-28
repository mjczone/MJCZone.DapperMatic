using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic.Models;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Provides methods to handle database view-related HTTP requests.
/// </summary>
public static class DatabaseViewHandlers
{
    /// <summary>
    /// Adds the database view handlers to the specified <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add the handlers to.</param>
    public static void AddDatabaseViewHandlers(this WebApplication app)
    {
        var options = app
            .Services.GetRequiredService<IOptionsMonitor<DapperMaticOptions>>()
            ?.CurrentValue;

        var prefix = options.GetApiPrefix();

        // add handler to get database view names
        app.MapGet(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/views",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromQuery] string? filter = null,
                    [FromQuery] bool expanded = false,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "_")
                    {
                        // force using default schema if not specified
                        schemaName = null;
                    }

                    if (!filter.ValidateFilterExpression())
                    {
                        return Results.BadRequest("The filter expression is invalid.");
                    }

                    var database = await DatabaseHandlers
                        .GetDatabaseAsync(
                            context,
                            databaseIdOrSlug,
                            databaseRegistry,
                            false,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (database is null)
                    {
                        return Results.NotFound();
                    }

                    using var connection = await databaseConnectionFactory
                        .OpenConnectionAsync(
                            context.GetTenantIdentifier(),
                            databaseIdOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (!expanded)
                    {
                        var viewNames = await connection
                            .GetViewNamesAsync(
                                schemaName,
                                filter,
                                cancellationToken: cancellationToken
                            )
                            .ConfigureAwait(false);

                        return Results.Ok(
                            new ViewListResponse(
                                [
                                    .. viewNames.Select(t => new DmView
                                    {
                                        ViewName = t,
                                        Definition = string.Empty,
                                    })
                                ]
                            )
                        );
                    }

                    var views = await connection
                        .GetViewsAsync(schemaName, filter, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok(new ViewListResponse(views));
                }
            )
            .WithName("GetViews")
            .WithDisplayName("Get Views")
            .WithSummary("Get the views in the schema.")
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Produces<ViewListResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        // add handler to get database view definition
        app.MapGet(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/views/{{viewName}}",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromRoute] string viewName,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "_")
                    {
                        // force using default schema if not specified
                        schemaName = null;
                    }

                    var database = await DatabaseHandlers
                        .GetDatabaseAsync(
                            context,
                            databaseIdOrSlug,
                            databaseRegistry,
                            false,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (database is null)
                    {
                        return Results.NotFound();
                    }

                    using var connection = await databaseConnectionFactory
                        .OpenConnectionAsync(
                            context.GetTenantIdentifier(),
                            databaseIdOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    var view = await connection
                        .GetViewAsync(schemaName, viewName, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    if (view is null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(new ViewResponse(view));
                }
            )
            .WithName("GetView")
            .WithDisplayName("Get View")
            .WithSummary("Get the view definition.")
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Produces<ViewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // add handler to create database view
        app.MapPost(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/views",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromBody] CreateViewRequest request,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "_")
                    {
                        // force using default schema if not specified
                        schemaName = null;
                    }

                    if (string.IsNullOrWhiteSpace(request.ViewName))
                    {
                        return Results.BadRequest("The view name is required.");
                    }

                    if (string.IsNullOrWhiteSpace(request.Definition))
                    {
                        return Results.BadRequest("The view definition is required.");
                    }

                    var database = await DatabaseHandlers
                        .GetDatabaseAsync(
                            context,
                            databaseIdOrSlug,
                            databaseRegistry,
                            true,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (database is null)
                    {
                        return Results.NotFound();
                    }

                    using var connection = await databaseConnectionFactory
                        .OpenConnectionAsync(
                            context.GetTenantIdentifier(),
                            databaseIdOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    var newView = new DmView
                    {
                        ViewName = request.ViewName,
                        Definition = request.Definition,
                    };

                    // does the view already exist?
                    var exists = await connection
                        .DoesViewExistAsync(
                            schemaName,
                            newView.ViewName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (exists)
                    {
                        return Results.Conflict(
                            $"View '{newView.ViewName}' already exists in schema '{newView.SchemaName}'."
                        );
                    }

                    var tx = connection.BeginTransaction();
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        var created = await connection
                            .CreateViewIfNotExistsAsync(
                                newView,
                                tx: tx,
                                cancellationToken: cancellationToken
                            )
                            .ConfigureAwait(false);
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return Results.BadRequest(ex.Message);
                    }
                    finally
                    {
                        tx?.Dispose();
                    }
#pragma warning restore CA1031 // Do not catch general exception types

                    var view = await connection
                        .GetViewAsync(
                            schemaName,
                            newView.ViewName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);

                    return view is null
                        ? Results.BadRequest("Failed to create view.")
                        : Results.Created(
                            $"{prefix}/databases/{databaseIdOrSlug}/schemas/{schemaName ?? "_"}/views/{newView.ViewName}",
                            new ViewResponse(view)
                        );
                }
            )
            .WithName("CreateView")
            .WithDisplayName("Create View")
            .WithSummary("Create a new view.")
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Accepts<CreateViewRequest>("application/json")
            .Produces<ViewResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        // add handler to delete database view
        app.MapDelete(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/views/{{viewName}}",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromRoute] string viewName,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "_")
                    {
                        // force using default schema if not specified
                        schemaName = null;
                    }

                    var database = await DatabaseHandlers
                        .GetDatabaseAsync(
                            context,
                            databaseIdOrSlug,
                            databaseRegistry,
                            true,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (database is null)
                    {
                        return Results.NotFound();
                    }

                    using var connection = await databaseConnectionFactory
                        .OpenConnectionAsync(
                            context.GetTenantIdentifier(),
                            databaseIdOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    // does the view exist
                    var exists = await connection
                        .DoesViewExistAsync(
                            schemaName,
                            viewName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (!exists)
                    {
                        return Results.NotFound();
                    }

                    var tx = connection.BeginTransaction();
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        var deleted = await connection
                            .DropViewIfExistsAsync(
                                schemaName,
                                viewName,
                                tx: tx,
                                cancellationToken: cancellationToken
                            )
                            .ConfigureAwait(false);
                        tx.Commit();
                        return Results.Ok(new BoolResponse(deleted));
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return Results.BadRequest(ex.Message);
                    }
                    finally
                    {
                        tx?.Dispose();
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }
            )
            .WithName("DeleteView")
            .WithDisplayName("Delete View")
            .WithSummary("Delete a view.")
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Produces<BoolResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // add handler to update database view
        app.MapPatch(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/views/{{viewName}}",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromRoute] string viewName,
                    [FromBody] UpdateViewRequest request,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "_")
                    {
                        // force using default schema if not specified
                        schemaName = null;
                    }

                    if (string.IsNullOrWhiteSpace(request.Definition))
                    {
                        return Results.BadRequest("The view definition is required.");
                    }

                    var database = await DatabaseHandlers
                        .GetDatabaseAsync(
                            context,
                            databaseIdOrSlug,
                            databaseRegistry,
                            true,
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (database is null)
                    {
                        return Results.NotFound();
                    }

                    using var connection = await databaseConnectionFactory
                        .OpenConnectionAsync(
                            context.GetTenantIdentifier(),
                            databaseIdOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    // does the view exist
                    var exists = await connection
                        .DoesViewExistAsync(
                            schemaName,
                            viewName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (!exists)
                    {
                        return Results.NotFound();
                    }

                    var tx = connection.BeginTransaction();
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(request.Definition))
                        {
                            var updated = await connection
                                .UpdateViewIfExistsAsync(
                                    schemaName,
                                    viewName,
                                    request.Definition,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!updated)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to update view.");
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(request.RenameViewTo))
                        {
                            var renamed = await connection
                                .RenameViewIfExistsAsync(
                                    schemaName,
                                    viewName,
                                    request.RenameViewTo,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!renamed)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to rename view.");
                            }
                        }
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return Results.BadRequest(ex.Message);
                    }
                    finally
                    {
                        tx?.Dispose();
                    }
#pragma warning restore CA1031 // Do not catch general exception types

                    var view = await connection
                        .GetViewAsync(schemaName, viewName, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return view is null
                        ? Results.BadRequest("Failed to update view.")
                        : Results.Ok(new ViewResponse(view));
                }
            )
            .WithName("UpdateView")
            .WithDisplayName("Update View")
            .WithSummary("Update a view.")
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Accepts<UpdateViewRequest>("application/json")
            .Produces<ViewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
}
