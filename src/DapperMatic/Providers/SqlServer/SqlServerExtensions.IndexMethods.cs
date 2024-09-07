using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> IndexExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                        AND name = @indexName and is_primary_key = 0 and is_unique_constraint = 0",
                    new { schemaAndTableName, indexName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateIndexIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string[] columnNames,
        string? schemaName = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (columnNames == null || columnNames.Length == 0)
            throw new ArgumentException(
                "At least one columnName must be specified.",
                nameof(columnNames)
            );

        if (
            await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columnNames);
        await ExecuteAsync(
                db,
                $@"
                CREATE {uniqueString} INDEX {indexName} ON {schemaAndTableName} ({columnList})
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public Task<IEnumerable<TableIndex>> GetIndexesAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetIndexNamesAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return QueryAsync<string>(
                db,
                $@"
                    SELECT name FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                            and is_primary_key = 0 and is_unique_constraint = 0
                        ORDER BY name",
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"
                    SELECT name FROM sys.indexes 
                        WHERE object_id = OBJECT_ID('{schemaAndTableName}') 
                            and name LIKE @where
                            and is_primary_key = 0 and is_unique_constraint = 0
                        ORDER BY name",
                new { schemaAndTableName, where },
                tx
            );
        }
    }

    public async Task<bool> DropIndexIfExistsAsync(
        IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, indexName) = NormalizeNames(schemaName, tableName, indexName);

        if (
            !await IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        await ExecuteAsync(
                db,
                $@"
                DROP INDEX [{schemaName}].[{tableName}].[{indexName}]
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
