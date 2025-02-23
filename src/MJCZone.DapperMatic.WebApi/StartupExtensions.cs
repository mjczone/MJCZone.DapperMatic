using System.Runtime.CompilerServices;
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

        services.TryAddSingleton<IConnectionStringVault, ConnectionStringFileVault>();
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

        app.AddDatabaseHandlers();

        return app;
    }
}
