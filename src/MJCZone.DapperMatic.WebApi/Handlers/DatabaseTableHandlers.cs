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
/// Provides methods to handle database table-related HTTP requests.
/// </summary>
public static class DatabaseTableHandlers
{
    /// <summary>
    /// Adds the database table handlers to the specified <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add the handlers to.</param>
    public static void AddDatabaseTableHandlers(this WebApplication app)
    {
        var options = app
            .Services.GetRequiredService<IOptionsMonitor<DapperMaticOptions>>()
            ?.CurrentValue;

        var prefix = options.GetApiPrefix();

        app.MapGet(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/tables",
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
                        var tableNames = await connection
                            .GetTableNamesAsync(
                                schemaName,
                                filter,
                                cancellationToken: cancellationToken
                            )
                            .ConfigureAwait(false);

                        return Results.Ok(
                            new TableListResponse(
                                [.. tableNames.Select(t => new DmTable { TableName = t })]
                            )
                        );
                    }

                    var tables = await connection
                        .GetTablesAsync(schemaName, filter, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok(new TableListResponse(tables));
                }
            )
            .WithName("GetTables")
            .WithDisplayName("Get Tables")
            .WithSummary("Get the tables in the specified schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Produces<TableListResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        app.MapGet(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/tables/{{tableName}}",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromRoute] string tableName,
                    CancellationToken cancellationToken
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

