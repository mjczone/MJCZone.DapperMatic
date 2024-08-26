using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public Task<IEnumerable<string>> GetSchemasAsync(
        IDbConnection db,
        string? filter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // does not support schemas, so we return an empty list
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }
}
