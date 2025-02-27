using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Extension methods for setting up DapperMatic operation handlers in an <see cref="WebApplication"/>.
/// </summary>
public static class OperationHandlers
{
    /// <summary>
    /// Adds DapperMatic operation handlers to the <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/>.</param>
    public static void AddOperationHandlers(this WebApplication app)
    {
        var options = app
            .Services.GetRequiredService<IOptionsMonitor<DapperMaticOptions>>()
            ?.CurrentValue;

        var prefix = options.GetApiPrefix();

        app.MapGet(
                prefix + "/operations",
                async (
                    HttpContext httpContext,
                    [FromServices] IOperationsManager operationsManager,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    var operations = await operationsManager
                        .GetOperationsAsync(tenantIdentifier, cancellationToken)
                        .ConfigureAwait(false);

                    var retrieved = FilterOperations(httpContext, operations).ToList();

                    var response = new DatabaseOperationsResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("GetDatabaseOperations")
            .WithDisplayName("Get Database Operations")
            .WithSummary("Retrieves a list of database operations.")
            .WithTags("DapperMatic")
            .WithGroupName("Operations")
            .Produces<DatabaseOperationsResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapGet(
                prefix + "/operations/{idOrSlug}",
                async (
                    HttpContext httpContext,
                    [FromRoute] string idOrSlug,
                    [FromServices] IOperationsManager operationsManager,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    var operation = await operationsManager
                        .GetOperationAsync(tenantIdentifier, idOrSlug, cancellationToken)
                        .ConfigureAwait(false);

                    if (operation is null)
                    {
                        return Results.NotFound();
                    }

                    var retrieved = FilterOperations(httpContext, [operation]).FirstOrDefault();

                    if (retrieved is null)
                    {
                        return Results.NotFound();
                    }

                    var response = new DatabaseOperationResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("GetDatabaseOperation")
            .WithDisplayName("Get Database Operation")
            .WithSummary("Retrieves a specific database operation by its identifier or slug.")
            .WithTags("DapperMatic")
            .WithGroupName("Operations")
            .Produces<DatabaseOperationResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapPost(
                prefix + "/operations",
                async (
                    [FromBody] DatabaseOperationRequest request,
                    [FromServices] IOperationsManager operationsManager,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var operation = request.ToDatabaseOperation();

                    var result = await operationsManager
                        .AddOperationAsync(operation, cancellationToken)
                        .ConfigureAwait(false);

                    if (!result)
                    {
                        return Results.BadRequest();
                    }

                    var response = new DatabaseOperationResponse(operation);
                    return Results.CreatedAtRoute(
                        "GetDatabaseOperation",
                        new { idOrSlug = operation.Id },
                        response
                    );
                }
            )
            .WithName("AddDatabaseOperation")
            .WithDisplayName("Add Database Operation")
            .WithSummary("Adds a new database operation.")
            .WithTags("DapperMatic")
            .WithGroupName("Operations")
            .Accepts<DatabaseOperationRequest>("application/json")
            .Produces<DatabaseOperationResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        app.MapPatch(
                prefix + "/operations/{idOrSlug}",
                async (
                    [FromRoute] string idOrSlug,
                    [FromBody] DatabaseOperationRequest request,
                    [FromServices] IOperationsManager operationsManager,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var operation = request.ToDatabaseOperation();

                    var result = await operationsManager
                        .PatchOperationAsync(operation, cancellationToken)
                        .ConfigureAwait(false);

                    if (!result)
                    {
                        return Results.BadRequest();
                    }

                    var response = new DatabaseOperationResponse(operation);
                    return Results.Ok(response);
                }
            )
            .WithName("PatchDatabaseOperation")
            .WithDisplayName("Patch Database Operation")
            .WithSummary("Updates an existing database operation.")
            .WithTags("DapperMatic")
            .WithGroupName("Operations")
            .Accepts<DatabaseOperationRequest>("application/json")
            .Produces<DatabaseOperationResponse>()
            .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete(
                prefix + "/operations/{idOrSlug}",
                async (
                    HttpContext httpContext,
                    [FromRoute] string idOrSlug,
                    [FromServices] IOperationsManager operationsManager,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    var result = await operationsManager
                        .DeleteOperationAsync(tenantIdentifier, idOrSlug, cancellationToken)
                        .ConfigureAwait(false);

                    if (!result)
                    {
                        return Results.BadRequest();
                    }

                    return Results.NoContent();
                }
            )
            .WithName("DeleteDatabaseOperation")
            .WithDisplayName("Delete Database Operation")
            .WithSummary("Deletes a database operation by its identifier or slug.")
            .WithTags("DapperMatic")
            .WithGroupName("Operations")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost(
                prefix + "/operations/{idOrSlug}/execute",
                async (
                    HttpContext httpContext,
                    [FromBody] DatabaseOperationExecutionRequest request,
                    [FromRoute] string idOrSlug,
                    [FromServices] IOperationsManager operationsManager,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    var operation = await operationsManager
                        .GetOperationAsync(tenantIdentifier, idOrSlug, cancellationToken)
                        .ConfigureAwait(false);

                    if (operation is null)
                    {
                        return Results.NotFound();
                    }

                    var retrieved = FilterOperations(httpContext, [operation]).FirstOrDefault();

                    if (retrieved is null)
                    {
                        return Results.NotFound();
                    }

                    var results = await operationsManager
                        .ExecuteOperationAsync(
                            tenantIdentifier,
                            idOrSlug,
                            request.Parameters,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    var response = new DatabaseOperationExecutionResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("ExecuteDatabaseOperation")
            .WithDisplayName("Execute Database Operation")
            .WithSummary("Executes a database operation and returns the result.")
            .WithTags("DapperMatic")
            .WithGroupName("Operations")
            .Accepts<DatabaseOperationExecutionRequest>("application/json")
            .Produces<DatabaseOperationResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static List<DatabaseOperation> FilterOperations(
        HttpContext httpContext,
        IEnumerable<DatabaseOperation> operations
    )
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var user = isAuthenticated ? httpContext.User : null;

        // we only return databases that the user has access to
        var filtered = operations.Where(d =>
            // if there are no roles defined, anybody can see these operations
            // if there are roles defined, only users in those roles can see these operations
            (
                (d.ManagementRoles == null || d.ManagementRoles.Count == 0)
                && (d.ExecutionRoles == null || d.ExecutionRoles.Count == 0)
            )
            || (d.ManagementRoles ?? []).Any(r => isAuthenticated && user!.IsInRole(r))
            || (d.ExecutionRoles ?? []).Any(r => isAuthenticated && user!.IsInRole(r))
        );
        return [.. filtered];
    }
}