                    var table = await connection
                        .GetTableAsync(schemaName, tableName, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    if (table is null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(new TableResponse(table));
                }
            )
            .WithName("GetTable")
            .WithDisplayName("Get Table")
            .WithSummary("Get the specified table in the specified schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Produces<TableResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapPost(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/tables",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromBody] CreateTableRequest request,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(schemaName) || schemaName == "_")
                    {
                        // force using default schema if not specified
                        schemaName = null;
                    }

                    if (string.IsNullOrWhiteSpace(request.TableName))
                    {
                        return Results.BadRequest("Table name is required.");
                    }

                    var newTable = new DmTable
                    {
                        SchemaName = schemaName,
                        TableName = request.TableName,
                    };
                    if (request.Columns is not null && request.Columns.Count > 0)
                    {
                        newTable.Columns = request.Columns;
                    }
                    else
                    {
                        // return Results.BadRequest("At least one column is required.");

                        // let's just create the table with a singular column 'id' of type 'Guid'
                        newTable.Columns =
                        [
                            new DmColumn(
                                newTable.SchemaName,
                                newTable.TableName,
                                columnName: "id",
                                dotnetType: typeof(Guid),
                                isNullable: false,
                                isPrimaryKey: true,
                                isUnique: true
                            ),
                        ];
                    }

                    if (request.PrimaryKeyConstraint is not null)
                    {
                        newTable.PrimaryKeyConstraint = request.PrimaryKeyConstraint;
                    }

                    if (request.CheckConstraints is not null && request.CheckConstraints.Count > 0)
                    {
                        newTable.CheckConstraints = request.CheckConstraints;
                    }

                    if (
                        request.DefaultConstraints is not null
                        && request.DefaultConstraints.Count > 0
                    )
                    {
                        newTable.DefaultConstraints = request.DefaultConstraints;
                    }

                    if (
                        request.UniqueConstraints is not null
                        && request.UniqueConstraints.Count > 0
                    )
                    {
                        newTable.UniqueConstraints = request.UniqueConstraints;
                    }

                    if (
                        request.ForeignKeyConstraints is not null
                        && request.ForeignKeyConstraints.Count > 0
                    )
                    {
                        newTable.ForeignKeyConstraints = request.ForeignKeyConstraints;
                    }

                    if (request.Indexes is not null && request.Indexes.Count > 0)
                    {
                        newTable.Indexes = request.Indexes;
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

                    // does the table already exist?
                    var exists = await connection
                        .DoesTableExistAsync(
                            newTable.SchemaName,
                            newTable.TableName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);
                    if (exists)
                    {
                        return Results.Conflict(
                            $"Table '{newTable.TableName}' already exists in schema '{newTable.SchemaName}'."
                        );
                    }

                    var tx = connection.BeginTransaction();
#pragma warning disable CA1031 // Do not catch general exception types
                    try
                    {
                        var created = await connection
                            .CreateTableIfNotExistsAsync(
                                newTable,
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

                    var table = await connection
                        .GetTableAsync(
                            newTable.SchemaName,
                            newTable.TableName,
                            cancellationToken: cancellationToken
                        )
                        .ConfigureAwait(false);

                    return table is null
                        ? Results.BadRequest("Failed to create table.")
                        : Results.Created(
                            $"{prefix}/databases/{databaseIdOrSlug}/schemas/{schemaName ?? "_"}/tables/{table!.TableName}",
                            new TableResponse(table)
                        );
                }
            )
            .WithName("CreateTable")
            .WithDisplayName("Create Table")
            .WithSummary("Create a new table in the specified schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Accepts<CreateTableRequest>("application/json")
            .Produces<TableResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        app.MapDelete(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/tables/{{tableName}}",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromRoute] string tableName,
                    CancellationToken cancellationToken
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

                    // does table exist
                    var exists = await connection
                        .DoesTableExistAsync(
                            schemaName,
                            tableName,
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
                            .DropTableIfExistsAsync(
                                schemaName,
                                tableName,
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
            .WithName("DeleteTable")
            .WithDisplayName("Delete Table")
            .WithSummary("Delete the specified table in the specified schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Produces<BoolResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapPatch(
                $"{prefix}/databases/{{databaseIdOrSlug}}/schemas/{{schemaName}}/tables/{{tableName}}",
                async (
                    HttpContext context,
                    [FromServices] IDatabaseRegistry databaseRegistry,
                    [FromServices] IDatabaseConnectionFactory databaseConnectionFactory,
                    [FromRoute] string databaseIdOrSlug,
                    [FromRoute] string? schemaName,
                    [FromRoute] string tableName,
                    [FromBody] UpdateTableRequest request,
                    CancellationToken cancellationToken
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

                    // does table exist
                    var exists = await connection
                        .DoesTableExistAsync(
                            schemaName,
                            tableName,
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
                        // drop columns if requested
                        foreach (var column in request.DropColumns ?? [])
                        {
                            var dropped = await connection
                                .DropColumnIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    column,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop column.");
                            }
                        }

                        // drop check constraints if requested
                        foreach (var constraint in request.DropCheckConstraints ?? [])
                        {
                            var dropped = await connection
                                .DropCheckConstraintIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop check constraint.");
                            }
                        }

                        // drop default constraints if requested
                        foreach (var constraint in request.DropDefaultConstraints ?? [])
                        {
                            var dropped = await connection
                                .DropDefaultConstraintIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop default constraint.");
                            }
                        }

                        // drop unique constraints if requested
                        foreach (var constraint in request.DropUniqueConstraints ?? [])
                        {
                            var dropped = await connection
                                .DropUniqueConstraintIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop unique constraint.");
                            }
                        }

                        // drop foreign key constraints if requested
                        foreach (var constraint in request.DropForeignKeyConstraints ?? [])
                        {
                            var dropped = await connection
                                .DropForeignKeyConstraintIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop foreign key constraint.");
                            }
                        }

                        // drop indexes if requested
                        foreach (var index in request.DropIndexes ?? [])
                        {
                            var dropped = await connection
                                .DropIndexIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    index,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop index.");
                            }
                        }

                        // drop primary key constraint if requested
                        if (request.DropPrimaryKeyConstraint ?? false)
                        {
                            var dropped = await connection
                                .DropPrimaryKeyConstraintIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!dropped)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to drop primary key constraint.");
                            }
                        }

                        // add columns
                        foreach (var column in request.AddColumns ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(column.SchemaName))
                            {
                                column.SchemaName = schemaName;
                            }
                            if (string.IsNullOrWhiteSpace(column.TableName))
                            {
                                column.TableName = tableName;
                            }
                            var added = await connection
                                .CreateColumnIfNotExistsAsync(
                                    column,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!added)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to add column.");
                            }
                        }

                        // add check constraints
                        foreach (var constraint in request.AddCheckConstraints ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(constraint.SchemaName))
                            {
                                constraint.SchemaName = schemaName;
                            }
                            if (string.IsNullOrWhiteSpace(constraint.TableName))
                            {
                                constraint.TableName = tableName;
                            }
                            var added = await connection
                                .CreateCheckConstraintIfNotExistsAsync(
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!added)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to add check constraint.");
                            }
                        }

                        // add default constraints
                        foreach (var constraint in request.AddDefaultConstraints ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(constraint.SchemaName))
                            {
                                constraint.SchemaName = schemaName;
                            }
                            if (string.IsNullOrWhiteSpace(constraint.TableName))
                            {
                                constraint.TableName = tableName;
                            }
                            var added = await connection
                                .CreateDefaultConstraintIfNotExistsAsync(
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!added)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to add default constraint.");
                            }
                        }

                        // add unique constraints
                        foreach (var constraint in request.AddUniqueConstraints ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(constraint.SchemaName))
                            {
                                constraint.SchemaName = schemaName;
                            }
                            if (string.IsNullOrWhiteSpace(constraint.TableName))
                            {
                                constraint.TableName = tableName;
                            }
                            var added = await connection
                                .CreateUniqueConstraintIfNotExistsAsync(
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!added)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to add unique constraint.");
                            }
                        }

                        // add foreign key constraints
                        foreach (var constraint in request.AddForeignKeyConstraints ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(constraint.SchemaName))
                            {
                                constraint.SchemaName = schemaName;
                            }
                            if (string.IsNullOrWhiteSpace(constraint.TableName))
                            {
                                constraint.TableName = tableName;
                            }
                            var added = await connection
                                .CreateForeignKeyConstraintIfNotExistsAsync(
                                    constraint,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!added)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to add foreign key constraint.");
                            }
                        }

                        // add indexes
                        foreach (var index in request.AddIndexes ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(index.SchemaName))
                            {
                                index.SchemaName = schemaName;
                            }
                            if (string.IsNullOrWhiteSpace(index.TableName))
                            {
                                index.TableName = tableName;
                            }
                            var added = await connection
                                .CreateIndexIfNotExistsAsync(
                                    index,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!added)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to add index.");
                            }
                        }

                        // rename columns
                        foreach (var column in request.RenameColumnsTo ?? [])
                        {
                            var renamed = await connection
                                .RenameColumnIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    column.Key,
                                    column.Value,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!renamed)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to rename column.");
                            }
                        }

                        // rename table if requested
                        if (!string.IsNullOrWhiteSpace(request.RenameTableTo))
                        {
                            var renamed = await connection
                                .RenameTableIfExistsAsync(
                                    schemaName,
                                    tableName,
                                    request.RenameTableTo,
                                    tx: tx,
                                    cancellationToken: cancellationToken
                                )
                                .ConfigureAwait(false);
                            if (!renamed)
                            {
                                tx.Rollback();
                                return Results.BadRequest("Failed to rename table.");
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

                    var table = await connection
                        .GetTableAsync(schemaName, tableName, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    return table is null
                        ? Results.BadRequest("Failed to create table.")
                        : Results.Ok(new TableResponse(table));
                }
            )
            .WithName("UpdateTable")
            .WithDisplayName("Update Table")
            .WithSummary("Update the specified table in the specified schema.")
            .WithTags("DapperMatic")
            .WithGroupName("DDL")
            .Accepts<UpdateTableRequest>("application/json")
            .Produces<TableResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
}
