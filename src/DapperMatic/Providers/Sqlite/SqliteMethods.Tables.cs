using System.Data;
using System.Data.Common;
using System.Text;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    public override async Task<bool> DoesTableExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @tableName",
                    new { tableName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public override async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        DxColumn[]? columns = null,
        DxPrimaryKeyConstraint? primaryKey = null,
        DxCheckConstraint[]? checkConstraints = null,
        DxDefaultConstraint[]? defaultConstraints = null,
        DxUniqueConstraint[]? uniqueConstraints = null,
        DxForeignKeyConstraint[]? foreignKeyConstraints = null,
        DxIndex[]? indexes = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        var fillWithAdditionalIndexesToCreate = new List<DxIndex>();

        var sql = new StringBuilder();

        sql.AppendLine($"CREATE TABLE {tableName} (");
        var columnDefinitionClauses = new List<string>();
        for (var i = 0; i < columns?.Length; i++)
        {
            var column = columns[i];

            var colSql = BuildColumnDefinitionSql(
                tableName,
                column.ColumnName,
                column.DotnetType,
                column.ProviderDataType,
                column.Length,
                column.Precision,
                column.Scale,
                column.CheckExpression,
                column.DefaultExpression,
                column.IsNullable,
                column.IsPrimaryKey,
                column.IsAutoIncrement,
                column.IsUnique,
                column.IsIndexed,
                column.IsForeignKey,
                column.ReferencedTableName,
                column.ReferencedColumnName,
                column.OnDelete,
                column.OnUpdate,
                primaryKey,
                checkConstraints,
                defaultConstraints,
                uniqueConstraints,
                foreignKeyConstraints,
                indexes,
                fillWithAdditionalIndexesToCreate
            );

            columnDefinitionClauses.Add(colSql.ToString());
        }
        sql.AppendLine(string.Join(", ", columnDefinitionClauses));

        // add single column primary key constraints as column definitions; and,
        // add multi column primary key constraints here
        if (primaryKey != null && primaryKey.Columns.Length > 1)
        {
            var pkColumns = primaryKey.Columns.Select(c => c.ToString());
            var pkColumnNames = primaryKey.Columns.Select(c => c.ColumnName);
            sql.AppendLine(
                $", CONSTRAINT pk_{tableName}_{string.Join('_', pkColumnNames)} PRIMARY KEY ({string.Join(", ", pkColumns)})"
            );
        }

        // add check constraints
        if (checkConstraints != null && checkConstraints.Length > 0)
        {
            foreach (
                var constraint in checkConstraints.Where(c =>
                    !string.IsNullOrWhiteSpace(c.Expression)
                )
            )
            {
                var checkConstraintName = ToAlphaNumericString(constraint.ConstraintName);
                sql.AppendLine(
                    $", CONSTRAINT {checkConstraintName} CHECK ({constraint.Expression})"
                );
            }
        }

        // add foreign key constraints
        if (foreignKeyConstraints != null && foreignKeyConstraints.Length > 0)
        {
            foreach (var constraint in foreignKeyConstraints)
            {
                var fkName = ToAlphaNumericString(constraint.ConstraintName);
                var fkColumns = constraint.SourceColumns.Select(c => c.ToString());
                var fkReferencedColumns = constraint.ReferencedColumns.Select(c => c.ToString());
                sql.AppendLine(
                    $", CONSTRAINT {fkName} FOREIGN KEY ({string.Join(", ", fkColumns)}) REFERENCES {ToAlphaNumericString(constraint.ReferencedTableName)} ({string.Join(", ", fkReferencedColumns)})"
                );
                sql.AppendLine($" ON DELETE {constraint.OnDelete.ToSql()}");
                sql.AppendLine($" ON UPDATE {constraint.OnUpdate.ToSql()}");
            }
        }

        // add unique constraints
        if (uniqueConstraints != null && uniqueConstraints.Length > 0)
        {
            foreach (var constraint in uniqueConstraints)
            {
                var uniqueConstraintName = ToAlphaNumericString(constraint.ConstraintName);
                var uniqueColumns = constraint.Columns.Select(c => c.ToString());
                sql.AppendLine(
                    $", CONSTRAINT {uniqueConstraintName} UNIQUE ({string.Join(", ", uniqueColumns)})"
                );
            }
        }

        sql.AppendLine(")");
        var createTableSql = sql.ToString();

        Logger.LogInformation(
            "Generated table SQL: \n{sql}\n for table '{tableName}'",
            createTableSql,
            tableName
        );

        await ExecuteAsync(db, createTableSql, transaction: tx).ConfigureAwait(false);

        var combinedIndexes = (indexes ?? []).Union(fillWithAdditionalIndexesToCreate).ToList();

        foreach (var index in combinedIndexes)
        {
            await CreateIndexIfNotExistsAsync(db, index, tx, cancellationToken)
                .ConfigureAwait(false);
            // var indexName = NormalizeName(index.IndexName);
            // var indexColumns = index.Columns.Select(c => c.ToString());
            // var indexColumnNames = index.Columns.Select(c => c.ColumnName);
            // // create index sql
            // var createIndexSql =
            //     $"CREATE {(index.IsUnique ? "UNIQUE INDEX" : "INDEX")} ix_{tableName}_{string.Join('_', indexColumnNames)} ON {tableName} ({string.Join(", ", indexColumns)})";
            // await ExecuteAsync(db, createIndexSql, transaction: tx).ConfigureAwait(false);
        }

        return true;
    }

    public override async Task<List<string>> GetTableNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : $"{ToAlphaNumericString(tableNameFilter)}".Replace("*", "%");

        var sql = new StringBuilder();
        sql.AppendLine(
            "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'"
        );
        if (!string.IsNullOrWhiteSpace(where))
            sql.AppendLine(" AND name LIKE @where");
        sql.AppendLine("ORDER BY name");

        return await QueryAsync<string>(db, sql.ToString(), new { where }, transaction: tx)
            .ConfigureAwait(false);
    }

    public override async Task<List<DxTable>> GetTablesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : $"{ToAlphaNumericString(tableNameFilter)}".Replace("*", "%");

        var sql = new StringBuilder();
        sql.AppendLine(
            "SELECT name as table_name, sql as table_sql FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'"
        );
        if (!string.IsNullOrWhiteSpace(where))
            sql.AppendLine(" AND name LIKE @where");
        sql.AppendLine("ORDER BY name");

        var results = await QueryAsync<(string table_name, string table_sql)>(
                db,
                sql.ToString(),
                new { where },
                transaction: tx
            )
            .ConfigureAwait(false);

        var tables = new List<DxTable>();
        foreach (var result in results)
        {
            var table = SqliteSqlParser.ParseCreateTableStatement(result.table_sql);
            if (table == null)
                continue;
            tables.Add(table);
        }

        // attach indexes
        var whereStatement =
            (tables.Count > 0 && tables.Count < 15) ? " AND m.name IN @tableNames" : "";
        var whereParams = new
        {
            tableNames = (tables.Count > 0 && tables.Count < 15)
                ? tables.Select(t => t.TableName).ToArray()
                : []
        };

        var indexes = await GetIndexesInternalAsync(
                db,
                schemaName,
                whereStatement,
                whereParams,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (indexes.Count > 0)
        {
            foreach (var table in tables)
            {
                table.Indexes = indexes
                    .Where(i =>
                        i.TableName.Equals(table.TableName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
                if (table.Indexes.Count > 0)
                {
                    foreach (var column in table.Columns)
                    {
                        column.IsIndexed = table.Indexes.Any(i =>
                            i.Columns.Any(c =>
                                c.ColumnName.Equals(
                                    column.ColumnName,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        );
                        if (column.IsIndexed && !column.IsUnique)
                        {
                            column.IsUnique = table
                                .Indexes.Where(i => i.IsUnique)
                                .Any(i =>
                                    i.Columns.Any(c =>
                                        c.ColumnName.Equals(
                                            column.ColumnName,
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                    )
                                );
                        }
                    }
                }
            }
        }

        return tables;
    }

    public override async Task<bool> TruncateTableIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !(
                await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                    .ConfigureAwait(false)
            )
        )
            return false;

        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        // in SQLite, you could either delete all the records and reset the index (this could take a while if it's a big table)
        // - DELETE FROM table_name;
        // - DELETE FROM sqlite_sequence WHERE name = 'table_name';

        // or just drop the table (this is faster) and recreate it
        var createTableSql = await ExecuteScalarAsync<string>(
                db,
                $"select sql FROM sqlite_master WHERE type = 'table' AND name = @tableName",
                new { tableName },
                transaction: tx
            )
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(createTableSql))
            return false;

        await DropTableIfExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
        await ExecuteAsync(db, createTableSql, transaction: tx).ConfigureAwait(false);
        return true;
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

        // create a temporary table with the updated schema
        var tmpTable = new DxTable(
            table.SchemaName,
            table.TableName,
            [.. table.Columns],
            table.PrimaryKeyConstraint,
            [.. table.CheckConstraints],
            [.. table.DefaultConstraints],
            [.. table.UniqueConstraints],
            [.. table.ForeignKeyConstraints],
            [.. table.Indexes]
        );
        var newTable = updateTable(tmpTable);

        await AlterTableUsingRecreateTableStrategyAsync(
            db,
            schemaName,
            table,
            newTable,
            tx,
            cancellationToken
        );

        return true;
    }

    private async Task AlterTableUsingRecreateTableStrategyAsync(
        IDbConnection db,
        string? schemaName,
        DxTable existingTable,
        DxTable updatedTable,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var tableName = existingTable.TableName;
        var tempTableName = $"{tableName}_temp";
        // updatedTable.TableName = newTableName;

        // get the create index sql statements for the existing table
        // var createIndexStatements = await GetCreateIndexSqlStatementsForTable(
        //         db,
        //         schemaName,
        //         tableName,
        //         tx,
        //         cancellationToken
        //     )
        //     .ConfigureAwait(false);

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
            // create a temporary table from the existing table's data
            await ExecuteAsync(
                    db,
                    $@"CREATE TEMP TABLE {tempTableName} AS SELECT * FROM {tableName}",
                    transaction: innerTx
                )
                .ConfigureAwait(false);

            // drop the old table
            await ExecuteAsync(db, $@"DROP TABLE {tableName}", transaction: innerTx)
                .ConfigureAwait(false);

            var created = await CreateTableIfNotExistsAsync(
                    db,
                    updatedTable,
                    innerTx,
                    cancellationToken
                )
                .ConfigureAwait(false);

            if (created)
            {
                // populate the new table with the data from the old table
                var previousColumnNames = existingTable.Columns.Select(c => c.ColumnName);

                // make sure to only copy columns that exist in both tables
                var columnNamesInBothTables = previousColumnNames.Where(c =>
                    updatedTable.Columns.Any(x =>
                        x.ColumnName.Equals(c, StringComparison.OrdinalIgnoreCase)
                    )
                );

                if (columnNamesInBothTables.Count() > 0)
                {
                    var columnsToCopyString = string.Join(", ", columnNamesInBothTables);
                    await ExecuteAsync(
                            db,
                            $@"INSERT INTO {updatedTable.TableName} ({columnsToCopyString}) SELECT {columnsToCopyString} FROM {tempTableName}",
                            transaction: innerTx
                        )
                        .ConfigureAwait(false);
                }

                // drop the temp table
                await ExecuteAsync(db, $@"DROP TABLE {tempTableName}", transaction: innerTx)
                    .ConfigureAwait(false);

                // // drop the old table
                // await ExecuteAsync(db, $@"DROP TABLE {tableName}", transaction: innerTx)
                //     .ConfigureAwait(false);

                // rename the new table to the old table name
                // await ExecuteAsync(
                //         db,
                //         $@"ALTER TABLE {updatedTable.TableName} RENAME TO {tableName}",
                //         transaction: innerTx
                //     )
                //     .ConfigureAwait(false);

                // add back the indexes to the new table
                // foreach (var createIndexStatement in createIndexStatements)
                // {
                //     await ExecuteAsync(db, createIndexStatement, null, transaction: innerTx)
                //         .ConfigureAwait(false);
                // }

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
