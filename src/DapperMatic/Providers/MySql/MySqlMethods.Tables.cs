using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override async Task<bool> DoesTableExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var sql = $@"
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE' 
                and TABLE_SCHEMA = DATABASE()
                and TABLE_NAME = @tableName
            ".Trim();

        var result = await ExecuteScalarAsync<int>(
                db,
                sql,
                new { schemaName, tableName },
                transaction: tx
            )
            .ConfigureAwait(false);

        return result > 0;
    }

    public override async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        DxTable table,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(table.TableName))
        {
            throw new ArgumentException("Table name is required.", nameof(table));
        }

        if (await DoesTableExistAsync(db, table.SchemaName, table.TableName, tx, cancellationToken))
            return false;

        var (schemaName, tableName, _) = NormalizeNames(table.SchemaName, table.TableName);

        var fillWithAdditionalIndexesToCreate = new List<DxIndex>();

        var tableWithChanges = new DxTable(schemaName, tableName);

        var sql = new StringBuilder();
        sql.Append($"CREATE TABLE {GetSchemaQualifiedTableName(schemaName, tableName)} (");
        var columnDefinitionClauses = new List<string>();
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];

            var colSql = BuildColumnDefinitionSql(
                table,
                tableWithChanges,
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
                column.OnUpdate
            );

            columnDefinitionClauses.Add(colSql.ToString());
        }
        sql.AppendLine(string.Join(", ", columnDefinitionClauses));

        // add single column primary key constraints as column definitions; and,
        // add multi column primary key constraints here
        var primaryKey = table.PrimaryKeyConstraint ?? tableWithChanges.PrimaryKeyConstraint;
        if (primaryKey != null && primaryKey.Columns.Length > 0)
        {
            var pkColumns = primaryKey.Columns.Select(c =>
                c.ToString(SupportsOrderedKeysInConstraints)
            );
            var pkColumnNames = primaryKey.Columns.Select(c => c.ColumnName);
            var primaryKeyConstraintName = !string.IsNullOrWhiteSpace(primaryKey.ConstraintName)
                ? primaryKey.ConstraintName
                : ProviderUtils.GeneratePrimaryKeyConstraintName(tableName, [.. pkColumnNames]);
            sql.AppendLine(
                $", CONSTRAINT {primaryKeyConstraintName} PRIMARY KEY ({string.Join(", ", pkColumns)})"
            );
        }

        // add check constraints
        var checkConstraints = table.CheckConstraints.Union(tableWithChanges.CheckConstraints);
        foreach (
            var constraint in checkConstraints.Where(c => !string.IsNullOrWhiteSpace(c.Expression))
        )
        {
            sql.AppendLine(
                $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} CHECK ({constraint.Expression})"
            );
        }

        // add foreign key constraints
        var foreignKeyConstraints = table.ForeignKeyConstraints.Union(
            tableWithChanges.ForeignKeyConstraints
        );
        foreach (var constraint in foreignKeyConstraints)
        {
            var fkColumns = constraint.SourceColumns.Select(c =>
                c.ToString(SupportsOrderedKeysInConstraints)
            );
            var fkReferencedColumns = constraint.ReferencedColumns.Select(c =>
                c.ToString(SupportsOrderedKeysInConstraints)
            );
            sql.AppendLine(
                $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} FOREIGN KEY ({string.Join(", ", fkColumns)}) REFERENCES {NormalizeName(constraint.ReferencedTableName)} ({string.Join(", ", fkReferencedColumns)})"
            );
            sql.AppendLine($" ON DELETE {constraint.OnDelete.ToSql()}");
            sql.AppendLine($" ON UPDATE {constraint.OnUpdate.ToSql()}");
        }

        // add unique constraints
        var uniqueConstraints = table.UniqueConstraints.Union(tableWithChanges.UniqueConstraints);
        foreach (var constraint in uniqueConstraints)
        {
            var uniqueColumns = constraint.Columns.Select(c =>
                c.ToString(SupportsOrderedKeysInConstraints)
            );
            sql.AppendLine(
                $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} UNIQUE ({string.Join(", ", uniqueColumns)})"
            );
        }

        sql.AppendLine(")");
        var createTableSql = sql.ToString();

        await ExecuteAsync(db, createTableSql, transaction: tx).ConfigureAwait(false);

        var indexes = table.Indexes.Union(tableWithChanges.Indexes).ToArray();
        foreach (var index in indexes)
        {
            await CreateIndexIfNotExistsAsync(db, index, tx, cancellationToken)
                .ConfigureAwait(false);
        }

        return true;
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
        return await CreateTableIfNotExistsAsync(
            db,
            new DxTable(
                schemaName,
                tableName,
                columns,
                primaryKey,
                checkConstraints,
                defaultConstraints,
                uniqueConstraints,
                foreignKeyConstraints,
                indexes
            ),
            tx: tx,
            cancellationToken: cancellationToken
        );
    }

    public override async Task<List<string>> GetTableNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToLikeString(tableNameFilter);

        return await QueryAsync<string>(
                db,
                $@"
                SELECT t.TABLE_NAME as table_name
                FROM INFORMATION_SCHEMA.TABLES as t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                    AND t.TABLE_SCHEMA = DATABASE()
                    {(string.IsNullOrWhiteSpace(where) ? null : " AND t.TABLE_NAME LIKE @where")}
                ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME",
                new { schemaName, where },
                transaction: tx
            )
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
        schemaName = NormalizeSchemaName(schemaName);

        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToLikeString(tableNameFilter);

        // columns
        var columnsSql =
            @$"
            SELECT
                t.TABLE_SCHEMA AS schema_name,
                t.TABLE_NAME AS table_name,
                c.COLUMN_NAME AS column_name,
                t.TABLE_COLLATION AS table_collation,
                c.ORDINAL_POSITION AS column_ordinal,
                c.COLUMN_DEFAULT AS column_default,
                case when (c.COLUMN_KEY = 'PRI') then 1 else 0 end AS is_primary_key,
                case 
                    when (c.COLUMN_KEY = 'UNI') then 1 else 0 end AS is_unique,
                case 
                    when (c.COLUMN_KEY = 'UNI') then 1 
                    when (c.COLUMN_KEY = 'MUL') then 1 
                    else 0 
                end AS is_indexed,
                case when (c.IS_NULLABLE = 'YES') then 1 else 0 end AS is_nullable,
                c.DATA_TYPE AS data_type,
                c.COLUMN_TYPE AS data_type_complete,
                c.CHARACTER_MAXIMUM_LENGTH AS max_length,
                c.NUMERIC_PRECISION AS numeric_precision,
                c.NUMERIC_SCALE AS numeric_scale,
                c.EXTRA as extra
            FROM INFORMATION_SCHEMA.TABLES t
                LEFT OUTER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_SCHEMA = c.TABLE_SCHEMA and t.TABLE_NAME = c.TABLE_NAME
            WHERE t.TABLE_TYPE = 'BASE TABLE'
                AND t.TABLE_SCHEMA = DATABASE()
                {(string.IsNullOrWhiteSpace(where) ? null : " AND t.TABLE_NAME LIKE @where")}
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION
        ";
        var columnResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string column_name,
            string table_collation,
            int column_ordinal,
            string column_default,
            bool is_primary_key,
            bool is_unique,
            bool is_indexed,
            bool is_nullable,
            string data_type,
            string data_type_complete,
            int? max_length,
            int? numeric_precision,
            int? numeric_scale,
            string? extra
        )>(db, columnsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

        // get primary key, unique key in a single query
        var constraintsSql =
            @$"
                SELECT
                    tc.table_schema AS schema_name,
                    tc.table_name AS table_name,
                    tc.constraint_type AS constraint_type,
                    tc.constraint_name AS constraint_name,
                    GROUP_CONCAT(kcu.column_name ORDER BY kcu.ordinal_position ASC SEPARATOR ', ') AS columns_csv,
                    GROUP_CONCAT(CASE isc.collation
                                WHEN 'A' THEN 'ASC'
                                WHEN 'D' THEN 'DESC'
                                ELSE 'ASC'
                                END ORDER BY kcu.ordinal_position ASC SEPARATOR ', ') AS columns_desc_csv
                FROM
                    information_schema.table_constraints tc
                JOIN
                    information_schema.key_column_usage kcu
                    ON tc.constraint_name = kcu.constraint_name
                    AND tc.table_schema = kcu.table_schema
                    AND tc.table_name = kcu.table_name
                LEFT JOIN
                    information_schema.statistics isc
                    ON kcu.table_schema = isc.table_schema
                    AND kcu.table_name = isc.table_name
                    AND kcu.column_name = isc.column_name
                    AND kcu.constraint_name = isc.index_name
                WHERE
                    tc.table_schema = DATABASE()
                    and tc.constraint_type in ('UNIQUE', 'PRIMARY KEY')
                    {(string.IsNullOrWhiteSpace(where) ? null : " AND tc.table_name LIKE @where")}
                GROUP BY
                    tc.table_name,
                    tc.constraint_type,
                    tc.constraint_name
                ORDER BY
                    tc.table_name,
                    tc.constraint_type,
                    tc.constraint_name
        ";
        var constraintResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string constraint_type,
            string constraint_name,
            string columns_csv,
            string columns_desc_csv
        )>(db, constraintsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

        var allDefaultConstraints = columnResults
            .Where(t =>
                !string.IsNullOrWhiteSpace(t.column_default)
                &&
                // MariaDB adds NULL as a default constraint, let's ignore it
                !t.column_default.Equals("NULL", StringComparison.OrdinalIgnoreCase)
            )
            .Select(c =>
            {
                return new DxDefaultConstraint(
                    DefaultSchema,
                    c.table_name,
                    c.column_name,
                    ProviderUtils.GenerateDefaultConstraintName(c.table_name, c.column_name),
                    c.column_default.Trim(['(', ')'])
                );
            })
            .ToArray();

        var allPrimaryKeyConstraints = constraintResults
            .Where(t => t.constraint_type == "PRIMARY KEY")
            .Select(t =>
            {
                var columnNames = t.columns_csv.Split(", ");
                var columnDescs = t.columns_desc_csv.Split(", ");
                return new DxPrimaryKeyConstraint(
                    DefaultSchema,
                    t.table_name,
                    ProviderUtils.GeneratePrimaryKeyConstraintName(t.table_name, columnNames),
                    columnNames
                        .Select(
                            (c, i) =>
                                new DxOrderedColumn(
                                    c,
                                    columnDescs[i]
                                        .Equals("DESC", StringComparison.OrdinalIgnoreCase)
                                        ? DxColumnOrder.Descending
                                        : DxColumnOrder.Ascending
                                )
                        )
                        .ToArray()
                );
            })
            .ToArray();
        var allUniqueConstraints = constraintResults
            .Where(t => t.constraint_type == "UNIQUE")
            .Select(t =>
            {
                var columnNames = t.columns_csv.Split(", ");
                var columnDescs = t.columns_desc_csv.Split(", ");
                return new DxUniqueConstraint(
                    DefaultSchema,
                    t.table_name,
                    t.constraint_name,
                    columnNames
                        .Select(
                            (c, i) =>
                                new DxOrderedColumn(
                                    c,
                                    columnDescs[i]
                                        .Equals("DESC", StringComparison.OrdinalIgnoreCase)
                                        ? DxColumnOrder.Descending
                                        : DxColumnOrder.Ascending
                                )
                        )
                        .ToArray()
                );
            })
            .ToArray();

        var foreignKeysSql =
            @$"
            select distinct
                kcu.TABLE_SCHEMA as schema_name, 
                kcu.TABLE_NAME as table_name, 
                kcu.CONSTRAINT_NAME as constraint_name,
                kcu.REFERENCED_TABLE_SCHEMA as referenced_schema_name,
                kcu.REFERENCED_TABLE_NAME as referenced_table_name,
                rc.DELETE_RULE as delete_rule,
                rc.UPDATE_RULE as update_rule,
                kcu.ORDINAL_POSITION as key_ordinal,
                kcu.COLUMN_NAME as column_name,
                kcu.REFERENCED_COLUMN_NAME as referenced_column_name
            from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc on kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc on kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
            where kcu.CONSTRAINT_SCHEMA = DATABASE()
                and tc.CONSTRAINT_SCHEMA = DATABASE()
                and tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
                {(string.IsNullOrWhiteSpace(where) ? null : " AND kcu.TABLE_NAME LIKE @where")}
            order by schema_name, table_name, key_ordinal
        ";
        var foreignKeyResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string constraint_name,
            string referenced_schema_name,
            string referenced_table_name,
            string delete_rule,
            string update_rule,
            string key_ordinal,
            string column_name,
            string referenced_column_name
        )>(db, foreignKeysSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);
        var allForeignKeyConstraints = foreignKeyResults
            .GroupBy(t => new
            {
                t.schema_name,
                t.table_name,
                t.constraint_name,
                t.referenced_schema_name,
                t.referenced_table_name,
                t.update_rule,
                t.delete_rule
            })
            .Select(gb =>
            {
                return new DxForeignKeyConstraint(
                    DefaultSchema,
                    gb.Key.table_name,
                    gb.Key.constraint_name,
                    gb.Select(c => new DxOrderedColumn(c.column_name)).ToArray(),
                    gb.Key.referenced_table_name,
                    gb.Select(c => new DxOrderedColumn(c.referenced_column_name)).ToArray(),
                    gb.Key.delete_rule.ToForeignKeyAction(),
                    gb.Key.update_rule.ToForeignKeyAction()
                );
            })
            .ToArray();

        // the table CHECK_CONSTRAINTS only exists starting MySQL 8.0.16 and MariaDB 10.2.1
        // resolve issue for MySQL 5.0.12+
        var checkConstraintsSql =
            @$"
            SELECT 
                tc.TABLE_SCHEMA as schema_name,
                tc.TABLE_NAME as table_name, 
                kcu.COLUMN_NAME as column_name,
                tc.CONSTRAINT_NAME as constraint_name,
                cc.CHECK_CLAUSE AS check_expression
            FROM 
                INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
            JOIN 
                INFORMATION_SCHEMA.CHECK_CONSTRAINTS AS cc
                ON tc.CONSTRAINT_NAME = cc.CONSTRAINT_NAME
            LEFT JOIN 
                INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS kcu
                ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
            WHERE 
                tc.TABLE_SCHEMA = DATABASE()
                and tc.CONSTRAINT_TYPE = 'CHECK'
                {(string.IsNullOrWhiteSpace(where) ? null : " AND tc.TABLE_NAME LIKE @where")}
            order by schema_name, table_name, column_name, constraint_name            
            ";

        var checkConstraintResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string? column_name,
            string constraint_name,
            string check_expression
        )>(db, checkConstraintsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);
        var allCheckConstraints = checkConstraintResults
            .Select(t =>
            {
                if (string.IsNullOrWhiteSpace(t.column_name))
                {
                    // try to associate the check constraint with a column
                    var columnCount = 0;
                    var columnName = "";
                    foreach (var column in columnResults)
                    {
                        string pattern = $@"\b{Regex.Escape(column.column_name)}\b";
                        if (
                            column.table_name.Equals(
                                t.table_name,
                                StringComparison.OrdinalIgnoreCase
                            ) && Regex.IsMatch(t.check_expression, pattern, RegexOptions.IgnoreCase)
                        )
                        {
                            columnName = column.column_name;
                            columnCount++;
                        }
                    }
                    if (columnCount == 1)
                    {
                        t.column_name = columnName;
                    }
                }
                return new DxCheckConstraint(
                    DefaultSchema,
                    t.table_name,
                    t.column_name,
                    t.constraint_name,
                    t.check_expression
                );
            })
            .ToArray();

        var allIndexes = await GetIndexesInternalAsync(
                db,
                tableNameFilter,
                null,
                tx: tx,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        var tables = new List<DxTable>();

        foreach (
            var tableColumns in columnResults.GroupBy(r => new { r.schema_name, r.table_name })
        )
        {
            var tableName = tableColumns.Key.table_name;

            var foreignKeyConstraints = allForeignKeyConstraints
                .Where(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var checkConstraints = allCheckConstraints
                .Where(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var defaultConstraints = allDefaultConstraints
                .Where(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var uniqueConstraints = allUniqueConstraints
                .Where(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var primaryKeyConstraint = allPrimaryKeyConstraints.SingleOrDefault(t =>
                t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
            );
            var indexes = allIndexes
                .Where(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var columns = new List<DxColumn>();
            foreach (var tableColumn in tableColumns)
            {
                var columnIsUniqueViaUniqueConstraintOrIndex =
                    uniqueConstraints.Any(c =>
                        c.Columns.Length == 1
                        && c.Columns.Any(c =>
                            c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    || indexes.Any(i =>
                        i.IsUnique == true
                        && i.Columns.Length == 1
                        && i.Columns.Any(c =>
                            c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    );
                var columnIsPartOfIndex = indexes.Any(i =>
                    i.Columns.Any(c =>
                        c.ColumnName.Equals(
                            tableColumn.column_name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );
                var columnIsForeignKey = foreignKeyConstraints.Any(c =>
                    c.SourceColumns.Any(c =>
                        c.ColumnName.Equals(
                            tableColumn.column_name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );

                var foreignKeyConstraint = foreignKeyConstraints.FirstOrDefault(c =>
                    c.SourceColumns.Any(c =>
                        c.ColumnName.Equals(
                            tableColumn.column_name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );
                var foreignKeyColumnIndex = foreignKeyConstraint
                    ?.SourceColumns.Select((c, i) => new { c, i })
                    .FirstOrDefault(c =>
                        c.c.ColumnName.Equals(
                            tableColumn.column_name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    ?.i;

                var column = new DxColumn(
                    tableColumn.schema_name,
                    tableColumn.table_name,
                    tableColumn.column_name,
                    GetDotnetTypeFromSqlType(tableColumn.data_type),
                    tableColumn.data_type,
                    tableColumn.max_length,
                    tableColumn.numeric_precision,
                    tableColumn.numeric_scale,
                    checkConstraints
                        .FirstOrDefault(c =>
                            !string.IsNullOrWhiteSpace(c.ColumnName)
                            && c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        ?.Expression,
                    defaultConstraints
                        .FirstOrDefault(c =>
                            !string.IsNullOrWhiteSpace(c.ColumnName)
                            && c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        ?.Expression,
                    tableColumn.is_nullable,
                    primaryKeyConstraint?.Columns.Any(c =>
                        c.ColumnName.Equals(
                            tableColumn.column_name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ) == true,
                    tableColumn.extra?.Contains(
                        "auto_increment",
                        StringComparison.OrdinalIgnoreCase
                    ) == true,
                    columnIsUniqueViaUniqueConstraintOrIndex,
                    columnIsPartOfIndex,
                    foreignKeyConstraint != null,
                    foreignKeyConstraint?.ReferencedTableName,
                    foreignKeyConstraint
                        ?.ReferencedColumns.ElementAtOrDefault(foreignKeyColumnIndex ?? 0)
                        ?.ColumnName,
                    foreignKeyConstraint?.OnDelete,
                    foreignKeyConstraint?.OnUpdate
                );

                columns.Add(column);
            }

            var table = new DxTable(
                schemaName,
                tableName,
                [.. columns],
                primaryKeyConstraint,
                checkConstraints,
                defaultConstraints,
                uniqueConstraints,
                foreignKeyConstraints,
                indexes
            );
            tables.Add(table);
        }

        return tables;
    }
}
