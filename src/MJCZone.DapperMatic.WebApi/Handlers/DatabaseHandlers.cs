using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Provides methods to handle database-related HTTP requests.
/// </summary>
public static class DatabaseHandlers
{
    /// <summary>
    /// Adds the database handlers to the specified <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add the handlers to.</param>
    /// <remarks>
    /// This method maps the following endpoints:
    /// <list type="bullet">
    /// <item>
    /// <description><c>GET /api/databases</c> - Retrieves a list of databases.</description>
    /// </item>
    /// <item>
    /// <description><c>GET /api/databases/{id}</c> - Retrieves a specific database by ID.</description>
    /// </item>
    /// <item>
    /// <description><c>POST /api/databases</c> - Adds a new database.</description>
    /// </item>
    /// <item>
    /// <description><c>PUT /api/databases/{id}</c> - Updates an existing database by ID.</description>
    /// </item>
    /// <item>
    /// <description><c>DELETE /api/databases/{id}</c> - Deletes a specific database by ID.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static void AddDatabaseHandlers(this WebApplication app)
    {
        var options = app
            .Services.GetRequiredService<IOptionsMonitor<DapperMaticOptions>>()
            ?.CurrentValue;

        var prefix = options.GetApiPrefix();

        app.MapGet(
                prefix + "/databases",
                async (
                    HttpContext httpContext,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();
                    var databases = await databaseRegistry
                        .GetDatabasesAsync(tenantIdentifier, cancellationToken)
                        .ConfigureAwait(false);

                    var retrieved = FilterDatabases(httpContext, databases).ToList();

                    var response = new DatabasesResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("GetDatabases")
            .WithDisplayName("Get Databases")
            .WithSummary("Retrieves a list of databases.")
            .WithTags("DapperMatic/Databases")
            .WithGroupName("Databases")
            .Produces<DatabasesResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapGet(
                prefix + "/databases/{idOrSlug}",
                async (
                    HttpContext httpContext,
                    [FromRoute] string idOrSlug,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    DatabaseEntry? retrieved = await GetDatabaseAsync(
                            httpContext,
                            idOrSlug,
                            databaseRegistry,
                            false,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (retrieved == null)
                    {
                        return Results.NotFound();
                    }

                    var response = new DatabaseResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("GetDatabase")
            .WithDisplayName("Get Database")
            .WithSummary("Retrieves a specific database by ID or Slug.")
            .WithTags("DapperMatic/Databases")
            .WithGroupName("Databases")
            .Produces<DatabaseResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapPost(
                prefix + "/databases",
                async (
                    HttpContext httpContext,
                    [FromBody] DatabaseEntry database,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    database.TenantIdentifier = tenantIdentifier;
                    database.CreatedDate = DateTime.UtcNow;
                    database.CreatedBy = httpContext.User.Identity?.Name ?? "Anonymous";
                    database.ModifiedDate = DateTime.UtcNow;
                    database.ModifiedBy = httpContext.User.Identity?.Name ?? "Anonymous";

                    var added = await databaseRegistry
                        .AddDatabaseAsync(database, cancellationToken)
                        .ConfigureAwait(false);

                    var retrieved = await GetEligibleDatabaseAsync(
                            httpContext,
                            databaseRegistry,
                            tenantIdentifier,
                            added.Id.ToString(),
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (retrieved == null)
                    {
                        return Results.NotFound();
                    }

                    var response = new DatabaseResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("AddDatabase")
            .WithDisplayName("Add Database")
            .WithSummary("Adds a new database.")
            .WithTags("DapperMatic/Databases")
            .WithGroupName("Databases")
            .Produces<DatabaseResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapPut(
                prefix + "/databases/{idOrSlug}",
                async (
                    HttpContext httpContext,
                    [FromBody] DatabaseEntry database,
                    [FromRoute] string idOrSlug,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    var existing = await GetEligibleDatabaseAsync(
                            httpContext,
                            databaseRegistry,
                            tenantIdentifier,
                            idOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (existing == null)
                    {
                        return Results.NotFound();
                    }

                    database.Id = existing.Id;
                    database.CreatedDate = existing.CreatedDate;
                    database.CreatedBy = existing.CreatedBy;
                    database.ModifiedDate = DateTime.UtcNow;
                    database.ModifiedBy = httpContext.User.Identity?.Name ?? "Anonymous";

                    var updated = await databaseRegistry
                        .PatchDatabaseAsync(database, cancellationToken)
                        .ConfigureAwait(false);

                    var retrieved = await GetEligibleDatabaseAsync(
                            httpContext,
                            databaseRegistry,
                            tenantIdentifier,
                            updated.Id.ToString(),
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (retrieved == null)
                    {
                        return Results.NotFound();
                    }

                    var response = new DatabaseResponse(retrieved);
                    return Results.Ok(response);
                }
            )
            .WithName("UpdateDatabase")
            .WithDisplayName("Update Database")
            .WithSummary("Updates an existing database by ID or Slug.")
            .WithTags("DapperMatic/Databases")
            .WithGroupName("Databases")
            .Produces<DatabaseResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapDelete(
                prefix + "/databases/{idOrSlug}",
                async (
                    HttpContext httpContext,
                    [FromRoute] string idOrSlug,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var tenantIdentifier = httpContext.GetTenantIdentifier();

                    var existing = await GetEligibleDatabaseAsync(
                            httpContext,
                            databaseRegistry,
                            tenantIdentifier,
                            idOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    if (existing == null)
                    {
                        return Results.NotFound();
                    }

                    var deleted = await databaseRegistry
                        .DeleteDatabaseAsync(tenantIdentifier, idOrSlug, cancellationToken)
                        .ConfigureAwait(false);

                    return deleted ? Results.Ok() : Results.NotFound();
                }
            )
            .WithName("DeleteDatabase")
            .WithDisplayName("Delete Database")
            .WithSummary("Deletes a specific database by ID or Slug.")
            .WithTags("DapperMatic/Databases")
            .WithGroupName("Databases")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    /// <summary>
    /// Asynchronously retrieves a specific database by its identifier or slug.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
    /// <param name="idOrSlug">The unique identifier or slug of the database to retrieve.</param>
    /// <param name="databaseRegistry">The <see cref="IDatabaseRegistry"/>.</param>
    /// <param name="requireManagementRights">A value indicating whether management rights are required to access the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="DatabaseEntry"/>.</returns>
    internal static async Task<DatabaseEntry?> GetDatabaseAsync(
        HttpContext httpContext,
        string idOrSlug,
        IDatabaseRegistry databaseRegistry,
        bool requireManagementRights = false,
        CancellationToken cancellationToken = default
    )
    {
        var tenantIdentifier = httpContext.GetTenantIdentifier();
        var retrieved = await GetEligibleDatabaseAsync(
                httpContext,
                databaseRegistry,
                tenantIdentifier,
                idOrSlug,
                cancellationToken
            )
            .ConfigureAwait(false);
        return retrieved;
    }

    private static List<DatabaseEntry> FilterDatabases(
        HttpContext httpContext,
        IEnumerable<DatabaseEntry> databases,
        bool requireManagementRights = false
    )
    {
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated == true;
        var user = isAuthenticated ? httpContext.User : null;

        if (requireManagementRights)
        {
            if (!isAuthenticated)
            {
                return [];
            }

            // we only return databases that the user has access to
            var filtered = databases.Where(d =>
                (d.ManagementRoles ?? []).Any(r => user!.IsInRole(r))
            );
            return [.. filtered];
        }
        else
        {
            // we only return databases that the user has access to
            var filtered = databases.Where(d =>
                // if there are no roles defined, anybody can see these databases
                // if there are roles defined, only users in those roles can see these databases
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

    private static async Task<DatabaseEntry?> GetEligibleDatabaseAsync(
        HttpContext httpContext,
        IDatabaseRegistry databaseRegistry,
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken
    )
    {
        var retrieved = await databaseRegistry
            .GetDatabaseAsync(tenantIdentifier, idOrSlug, cancellationToken)
            .ConfigureAwait(false);

        return retrieved == null
            ? null
            : FilterDatabases(httpContext, [retrieved]).FirstOrDefault();
    }
}
