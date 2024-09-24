using System.Data;
using System.Text;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    public override async Task<bool> TableExistsAsync(
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
        if (await TableExistsAsync(db, schemaName, tableName, tx, cancellationToken))
            return false;

        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        var sql = new StringBuilder();

        sql.AppendLine($"CREATE TABLE {ToAlphaNumericString(tableName)} (");
        var columnDefinitionClauses = new List<string>();
        for (var i = 0; i < columns?.Length; i++)
        {
            var column = columns[i];
            var columnName = ToAlphaNumericString(column.ColumnName);
            var columnType = string.IsNullOrWhiteSpace(column.ProviderDataType)
                ? GetSqlTypeString(column.DotnetType, column.Length, column.Precision, column.Scale)
                : column.ProviderDataType;
            var columnSql = $"{columnName} {columnType}";
            if (column.IsNullable)
                columnSql += " NULL";
            else
                columnSql += " NOT NULL";
            if (primaryKey == null && column.IsPrimaryKey)
            {
                columnSql += $" CONSTRAINT pk_{tableName}_{columnName}  PRIMARY KEY";
                if (column.IsAutoIncrement)
                    columnSql += " AUTOINCREMENT";
            }
            if ((uniqueConstraints == null || uniqueConstraints.Length == 0) && column.IsUnique)
            {
                columnSql += $" CONSTRAINT uc_{tableName}_{columnName}  UNIQUE";
            }
            if (
                (defaultConstraints == null || defaultConstraints.Length == 0)
                && !string.IsNullOrWhiteSpace(column.DefaultExpression)
            )
            {
                columnSql +=
                    $" CONSTRAINT df_{tableName}_{columnName} DEFAULT {(column.DefaultExpression.Contains(' ') ? $"({column.DefaultExpression})" : column.DefaultExpression)}";
            }
            else if (defaultConstraints != null && defaultConstraints.Length > 0)
            {
                foreach (var constraint in defaultConstraints)
                {
                    if (
                        string.IsNullOrWhiteSpace(constraint.ColumnName)
                        || !constraint.ColumnName.Equals(
                            columnName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                        continue;

                    columnSql +=
                        $" CONSTRAINT {ToAlphaNumericString(constraint.ConstraintName)} DEFAULT {(constraint.Expression.Contains(' ') ? $"({constraint.Expression})" : constraint.Expression)}";
                }
            }
            if (
                (checkConstraints == null || checkConstraints.Length == 0)
                && !string.IsNullOrWhiteSpace(column.CheckExpression)
            )
            {
                columnSql +=
                    $" CONSTRAINT cf_{tableName}_{columnName} CHECK ({column.CheckExpression})";
            }
            if (
                (foreignKeyConstraints == null || foreignKeyConstraints.Length == 0)
                && column.IsForeignKey
                && !string.IsNullOrWhiteSpace(column.ReferencedTableName)
                && !string.IsNullOrWhiteSpace(column.ReferencedColumnName)
            )
            {
                columnSql +=
                    $" CONSTRAINT fk_{tableName}_{columnName}_{column.ReferencedTableName}_{column.ReferencedColumnName} FOREIGN KEY ({columnName}) REFERENCES {ToAlphaNumericString(column.ReferencedTableName)} ({ToAlphaNumericString(column.ReferencedColumnName)})";
                if (column.OnDelete.HasValue)
                    columnSql +=
                        $" ON DELETE {(column.OnDelete ?? DxForeignKeyAction.NoAction).ToSql()}";
                if (column.OnUpdate.HasValue)
                    columnSql +=
                        $" ON UPDATE {(column.OnUpdate ?? DxForeignKeyAction.NoAction).ToSql()}";
            }
            columnDefinitionClauses.Add(columnSql);
        }
        sql.AppendLine(string.Join(", ", columnDefinitionClauses));
        if (primaryKey != null)
        {
            var pkColumns = primaryKey.Columns.Select(c => c.ToString());
            sql.AppendLine(
                $", CONSTRAINT pk_{tableName} PRIMARY KEY ({string.Join(", ", pkColumns)})"
            );
        }
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
        await ExecuteAsync(db, createTableSql, transaction: tx).ConfigureAwait(false);

        if (indexes != null && indexes.Length > 0)
        {
            foreach (var index in indexes)
            {
                var indexName = ToAlphaNumericString(index.IndexName);
                var indexColumns = index.Columns.Select(c => c.ToString());
                // create index sql
                var createIndexSql =
                    $"CREATE {(index.IsUnique ? "UNIQUE INDEX" : "INDEX")} ix_{tableName}_{indexName} ON {tableName} ({string.Join(", ", indexColumns)})";
                await ExecuteAsync(db, createIndexSql, transaction: tx).ConfigureAwait(false);
            }
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
                await TableExistsAsync(db, schemaName, tableName, tx, cancellationToken)
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
}
