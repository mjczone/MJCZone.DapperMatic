using System.Data;
using System.Data.Common;
using System.Text;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
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
                $", CONSTRAINT {ProviderUtils.GeneratePrimaryKeyConstraintName(tableName, [.. pkColumnNames])} PRIMARY KEY ({string.Join(", ", pkColumns)})"
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
                sql.AppendLine(
                    $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} CHECK ({constraint.Expression})"
                );
            }
        }

        // add foreign key constraints
        if (foreignKeyConstraints != null && foreignKeyConstraints.Length > 0)
        {
            foreach (var constraint in foreignKeyConstraints)
            {
                var fkColumns = constraint.SourceColumns.Select(c => c.ToString());
                var fkReferencedColumns = constraint.ReferencedColumns.Select(c => c.ToString());
                sql.AppendLine(
                    $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} FOREIGN KEY ({string.Join(", ", fkColumns)}) REFERENCES {NormalizeName(constraint.ReferencedTableName)} ({string.Join(", ", fkReferencedColumns)})"
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
                var uniqueColumns = constraint.Columns.Select(c => c.ToString());
                sql.AppendLine(
                    $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} UNIQUE ({string.Join(", ", uniqueColumns)})"
                );
            }
        }

        sql.AppendLine(")");
        var createTableSql = sql.ToString();

        await ExecuteAsync(db, createTableSql, tx: tx).ConfigureAwait(false);

        var combinedIndexes = (indexes ?? []).Union(fillWithAdditionalIndexesToCreate).ToList();

        foreach (var index in combinedIndexes)
        {
            await CreateIndexIfNotExistsAsync(db, index, tx, cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
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
            : ToLikeString(tableNameFilter);

        var sql = new StringBuilder();
        sql.AppendLine(
            @"SELECT name as table_name, sql as table_sql 
                FROM sqlite_master 
                WHERE type = 'table' AND name NOT LIKE 'sqlite_%'"
        );
        if (!string.IsNullOrWhiteSpace(where))
            sql.AppendLine(" AND name LIKE @where");
        sql.AppendLine("ORDER BY name");

        var results = await QueryAsync<(string table_name, string table_sql)>(
                db,
                sql.ToString(),
                new { where },
                tx: tx
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

        // attach indexes to tables
        var indexes = await GetIndexesInternalAsync(
                db,
                schemaName,
                tableNameFilter,
                null,
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
            !await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false)
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
                tx: tx
            )
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(createTableSql))
            return false;

        await DropTableIfExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        await ExecuteAsync(db, createTableSql, tx: tx).ConfigureAwait(false);
        
        return true;
    }

    protected override async Task<List<DxIndex>> GetIndexesInternalAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter,
        string? indexNameFilter,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var whereTableLike = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToLikeString(tableNameFilter);
        var whereIndexLike = string.IsNullOrWhiteSpace(indexNameFilter)
            ? null
            : ToLikeString(indexNameFilter);

        var sql =
            $@"
                SELECT DISTINCT 
                    m.name AS table_name, 
                    il.name AS index_name,
                    il.""unique"" AS is_unique,	
                    ii.name AS column_name,
                    ii.DESC AS is_descending
                FROM sqlite_schema AS m,
                    pragma_index_list(m.name) AS il,
                    pragma_index_xinfo(il.name) AS ii
                WHERE m.type='table' 
                    and ii.name IS NOT NULL 
                    AND il.origin = 'c'
                    {(string.IsNullOrWhiteSpace(whereTableLike) ? "" : " AND m.name LIKE @whereTableLike")}
                    {(string.IsNullOrWhiteSpace(whereIndexLike) ? "" : " AND il.name LIKE @whereIndexLike")}                
                ORDER BY m.name, il.name, ii.seqno";

        var results = await QueryAsync<(
            string table_name,
            string index_name,
            bool is_unique,
            string column_name,
            bool is_descending
        )>(db, sql, new { whereTableLike, whereIndexLike }, tx: tx)
            .ConfigureAwait(false);

        var indexes = new List<DxIndex>();

        foreach (
            var group in results.GroupBy(r => new
            {
                r.table_name,
                r.index_name,
                r.is_unique
            })
        )
        {
            var index = new DxIndex
            {
                SchemaName = null,
                TableName = group.Key.table_name,
                IndexName = group.Key.index_name,
                IsUnique = group.Key.is_unique,
                Columns = group
                    .Select(r => new DxOrderedColumn(
                        r.column_name,
                        r.is_descending ? DxColumnOrder.Descending : DxColumnOrder.Ascending
                    ))
                    .ToArray()
            };
            indexes.Add(index);
        }

        return indexes;
    }

    // private async Task<List<string>> GetCreateIndexSqlStatementsForTable(
    //     IDbConnection db,
    //     string? schemaName,
    //     string tableName,
    //     IDbTransaction? tx = null,
    //     CancellationToken cancellationToken = default
    // )
    // {
    //     var getSqlCreateIndexStatements =
    //         @"
    //             SELECT DISTINCT
    //                 m.sql
    //             FROM sqlite_schema AS m,
    //                 pragma_index_list(m.name) AS il,
    //                 pragma_index_xinfo(il.name) AS ii
    //             WHERE m.type='table'
    //                 AND ii.name IS NOT NULL
    //                 AND il.origin = 'c'
    //                 AND m.name = @tableName
    //                 AND m.sql IS NOT NULL
    //              ORDER BY m.name, il.name, ii.seqno
    //     ";
    //     return (
    //         await QueryAsync<string>(db, getSqlCreateIndexStatements, new { tableName }, tx: tx)
    //             .ConfigureAwait(false)
    //     )
    //         .Select(sql =>
    //         {
    //             return sql.Contains("IF NOT EXISTS", StringComparison.OrdinalIgnoreCase)
    //                 ? sql
    //                 : sql.Replace(
    //                         "CREATE INDEX",
    //                         "CREATE INDEX IF NOT EXISTS",
    //                         StringComparison.OrdinalIgnoreCase
    //                     )
    //                     .Replace(
    //                         "CREATE UNIQUE INDEX",
    //                         "CREATE UNIQUE INDEX IF NOT EXISTS",
    //                         StringComparison.OrdinalIgnoreCase
    //                     )
    //                     .Trim();
    //         })
    //         .ToList();
    // }

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
                    tx: innerTx
                )
                .ConfigureAwait(false);

            // drop the old table
            await ExecuteAsync(db, $@"DROP TABLE {tableName}", tx: innerTx).ConfigureAwait(false);

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
                            tx: innerTx
                        )
                        .ConfigureAwait(false);
                }

                // drop the temp table
                await ExecuteAsync(db, $@"DROP TABLE {tempTableName}", tx: innerTx)
                    .ConfigureAwait(false);

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
