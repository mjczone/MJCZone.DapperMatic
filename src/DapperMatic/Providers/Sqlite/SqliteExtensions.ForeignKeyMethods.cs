using System.Data;
using System.Data.Common;
using DapperMatic.Models;

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
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        // foreign key names don't exist in sqlite, the columnName MUST be specified
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException(
                "Column name must be specified in SQLite.",
                nameof(columnName)
            );

        // this is the query to get all foreign keys for a tableName in SQLite
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
        string tableName,
        string columnName,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schemaName = null,
        string onDelete = "NO ACTION",
        string onUpdate = "NO ACTION",
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(referenceTable))
            throw new ArgumentException(
                "Reference tableName name must be specified.",
                nameof(referenceTable)
            );
        if (string.IsNullOrWhiteSpace(referenceColumn))
            throw new ArgumentException(
                "Reference columnName name must be specified.",
                nameof(referenceColumn)
            );
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name must be specified.", nameof(columnName));
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name must be specified.", nameof(tableName));

        if (
            await ForeignKeyExistsAsync(
                    db,
                    tableName,
                    columnName,
                    foreignKey,
                    schemaName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);
        var (_, referenceTableName, referenceColumnName) = NormalizeNames(
            schemaName,
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
            // first rename the tableName
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE '{tableName}' RENAME TO '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // re-create the tableName with the new constraint
            await ExecuteAsync(db, createSqlStatement, tx ?? innerTx).ConfigureAwait(false);
            // copy the data from the old tableName to the new tableName
            await ExecuteAsync(
                    db,
                    $@"INSERT INTO '{tableName}' SELECT * FROM '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // drop the old tableName
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

    public async Task<IEnumerable<ForeignKey>> GetForeignKeysAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            $@"SELECT 'fk_'||sm.name||'_'||p.""from""||'_'||p.""table""||'_'||p.""to"" AS constraint_name,
                        sm.name AS table_name,
                        p.""from"" AS column_name,
                        p.""table"" AS referenced_table_name,
                        p.""to"" AS referenced_column_name,
                        p.on_delete AS delete_rule,
                        p.on_update AS update_rule
                    FROM sqlite_master sm
                        JOIN pragma_foreign_key_list(sm.name) p
                    WHERE sm.type = 'table'";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND sm.name = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND constraint_name LIKE @where";
        sql += $@" order by constraint_name";

        var results = await QueryAsync<(
            string constraint_name,
            string table_name,
            string column_name,
            string referenced_table_name,
            string referenced_column_name,
            string delete_rule,
            string update_rule
        )>(
                db,
                sql,
                new
                {
                    schemaName,
                    tableName,
                    where
                },
                tx
            )
            .ConfigureAwait(false);

        return results.Select(r =>
        {
            var deleteRule = (r.delete_rule ?? "").Replace('_', ' ') switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "NO ACTION" => ReferentialAction.NoAction,
                _ => ReferentialAction.NoAction
            };
            var updateRule = (r.update_rule ?? "").Replace('_', ' ') switch
            {
                "CASCADE" => ReferentialAction.Cascade,
                "SET NULL" => ReferentialAction.SetNull,
                "NO ACTION" => ReferentialAction.NoAction,
                _ => ReferentialAction.NoAction
            };

            return new ForeignKey(
                null,
                r.constraint_name,
                r.table_name,
                r.column_name,
                r.referenced_table_name,
                r.referenced_column_name,
                deleteRule,
                updateRule
            );
        });
    }

    public async Task<IEnumerable<string>> GetForeignKeyNamesAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName);

        var where = string.IsNullOrWhiteSpace(nameFilter)
            ? null
            : $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");

        var sql =
            $@"SELECT 'fk_'||sm.name||'_'||p.""from""||'_'||p.""table""||'_'||p.""to"" AS constraint_name
                FROM sqlite_master sm
                    JOIN pragma_foreign_key_list(sm.name) p
                WHERE sm.type = 'table'";
        if (!string.IsNullOrWhiteSpace(tableName))
            sql += $@" AND sm.name = @tableName";
        if (!string.IsNullOrWhiteSpace(where))
            sql += $@" AND constraint_name LIKE @where";
        sql += @" ORDER BY constraint_name";

        return await QueryAsync<string>(
                db,
                sql,
                new
                {
                    schemaName,
                    tableName,
                    where
                },
                tx
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// In SQLite, to drop a foreign key, you must re-create the tableName without the foreign key,
    /// and then re-insert the data. It's a costly operation.
    /// </summary>
    /// <remarks>
    /// Example: https://www.techonthenet.com/sqlite/foreign_keys/drop.php
    /// </remarks>
    public async Task<bool> DropForeignKeyIfExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name must be specified.", nameof(tableName));
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name must be specified.", nameof(columnName));

        var fkExists = await ForeignKeyExistsAsync(
                db,
                tableName,
                columnName,
                foreignKey,
                schemaName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (!fkExists)
            return false;

        (_, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

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
         CREATE TABLE "tableName" (
            "columnName" INTEGER,
            FOREIGN KEY ("columnName") REFERENCES "referenceTable" ("referenceColumn") ON DELETE NO ACTION ON UPDATE NO ACTION
        )
        */

        // remove the foreign key constraint from the create statement
        var indexOfForeignKeyClause = originalCreateSqlStatement.IndexOf(
            $"FOREIGN KEY (\"{columnName}\")",
            StringComparison.OrdinalIgnoreCase
        );
        if (indexOfForeignKeyClause < 0)
            throw new InvalidOperationException(
                "Foreign key constraint not found in the tableName create statement."
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
                "Foreign key constraint not found in the tableName create statement."
            );

        var innerTx =
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
        await ExecuteAsync(db, "PRAGMA foreign_keys = 0", tx ?? innerTx).ConfigureAwait(false);
        try
        {
            // first rename the tableName
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE '{tableName}' RENAME TO '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // re-create the tableName with the new constraint
            await ExecuteAsync(db, createSqlStatement, tx ?? innerTx).ConfigureAwait(false);
            // copy the data from the old tableName to the new tableName
            await ExecuteAsync(
                    db,
                    $@"INSERT INTO '{tableName}' SELECT * FROM '{tableName}_old'",
                    tx ?? innerTx
                )
                .ConfigureAwait(false);
            // drop the old tableName
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
