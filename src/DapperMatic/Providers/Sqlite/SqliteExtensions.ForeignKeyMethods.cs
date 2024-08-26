using System.Data;
using System.Data.Common;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public override Task<bool> SupportsNamedForeignKeysAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    public async Task<bool> ForeignKeyExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string? foreignKey = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (_, tableName, columnName) = NormalizeNames(schema, table, column);

        // foreign key names don't exist in sqlite, the columnName MUST be specified
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name must be specified in SQLite.", nameof(column));

        // this is the query to get all foreign keys for a table in SQLite
        // for DEBUGGING purposes
        // var fks = (
        //     await db.QueryAsync($@"select * from pragma_foreign_key_list('{tableName}')", tx)
        //         .ConfigureAwait(false)
        // )
        //     .Cast<IDictionary<string, object?>>()
        //     .ToArray();
        // var fksJson = JsonConvert.SerializeObject(fks);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    $@"SELECT COUNT(*) 
                            FROM pragma_foreign_key_list('{tableName}')
                            WHERE ""from"" = @columnName",
                    new { tableName, columnName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateForeignKeyIfNotExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schema = null,
        string onDelete = "NO ACTION",
        string onUpdate = "NO ACTION",
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(referenceTable))
            throw new ArgumentException(
                "Reference table name must be specified.",
                nameof(referenceTable)
            );
        if (string.IsNullOrWhiteSpace(referenceColumn))
            throw new ArgumentException(
                "Reference column name must be specified.",
                nameof(referenceColumn)
            );
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("Column name must be specified.", nameof(column));
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name must be specified.", nameof(table));

        if (
            await ForeignKeyExistsAsync(
                    db,
                    table,
                    column,
                    foreignKey,
                    schema,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        var (_, tableName, columnName) = NormalizeNames(schema, table, column);
        var (_, referenceTableName, referenceColumnName) = NormalizeNames(
            schema,
            referenceTable,
            referenceColumn
        );

        var createSqlStatement = (
            await QueryAsync<string>(
                    db,
                    $@"SELECT sql FROM sqlite_master WHERE type = 'table' AND name = @tableName",
                    new { tableName },
                    tx
                )
                .ConfigureAwait(false)
        ).Single();
        createSqlStatement = createSqlStatement.Trim().TrimEnd(')').Trim().TrimEnd(',');
        createSqlStatement +=
            $@", FOREIGN KEY (""{columnName}"") REFERENCES ""{referenceTableName}"" (""{referenceColumnName}"") ON DELETE {onDelete} ON UPDATE {onUpdate})";

        var innerTx =
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
        await ExecuteAsync(db, "PRAGMA foreign_keys = 0", tx ?? innerTx).ConfigureAwait(false);
        try
        {
            // first rename the table
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE '{tableName}' RENAME TO '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // re-create the table with the new constraint
            await ExecuteAsync(db, createSqlStatement, tx ?? innerTx).ConfigureAwait(false);
            // copy the data from the old table to the new table
            await ExecuteAsync(
                    db,
                    $@"INSERT INTO '{tableName}' SELECT * FROM '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // drop the old table
            await ExecuteAsync(db, $@"DROP TABLE '{tableName}_old'", tx ?? innerTx)
                .ConfigureAwait(false);
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx ?? innerTx).ConfigureAwait(false);
            if (tx == null)
                innerTx.Commit();
        }
        catch
        {
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx ?? innerTx).ConfigureAwait(false);
            if (tx == null)
                innerTx.Rollback();
            throw;
        }

        return true;
    }

    public Task<IEnumerable<string>> GetForeignKeysAsync(
        IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name must be specified.", nameof(table));

        var (_, tableName, _) = NormalizeNames(schema, table);

        if (string.IsNullOrWhiteSpace(filter))
        {
            return QueryAsync<string>(
                db,
                $@"SELECT 'fk_{tableName}'||'_'||""from""||'_'||""table""||'_'||""to"" CONSTRAINT_NAME, * 
                    FROM pragma_foreign_key_list('{tableName}')
                    ORDER BY CONSTRAINT_NAME",
                new { tableName },
                tx
            );
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");

            return QueryAsync<string>(
                db,
                $@"SELECT 'fk_{tableName}'||'_'||""from""||'_'||""table""||'_'||""to"" CONSTRAINT_NAME, * 
                    FROM pragma_foreign_key_list('{tableName}')
                    WHERE CONSTRAINT_NAME LIKE @where
                    ORDER BY CONSTRAINT_NAME",
                new { tableName, where },
                tx
            );
        }
    }

    /// <summary>
    /// In SQLite, to drop a foreign key, you must re-create the table without the foreign key,
    /// and then re-insert the data. It's a costly operation.
    /// </summary>
    /// <remarks>
    /// Example: https://www.techonthenet.com/sqlite/foreign_keys/drop.php
    /// </remarks>
    public async Task<bool> DropForeignKeyIfExistsAsync(
        IDbConnection db,
        string table,
        string column,
        string? foreignKey = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(table))
            throw new ArgumentException("Table name must be specified.", nameof(table));
        if (string.IsNullOrWhiteSpace(column))
            throw new ArgumentException("Column name must be specified.", nameof(column));

        var fkExists = await ForeignKeyExistsAsync(
                db,
                table,
                column,
                foreignKey,
                schema,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (!fkExists)
            return false;

        var (_, tableName, columnName) = NormalizeNames(schema, table, column);

        var originalCreateSqlStatement = (
            await QueryAsync<string>(
                    db,
                    $@"SELECT sql FROM sqlite_master WHERE type = 'table' AND name = @tableName",
                    new { tableName }
                )
                .ConfigureAwait(false)
        ).Single();

        // this statement will look like this:
        /*
         CREATE TABLE "table" (
            "column" INTEGER,
            FOREIGN KEY ("column") REFERENCES "referenceTable" ("referenceColumn") ON DELETE NO ACTION ON UPDATE NO ACTION
        )
        */

        // remove the foreign key constraint from the create statement
        var indexOfForeignKeyClause = originalCreateSqlStatement.IndexOf(
            $"FOREIGN KEY (\"{columnName}\")",
            StringComparison.OrdinalIgnoreCase
        );
        if (indexOfForeignKeyClause < 0)
            throw new InvalidOperationException(
                "Foreign key constraint not found in the table create statement."
            );

        var createSqlStatement = originalCreateSqlStatement;
        // find the next ',' after the foreign key clause
        var indexOfNextComma = createSqlStatement.IndexOf(',', indexOfForeignKeyClause);
        if (indexOfNextComma > 0)
        {
            // replace the foreign key clause including the command with an empty string
            createSqlStatement = createSqlStatement.Remove(
                indexOfForeignKeyClause,
                indexOfNextComma - indexOfForeignKeyClause + 1
            );
        }
        else
        {
            // if there is no comma, assume it's the last statement, and remove the clause up until the last ')'
            var indexOfLastParenthesis = createSqlStatement.LastIndexOf(')');
            if (indexOfLastParenthesis > 0)
            {
                createSqlStatement =
                    createSqlStatement
                        .Remove(
                            indexOfForeignKeyClause,
                            indexOfLastParenthesis - indexOfForeignKeyClause + 1
                        )
                        .Trim()
                        .TrimEnd(',') + "\n)";
            }
        }

        // throw an error if the createSqlStatement is the same as the original
        if (createSqlStatement == originalCreateSqlStatement)
            throw new InvalidOperationException(
                "Foreign key constraint not found in the table create statement."
            );

        var innerTx =
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
        await ExecuteAsync(db, "PRAGMA foreign_keys = 0", tx ?? innerTx).ConfigureAwait(false);
        try
        {
            // first rename the table
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE '{tableName}' RENAME TO '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // re-create the table with the new constraint
            await ExecuteAsync(db, createSqlStatement, tx ?? innerTx).ConfigureAwait(false);
            // copy the data from the old table to the new table
            await ExecuteAsync(
                    db,
                    $@"INSERT INTO '{tableName}' SELECT * FROM '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // drop the old table
            await ExecuteAsync(db, $@"DROP TABLE '{tableName}_old'", tx ?? innerTx)
                .ConfigureAwait(false);
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx ?? innerTx).ConfigureAwait(false);
            if (tx == null)
                innerTx.Commit();
        }
        catch
        {
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx ?? innerTx).ConfigureAwait(false);
            if (tx == null)
                innerTx.Rollback();
            throw;
        }
        finally
        {
            if (tx == null)
                innerTx.Dispose();
        }

        return true;
    }
}
