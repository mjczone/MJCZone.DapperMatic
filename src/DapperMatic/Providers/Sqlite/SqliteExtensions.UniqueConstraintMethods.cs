using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> UniqueConstraintExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (_, tableName, uniqueConstraintName) = NormalizeNames(schema, table, uniqueConstraint);

        if (string.IsNullOrWhiteSpace(uniqueConstraintName))
            throw new ArgumentException(
                "Unique constraint name must be specified in SQLite.",
                nameof(uniqueConstraint)
            );

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
                            WHERE (""origin"" = 'u' or ""unique"" = 1) and ""name"" = @uniqueConstraintName",
                    new { tableName, uniqueConstraintName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string[] columns,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (_, tableName, uniqueConstraintName) = NormalizeNames(schema, table, uniqueConstraint);

        if (columns == null || columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await UniqueConstraintExistsAsync(
                    db,
                    table,
                    uniqueConstraint,
                    schema,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        // to create a unique index, you have to re-create the table in sqlite
        // so we will just create a regular index
        var columnList = string.Join(", ", columns);
        await ExecuteAsync(
                db,
                $@"
                CREATE UNIQUE INDEX {uniqueConstraintName} ON {tableName} ({columnList})
                ",
                transaction: tx
            )
            .ConfigureAwait(false);
        return true;
    }

    public Task<IEnumerable<string>> GetUniqueConstraintsAsync(
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
                            WHERE (""origin"" = 'u' or ""unique"" = 1)
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
                            WHERE (""origin"" = 'u' or ""unique"" = 1) and INDEX_NAME LIKE @where
                            ORDER BY INDEX_NAME",
                new { tableName, where },
                tx
            );
        }
    }

    public async Task<bool> DropUniqueConstraintIfExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await UniqueConstraintExistsAsync(
                    db,
                    table,
                    uniqueConstraint,
                    schema,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var (_, tableName, uniqueConstraintName) = NormalizeNames(schema, table, uniqueConstraint);

        // if it's an index, we can delete it (ASSUME THIS FOR NOW)
        if (
            0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                            FROM pragma_index_list('{tableName}')
                            WHERE (""origin"" = 'c' and ""unique"" = 1) and ""name"" = @uniqueConstraintName",
                    new { tableName, uniqueConstraintName },
                    tx
                )
                .ConfigureAwait(false)
        )
        {
            await ExecuteAsync(
                    db,
                    $@"
                DROP INDEX {uniqueConstraintName}
                ",
                    transaction: tx
                )
                .ConfigureAwait(false);

            return true;
        }

        // if it's a true unique constraint, we have to drop the table and re-create it
        return false;
    }
}
