using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;

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

        app.MapGet(
                prefix + "/databases/{idOrSlug}/datatypes",
                async (
                    HttpContext httpContext,
                    [FromRoute] string idOrSlug,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var database = await DatabaseHandlers
                        .GetDatabaseAsync(
                            httpContext,
                            idOrSlug,
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
                            httpContext.GetTenantIdentifier(),
                            idOrSlug,
                            cancellationToken
                        )
                        .ConfigureAwait(false);

                    var sqlTypeDescriptors = new List<SqlTypeDescriptor>();

                    var dotnetTypeDescriptors = new List<DotnetTypeDescriptor>
                    {
                        new(typeof(string), 128, isUnicode: false),
                        new(typeof(string), int.MaxValue, isUnicode: false),
                        new(typeof(string), 128, isUnicode: true),
                        new(typeof(string), int.MaxValue, isUnicode: true),
                        new(typeof(char)),
                        new(typeof(char[])),
                        new(typeof(byte)),
                        new(typeof(byte[])),
                        new(typeof(short)),
                        new(typeof(int)),
                        new(typeof(BigInteger)),
                        new(typeof(long)),
                        new(typeof(sbyte)),
                        new(typeof(ushort)),
                        new(typeof(uint)),
                        new(typeof(ulong)),
                        new(typeof(decimal)),
                        new(typeof(float)),
                        new(typeof(double)),
                        new(typeof(Guid)),
                        new(typeof(XDocument)),
                        new(typeof(XElement)),
                        new(typeof(JsonDocument)),
                        new(typeof(JsonElement)),
                        new(typeof(JsonArray)),
                        new(typeof(JsonNode)),
                        new(typeof(JsonObject)),
                        new(typeof(JsonValue)),
                        new(typeof(DateTime)),
                        new(typeof(DateTimeOffset)),
                        new(typeof(TimeSpan)),
                        new(typeof(DateOnly)),
                        new(typeof(TimeOnly)),
                        new(typeof(Dictionary<string, string>)),
                        new(typeof(HashSet<string>)),
                        new(typeof(List<string>)),
                    };

                    var typeMap = connection.GetProviderTypeMap();
                    foreach (var dotnetTypeDescriptor in dotnetTypeDescriptors)
                    {
                        if (
                            typeMap.TryGetProviderSqlTypeMatchingDotnetType(
                                dotnetTypeDescriptor,
                                out var sqlTypeDescriptor
                            ) && sqlTypeDescriptor is not null
                        )
                        {
                            if (
                                sqlTypeDescriptors.Any(d =>
                                    d.BaseTypeName == sqlTypeDescriptor.BaseTypeName
                                    && d.IsAutoIncrementing == sqlTypeDescriptor.IsAutoIncrementing
                                    && d.IsFixedLength == sqlTypeDescriptor.IsFixedLength
                                    && d.IsUnicode == sqlTypeDescriptor.IsUnicode
                                )
                            )
                            {
                                continue;
                            }
                            sqlTypeDescriptors.Add(sqlTypeDescriptor);
                        }
                    }

                    return Results.Ok(
                        new SqlTypeDescriptorListResponse
                        {
                            Results = [.. sqlTypeDescriptors.OrderBy(d => d.BaseTypeName)],
                        }
                    );
                }
            )
            .WithName("GetDatabaseDataTypes")
            .WithDisplayName("Get Database Data Types")
            .WithSummary("Get the data types supported by the database.")
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

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
            .WithTags("DapperMatic/DDL")
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
            .WithTags("DapperMatic/DDL")
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
            .WithTags("DapperMatic/DDL")
            .WithGroupName("DDL")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
}
