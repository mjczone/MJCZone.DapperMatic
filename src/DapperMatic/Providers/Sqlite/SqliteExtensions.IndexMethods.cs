using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
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
        var (_, tableName, indexName) = NormalizeNames(schema, table, index);

        if (string.IsNullOrWhiteSpace(indexName))
            throw new ArgumentException("Index name must be specified in SQLite.", nameof(index));

        // this is the query to get all indexes for a table in SQLite
        // for DEBUGGING purposes
        // var fks = (
        //     await db.QueryAsync($@"select * from pragma_index_list('{tableName}')", tx)
        //         .ConfigureAwait(false)
        // )
        //     .Cast<IDictionary<string, object?>>()
        //     .ToArray();
        // var fksJson = JsonConvert.SerializeObject(fks);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                            FROM pragma_index_list('{tableName}')
                            WHERE ""origin"" = 'c' and ""name"" = @indexName",
                    new { tableName, indexName },
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
        var (_, tableName, indexName) = NormalizeNames(schema, table, index);

        if (columns == null || columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var uniqueString = unique ? "UNIQUE" : "";
        var columnList = string.Join(", ", columns);
        await ExecuteAsync(
                db,
                $@"
                CREATE {uniqueString} INDEX {indexName} ON {tableName} ({columnList})
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
        var (_, tableName, _) = NormalizeNames(schema, table);

        // can also query using
        // SELECT type, name, tbl_name, sql FROM sqlite_master WHERE type= 'index';

        if (string.IsNullOrWhiteSpace(filter))
        {
            return QueryAsync<string>(
                db,
                $@"SELECT ""name"" INDEX_NAME
                            FROM pragma_index_list('{tableName}')
                            WHERE ""origin"" = 'c'
                            ORDER BY INDEX_NAME",
                new { tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
            return QueryAsync<string>(
                db,
                $@"SELECT ""name"" INDEX_NAME
                            FROM pragma_index_list('{tableName}')
                            WHERE ""origin"" = 'c' and INDEX_NAME LIKE @where
                            ORDER BY INDEX_NAME",
                new { tableName, where },
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
        var (_, _, indexName) = NormalizeNames(schema, table, index);

        if (
            !await IndexExistsAsync(db, table, index, schema, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        await ExecuteAsync(
                db,
                $@"
                DROP INDEX {indexName}
                ",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
