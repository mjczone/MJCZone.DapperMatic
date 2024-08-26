using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> IndexExistsAsync(
        IDbConnection db,
        string table,
        string index,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, indexName) = NormalizeNames(schema, table, index);

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
        string table,
        string index,
        string[] columns,
        string? schema = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, indexName) = NormalizeNames(schema, table, index);

        if (columns == null || columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";
        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columns);
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

    public Task<IEnumerable<string>> GetIndexesAsync(
        IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, _) = NormalizeNames(schema, table);

        var schemaAndTableName = "[" + schemaName + "].[" + tableName + "]";

        if (string.IsNullOrWhiteSpace(filter))
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
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
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
        string table,
        string index,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (schemaName, tableName, indexName) = NormalizeNames(schema, table, index);

        if (
            !await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
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
