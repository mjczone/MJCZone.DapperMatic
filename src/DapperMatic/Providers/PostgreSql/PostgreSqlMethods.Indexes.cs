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

    public override async Task<bool> DropIndexIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await DoesIndexExistAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        // drop index
        await ExecuteAsync(db, $@"DROP INDEX {indexName} CASCADE", transaction: tx)
            .ConfigureAwait(false);

        return true;
    }
}
