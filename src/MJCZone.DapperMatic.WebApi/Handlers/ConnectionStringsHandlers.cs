using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MJCZone.DapperMatic.WebApi.HandlerTypes;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Handles connection strings for the application.
/// </summary>
public static class ConnectionStringsHandlers
{
    /// <summary>
    /// Adds connection strings handlers to the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void AddConnectionStringsHandlers(this WebApplication app)
    {
        var options = app
            .Services.GetRequiredService<IOptionsMonitor<DapperMaticOptions>>()
            ?.CurrentValue;

        var prefix = options.GetApiPrefix();

        app.MapGet(
                prefix + "/cs/vault-factories",
                ([FromServices] IEnumerable<IConnectionStringsVaultFactory> factories) =>
                {
                    var factoryNames = factories.Select(f => f.Name);
                    return Results.Ok(new StringListResponse(factoryNames));
                }
            )
            .WithName("GetConnectionStringsVaultFactoryNames")
            .WithDisplayName("Get ConnectionStrings Vault Factory Names")
            .WithSummary("Gets the names of the connection strings vault factories.")
            .WithTags("DapperMatic")
            .WithGroupName("ConnectionStrings")
            .Produces<StringListResponse>()
            .RequireAuthorization();

        app.MapGet(
                prefix + "/cs/vaults",
                (
                    [FromServices] IOptionsMonitor<DapperMaticOptions> options,
                    [FromQuery] string? factory = null
                ) =>
                {
                    var defaultVaultName =
                        options.CurrentValue?.DefaultConnectionStringsVaultName ?? string.Empty;
                    var vaults = options.CurrentValue?.ConnectionStringsVaults ?? [];

                    var vaultInfos = vaults
                        .Select(v =>
                        {
                            var vaultInfo = new ConnectionStringsVaultInfo
                            {
                                Name = v.Key,
                                FactoryName = v.Value?.FactoryName,
                                IsDefault = defaultVaultName.Equals(
                                    v.Key,
                                    StringComparison.OrdinalIgnoreCase
                                ),
                                IsReadOnly = v.Value?.IsReadOnly ?? false,
                            };
                            return vaultInfo;
                        })
                        .OrderBy(d => d.Name);

                    var list = string.IsNullOrWhiteSpace(factory)
                        ? vaultInfos
                        : vaultInfos.Where(v => v.FactoryName == factory);

                    return Results.Ok(new ConnectionStringsVaultInfoResponse(list));
                }
            )
            .WithName("GetConnectionStringsVaults")
            .WithDisplayName("Get ConnectionStrings Vaults")
            .WithSummary("Gets the connection strings vaults.")
            .WithTags("DapperMatic")
            .WithGroupName("ConnectionStrings")
            .Produces<ConnectionStringsVaultInfoResponse>()
            .RequireAuthorization();

        app.MapPut(
                prefix + "/cs/entries",
                async (
                    HttpContext context,
                    [FromBody] ConnectionStringsEntryRequest request,
                    [FromServices] IOptionsMonitor<DapperMaticOptions> options,
                    [FromServices] IEnumerable<IConnectionStringsVaultFactory> vaultFactories,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    var name = request.Name;
                    var connectionString = request.ConnectionString;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        return Results.BadRequest("Name is required.");
                    }

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        return Results.BadRequest("Connection string is required.");
                    }

                    var result = PopulateVaultInstance(
                        options.CurrentValue,
                        request.Vault,
                        vaultFactories,
                        out var vaultInstance
                    );
                    if (result is not null)
                    {
                        return result;
                    }

                    if (vaultInstance == null)
                    {
                        return Results.NotFound("Vault not found.");
                    }

                    // make sure the user is allowed to delete the connection string
                    var roles = vaultInstance.GetRoles();
                    if (roles.Length > 0 && roles.All(r => !context.User.IsInRole(r)))
                    {
                        return Results.Forbid();
                    }

                    await vaultInstance
                        .SetConnectionStringAsync(name, connectionString, cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok(new EmptyResponse());
                }
            )
            .WithName("SetConnectionStringsEntry")
            .WithDisplayName("Set ConnectionStrings Entry")
            .WithSummary("Sets a connection strings entry.")
            .WithTags("DapperMatic")
            .WithGroupName("ConnectionStrings")
            .Accepts<ConnectionStringsEntryRequest>("application/json")
            .Produces<EmptyResponse>()
            .RequireAuthorization();

        app.MapDelete(
                prefix + "/cs/entries",
                async (
                    HttpContext context,
                    [FromQuery] string name,
                    [FromQuery] string vault,
                    [FromServices] IOptionsMonitor<DapperMaticOptions> options,
                    [FromServices] IEnumerable<IConnectionStringsVaultFactory> vaultFactories,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        return Results.BadRequest("Name is required.");
                    }

                    if (string.IsNullOrWhiteSpace(vault))
                    {
                        return Results.BadRequest("Vault is required.");
                    }

                    var result = PopulateVaultInstance(
                        options.CurrentValue,
                        vault,
                        vaultFactories,
                        out var vaultInstance
                    );
                    if (result is not null)
                    {
                        return result;
                    }

                    if (vaultInstance == null)
                    {
                        return Results.NotFound("Vault not found.");
                    }

                    // make sure the user is allowed to delete the connection string
                    var roles = vaultInstance.GetRoles();
                    if (roles.Length > 0 && roles.All(r => !context.User.IsInRole(r)))
                    {
                        return Results.Forbid();
                    }

                    await vaultInstance
                        .DeleteConnectionStringAsync(name, cancellationToken)
                        .ConfigureAwait(false);

                    return Results.Ok(new EmptyResponse());
                }
            )
            .WithName("RemoveConnectionStringsEntry")
            .WithDisplayName("Remove ConnectionStrings Entry")
            .WithSummary("Removes a connection strings entry.")
            .WithTags("DapperMatic")
            .WithGroupName("ConnectionStrings")
            .Accepts<ConnectionStringsEntryRequest>("application/json")
            .Produces<EmptyResponse>()
            .RequireAuthorization();
    }

    private static IResult? PopulateVaultInstance(
        DapperMaticOptions? options,
        string? vaultName,
        IEnumerable<IConnectionStringsVaultFactory> vaultFactories,
        out IConnectionStringsVault? vaultInstance
    )
    {
        vaultInstance = null;

        var defaultVaultName = options?.DefaultConnectionStringsVaultName ?? string.Empty;
        var vaults = options?.ConnectionStringsVaults ?? [];

        var vault = string.IsNullOrWhiteSpace(vaultName) ? defaultVaultName : vaultName;

        if (string.IsNullOrWhiteSpace(vault))
        {
            return Results.BadRequest("Vault name is required.");
        }

        // get the options for the vault
        if (
            !vaults
                .ToDictionary(v => v.Key.ToLowerInvariant(), v => v)
                .TryGetValue(vault.ToLowerInvariant(), out var vaultOptions)
            || vaultOptions.Value == null
        )
        {
            return Results.NotFound("Vault not found.");
        }

        var vaultFactory = vaultFactories.FirstOrDefault(f =>
            f.Name.Equals(vaultOptions.Value.FactoryName, StringComparison.OrdinalIgnoreCase)
        );

        if (vaultFactory == null)
        {
            return Results.NotFound("Vault factory not found.");
        }

        vaultInstance = vaultFactory.Create(vault, vaultOptions.Value);
        return null;
    }
}
