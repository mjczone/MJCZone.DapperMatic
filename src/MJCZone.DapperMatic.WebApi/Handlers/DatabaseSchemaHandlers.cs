using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic.WebApi.HandlerTypes;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Provides methods to handle database schema-related HTTP requests.
/// </summary>
public static class DatabaseSchemaHandlers
{
    /// <summary>
    /// Adds the database schema handlers to the specified <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add the handlers to.</param>
    public static void AddDatabaseSchemaHandlers(this WebApplication app)
    {
        var options = app
            .Services.GetRequiredService<IOptionsMonitor<DapperMaticOptions>>()
            ?.CurrentValue;

        var prefix = options.GetApiPrefix();

        // add handler to get database schema names
        app.MapGet(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas",
                async (
                    HttpContext context,
                    [FromRoute] string databaseIdOrSlug,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    CancellationToken cancellationToken
                ) =>
                {
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

                    var schemaNames = await connection
                        .GetSchemaNamesAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok(new StringListResponse(schemaNames));
                }
            )
            .WithName("GetSchemaNames")
            .WithDisplayName("Get Schema Names")
            .WithSummary("Get the schema names in the database.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // add handler to create database schema
        app.MapPost(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas",
                async (
                    HttpContext context,
                    [FromRoute] string databaseIdOrSlug,
                    [FromBody] CreateSchemaRequest request,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    CancellationToken cancellationToken
                ) =>
                {
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

                    // validate request
                    if (string.IsNullOrWhiteSpace(request.SchemaName))
                    {
                        return Results.BadRequest("Schema name is required.");
                    }

                    using var connection = await databaseConnectionFactory
                        .OpenConnectionAsync(
                            context.GetTenantIdentifier(),
                            databaseIdOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    var created = await connection
                        .CreateSchemaIfNotExistsAsync(
                            request.SchemaName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);

                    return Results.Created(
                        $"{prefix}/databases/{databaseIdOrSlug}/schemas/{request.SchemaName}",
                        new BoolResponse(created)
                    );
                }
            )
            .WithName("CreateSchema")
            .WithDisplayName("Create Schema")
            .WithSummary("Create a new database schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Accepts<CreateSchemaRequest>("application/json")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // add handler to delete database schema
        app.MapDelete(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}",
                async (
                    HttpContext context,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string schemaName,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    CancellationToken cancellationToken
                ) =>
                {
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

                    var deleted = await connection
                        .DropSchemaIfExistsAsync(schemaName, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok(new BoolResponse(deleted));
                }
            )
            .WithName("DeleteSchema")
            .WithDisplayName("Delete Schema")
            .WithSummary("Delete a database schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
}
