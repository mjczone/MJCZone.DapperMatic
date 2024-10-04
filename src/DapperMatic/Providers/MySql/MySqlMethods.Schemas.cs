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
        throw new NotImplementedException();
    }

    public override Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<string>> GetSchemaNamesAsync(
        IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}
