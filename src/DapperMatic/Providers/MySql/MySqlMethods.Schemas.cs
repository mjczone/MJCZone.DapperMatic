using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    protected override string DefaultSchema => "";

    public override Task<bool> DoesSchemaExistAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public override Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public override Task<IEnumerable<string>> GetSchemaNamesAsync(
        IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // does not support schemas, so we return an empty list
        return Task.FromResult(Enumerable.Empty<string>());
    }

    public override Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }
}
