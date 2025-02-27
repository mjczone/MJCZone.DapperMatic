using Microsoft.AspNetCore.Http;

namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Resolves the tenant identifier from the HTTP context.
/// </summary>
public interface ITenantIdentifierResolver
{
    /// <summary>
    /// Resolves the tenant identifier from the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant identifier.</returns>
    Task<string?> ResolveTenantIdentifierAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Resolves the tenant identifier from the HTTP context header.
/// </summary>
public class TenantIdentifierInHeaderResolver : ITenantIdentifierResolver
{
    /// <summary>
    /// Gets or sets the name of the header that contains the tenant identifier.
    /// </summary>
    public static string? HeaderName { get; set; } = "X-Tenant";

    /// <summary>
    /// Resolves the tenant identifier from the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant identifier.</returns>
    public Task<string?> ResolveTenantIdentifierAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(HeaderName))
        {
            return Task.FromResult<string?>(null);
        }

        var tenantIdentifier = httpContext.Request.Headers.TryGetValue(HeaderName, out var values)
            ? values.FirstOrDefault()
            : null;

        return Task.FromResult(tenantIdentifier);
    }
}

/// <summary>
/// Resolves the tenant identifier from the HTTP context header.
/// </summary>
public class TenantIdentifierInQueryStringResolver : ITenantIdentifierResolver
{
    /// <summary>
    /// Gets or sets the name of the query string that contains the tenant identifier.
    /// </summary>
    public static string? QueryStringName { get; set; } = "tid";

    /// <summary>
    /// Resolves the tenant identifier from the HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant identifier.</returns>
    public Task<string?> ResolveTenantIdentifierAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(QueryStringName))
        {
            return Task.FromResult<string?>(null);
        }

        var tenantIdentifier = httpContext.Request.Query.TryGetValue(
            QueryStringName,
            out var values
        )
            ? values.FirstOrDefault()
            : null;

        return Task.FromResult(tenantIdentifier);
    }
}

/// <summary>
/// Middleware that resolves the tenant identifier from the HTTP context.
/// </summary>
public class TenantIdentifierResolverMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantIdentifierResolverMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public TenantIdentifierResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="tenantIdentifierResolvers">The tenant identifier resolver strategies (first hit wins).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public async Task InvokeAsync(
        HttpContext httpContext,
        IEnumerable<ITenantIdentifierResolver> tenantIdentifierResolvers,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var resolver in tenantIdentifierResolvers)
        {
            var tenantIdentifier = await resolver
                .ResolveTenantIdentifierAsync(httpContext, cancellationToken)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(tenantIdentifier))
            {
                httpContext.Items["TenantIdentifier"] = tenantIdentifier;

                // found the tenant identifier, no need to check other resolvers
                break;
            }
        }

        await _next(httpContext).ConfigureAwait(false);
    }
}

/// <summary>
/// Extensions for the tenant resolver middleware.
/// </summary>
public static class TenantIdentifierResolverMiddlewareExtensions
{
    /// <summary>
    /// Adds the tenant resolver middleware to the pipeline.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The tenant identifier.</returns>
    public static string? GetTenantIdentifier(this HttpContext httpContext)
    {
        return httpContext.Items["TenantIdentifier"] as string;
    }
}
