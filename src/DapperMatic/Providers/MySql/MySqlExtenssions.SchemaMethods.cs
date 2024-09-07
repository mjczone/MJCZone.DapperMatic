using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public override Task<bool> SupportsSchemasAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public Task<bool> SchemaExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetSchemasAsync(
        IDbConnection db,
        string? nameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // does not support schemas, so we return an empty list
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }
}
