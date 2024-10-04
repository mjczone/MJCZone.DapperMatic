using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    public override async Task<List<DxIndex>> GetIndexesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        return await GetIndexesInternalAsync(
                db,
                schemaName,
                tableName,
                indexNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
