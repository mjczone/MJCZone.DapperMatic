using System.Runtime.CompilerServices;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MJCZone.DapperMatic.WebApi.Handlers;

[assembly: InternalsVisibleTo("MJCZone.DapperMatic.WebApi.Tests")]

namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Extension methods for setting up DapperMatic services in an <see cref="IServiceCollection" />.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Registers services required by DapperMatic services and configures <see cref="DapperMaticOptions"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">A delegate to configure <see cref="DapperMaticOptions"/>.</param>
    public static void AddDapperMatic(
        this IServiceCollection services,
        Action<DapperMaticOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());

        services.AddSingleton<ITenantIdentifierResolver, TenantIdentifierInHeaderResolver>();
        services.AddSingleton<ITenantIdentifierResolver, TenantIdentifierInQueryStringResolver>();

        services.AddSingleton<IConnectionStringsVaultFactory, ConnectionStringsFileVaultFactory>();
        services.AddSingleton<
            IConnectionStringsVaultFactory,
            ConnectionStringsDatabaseVaultFactory
        >();
        services.TryAddSingleton<IDatabaseRegistry, DatabaseRegistry>();
        services.TryAddSingleton<
            IDatabaseRegistryConnectionFactory,
            DatabaseRegistryConnectionFactory
        >();
        services.TryAddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();

        services.AddOptions<DapperMaticOptions>().BindConfiguration("DapperMatic");

        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.PostConfigure<DapperMaticOptions>(options => { });
    }

    /// <summary>
    /// Adds DapperMatic middleware to the pipeline.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/>.</param>
    /// <returns>
    /// The <see cref="WebApplication"/> with DapperMatic middleware added.
    /// </returns>
    public static WebApplication UseDapperMatic(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // initialize the database registry
        app.Services.GetRequiredService<IDatabaseRegistry>()
            .InitializeAsync()
            .GetAwaiter()
            .GetResult();

        app.UseMiddleware<TenantIdentifierResolverMiddleware>();

        app.AddConnectionStringsHandlers();
        app.AddDatabaseHandlers();
        app.AddDatabaseSchemaHandlers();
        app.AddDatabaseTableHandlers();
        app.AddDatabaseViewHandlers();
        app.AddOperationHandlers();

        return app;
    }
}

/// <summary>
/// Represents a type handler for <see cref="Guid"/>.
/// </summary>
public class GuidHandler : SqlMapper.ITypeHandler
{
    /// <summary>
    /// Converts the value to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="destinationType">The type to convert to.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    public object? Parse(Type destinationType, object value)
    {
        return Guid.Parse((string)value);
    }

    /// <summary>
    /// Sets the value of a parameter.
    /// </summary>
    /// <param name="parameter">The parameter to set the value of.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.Value = value.ToString();
    }
}
