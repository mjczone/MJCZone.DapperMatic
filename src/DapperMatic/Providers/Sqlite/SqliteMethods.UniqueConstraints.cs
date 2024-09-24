using System.Data;
using System.Data.Common;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    public override async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, constraintName) = NormalizeNames(schemaName, tableName, constraintName);

        if (columns.Length == 0)
            throw new ArgumentException("At least one column must be specified.", nameof(columns));

        if (
            await UniqueConstraintExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    constraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        // to create a unique index, you have to re-create the table in sqlite
        // so we could just create a regular index, but then we already have a method for that
        // var sql =
        //     $@"CREATE UNIQUE INDEX {constraintName} ON {tableName} ({string.Join(", ", columns.Select(c => c.ToString()))})";
        // await ExecuteAsync(db, sql, transaction: tx).ConfigureAwait(false);

        // get the create table sql for the existing table
        var sql = await ExecuteScalarAsync<string>(
                db,
                $@"SELECT sql FROM sqlite_master WHERE type = 'table' AND name = @tableName",
                new { tableName },
                transaction: tx
            )
            .ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        // get the create index sql statements for the existing table
        var createIndexStatements = await QueryAsync<string>(
                db,
                $@"SELECT sql FROM sqlite_master WHERE type = 'index' and tbl_name = @tableName and sql is not null",
                new { tableName },
                transaction: tx
            )
            .ConfigureAwait(false);

        // create a new table with the same name as the old table, but with a temporary suffix
        // this is the safest approach as it will not break any existing data or references
        // however, it might be risky if there are foreign key constraints or other dependencies on the old table
        var newTableName = $"{tableName}_temp";
        // try renaming the table in the sql statement from safest approach to most risky approach
        var newTableSql = sql.Replace(
            $"CREATE TABLE {tableName}",
            $"CREATE TABLE {newTableName}",
            StringComparison.OrdinalIgnoreCase
        );
        if (newTableSql == sql)
            newTableSql = sql.Replace(
                $"CREATE TABLE \"{tableName}\"",
                $"CREATE TABLE \"{newTableName}\"",
                StringComparison.OrdinalIgnoreCase
            );
        if (newTableSql == sql)
            newTableSql = sql.Replace(tableName, newTableName, StringComparison.OrdinalIgnoreCase);
        if (newTableSql == sql)
            return false;

        // add the constraint to the end of the sql statement
        newTableSql = newTableSql.Insert(
            newTableSql.LastIndexOf(")"),
            $", CONSTRAINT {constraintName} UNIQUE ({string.Join(", ", columns.Select(c => c.ToString()))})"
        );

        // disable foreign key constraints temporarily
        await ExecuteAsync(db, "PRAGMA foreign_keys = 0", tx).ConfigureAwait(false);

        var innerTx = (DbTransaction)(
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false)
        );
        try
        {
            // create the new table
            await ExecuteAsync(db, newTableSql, transaction: innerTx).ConfigureAwait(false);

            // populate the new table with the data from the old table
            await ExecuteAsync(
                    db,
                    $@"INSERT INTO {newTableName} SELECT * FROM {tableName}",
                    transaction: innerTx
                )
                .ConfigureAwait(false);

            // drop the old table
            await ExecuteAsync(db, $@"DROP TABLE {tableName}", transaction: innerTx)
                .ConfigureAwait(false);

            // rename the new table to the old table name
            await ExecuteAsync(
                    db,
                    $@"ALTER TABLE {newTableName} RENAME TO {tableName}",
                    transaction: innerTx
                )
                .ConfigureAwait(false);

            // add back the indexes to the new table
            foreach (var createIndexStatement in createIndexStatements)
            {
                await ExecuteAsync(db, createIndexStatement, null, transaction: innerTx)
                    .ConfigureAwait(false);
            }

            //TODO: add back the triggers to the new table

            //TODO: add back the views to the new table

            // commit the transaction
            if (tx == null)
            {
                await innerTx.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            if (tx == null)
            {
                await innerTx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            throw;
        }
        finally
        {
            if (tx == null)
            {
                await innerTx.DisposeAsync();
            }
            // re-enable foreign key constraints
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx).ConfigureAwait(false);
        }

        return true;
    }

    public override async Task<bool> DropUniqueConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, constraintName) = NormalizeNames(schemaName, tableName, constraintName);

        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (
            table == null
            || table.UniqueConstraints.All(x =>
                !x.ConstraintName.Equals(constraintName, StringComparison.OrdinalIgnoreCase)
            )
        )
            return false;

        // to drop a unique index, you have to re-create the table in sqlite

        // get the create table sql for the existing table
        // var sql = await ExecuteScalarAsync<string>(
        //         db,
        //         $@"SELECT sql FROM sqlite_master WHERE type = 'table' AND name = @tableName",
        //         new { tableName },
        //         transaction: tx
        //     )
        //     .ConfigureAwait(false);
        // if (string.IsNullOrWhiteSpace(sql))
        //     return false;

        // get the create index sql statements for the existing table
        var createIndexStatements = await QueryAsync<string>(
                db,
                $@"SELECT sql FROM sqlite_master WHERE type = 'index' and tbl_name = @tableName and sql is not null",
                new { tableName },
                transaction: tx
            )
            .ConfigureAwait(false);

        // create a new table with the same name as the old table, but with a temporary suffix
        var newTableName = $"{tableName}_temp";
        table.TableName = newTableName;
        table.UniqueConstraints.RemoveAll(x =>
            x.ConstraintName.Equals(constraintName, StringComparison.OrdinalIgnoreCase)
        );

        // disable foreign key constraints temporarily
        await ExecuteAsync(db, "PRAGMA foreign_keys = 0", tx).ConfigureAwait(false);

        var innerTx = (DbTransaction)(
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false)
        );
        try
        {
            var created = await CreateTableIfNotExistsAsync(db, table, tx, cancellationToken)
                .ConfigureAwait(false);

            if (created)
            {
                // populate the new table with the data from the old table
                await ExecuteAsync(
                        db,
                        $@"INSERT INTO {newTableName} SELECT * FROM {tableName}",
                        transaction: tx
                    )
                    .ConfigureAwait(false);

                // drop the old table
                await ExecuteAsync(db, $@"DROP TABLE {tableName}", transaction: tx)
                    .ConfigureAwait(false);

                // rename the new table to the old table name
                await ExecuteAsync(
                        db,
                        $@"ALTER TABLE {newTableName} RENAME TO {tableName}",
                        transaction: tx
                    )
                    .ConfigureAwait(false);

                // add back the indexes to the new table
                foreach (var createIndexStatement in createIndexStatements)
                {
                    await ExecuteAsync(db, createIndexStatement, null, transaction: innerTx)
                        .ConfigureAwait(false);
                }

                //TODO: add back the triggers to the new table

                //TODO: add back the views to the new table

                // commit the transaction
                if (tx == null)
                {
                    await innerTx.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            if (tx == null)
            {
                await innerTx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            throw;
        }
        finally
        {
            if (tx == null)
            {
                await innerTx.DisposeAsync();
            }
            // re-enable foreign key constraints
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx).ConfigureAwait(false);
        }

        return true;
    }
}
