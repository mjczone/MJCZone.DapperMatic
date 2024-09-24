using System.Data;
using System.Data.Common;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase, IDatabaseMethods
{
    protected override string DefaultSchema => "";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DbProviderType.Sqlite);

    internal SqliteMethods() { }

    public async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"select sqlite_version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }

    public Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return SqliteSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    private async Task<bool> AlterTableUsingRecreateTableStrategyAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        Func<DxTable, bool>? validateTable,
        Func<DxTable, DxTable> updateTable,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            return false;

        if (validateTable != null && !validateTable(table))
            return false;

        var newTable = updateTable(table);

        await AlterTableUsingRecreateTableStrategyAsync(
            db,
            schemaName,
            tableName,
            newTable,
            tx,
            cancellationToken
        );

        return true;
    }

    private async Task AlterTableUsingRecreateTableStrategyAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        DxTable updatedTable,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var newTableName = $"{tableName}_temp";
        updatedTable.TableName = newTableName;

        // get the create index sql statements for the existing table
        var createIndexStatements = await GetCreateIndexSqlStatementsForTable(
                db,
                schemaName,
                tableName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

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
            var created = await CreateTableIfNotExistsAsync(db, updatedTable, tx, cancellationToken)
                .ConfigureAwait(false);

            if (created)
            {
                // populate the new table with the data from the old table
                await ExecuteAsync(
                        db,
                        $@"INSERT INTO {updatedTable.TableName} SELECT * FROM {tableName}",
                        transaction: tx
                    )
                    .ConfigureAwait(false);

                // drop the old table
                await ExecuteAsync(db, $@"DROP TABLE {tableName}", transaction: tx)
                    .ConfigureAwait(false);

                // rename the new table to the old table name
                await ExecuteAsync(
                        db,
                        $@"ALTER TABLE {updatedTable.TableName} RENAME TO {tableName}",
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
    }
}
