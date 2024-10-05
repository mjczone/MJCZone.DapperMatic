using System.Data;
using System.Text;
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

        var sql =
            $@"
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = @schemaName
            AND TABLE_NAME = @tableName            
            ";

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
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        if (await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken))
            return false;

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var fillWithAdditionalIndexesToCreate = new List<DxIndex>();

        var sql = new StringBuilder();
        sql.Append($"CREATE TABLE {GetSchemaQualifiedTableName(schemaName, tableName)} (");
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
            var pkColumns = primaryKey.Columns.Select(c =>
                c.ToString(SupportsOrderedKeysInConstraints)
            );
            var pkColumnNames = primaryKey.Columns.Select(c => c.ColumnName);
            sql.AppendLine(
                $", CONSTRAINT {ProviderUtils.GetPrimaryKeyConstraintName(tableName, [.. pkColumnNames])} PRIMARY KEY ({string.Join(", ", pkColumns)})"
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
        }

        // add unique constraints
        if (uniqueConstraints != null && uniqueConstraints.Length > 0)
        {
            foreach (var constraint in uniqueConstraints)
            {
                var uniqueColumns = constraint.Columns.Select(c =>
                    c.ToString(SupportsOrderedKeysInConstraints)
                );
                sql.AppendLine(
                    $", CONSTRAINT {NormalizeName(constraint.ConstraintName)} UNIQUE ({string.Join(", ", uniqueColumns)})"
                );
            }
        }

        sql.AppendLine(")");
        var createTableSql = sql.ToString();

        await ExecuteAsync(db, createTableSql, transaction: tx).ConfigureAwait(false);

        var combinedIndexes = (indexes ?? []).Union(fillWithAdditionalIndexesToCreate).ToList();

        foreach (var index in combinedIndexes)
        {
            await CreateIndexIfNotExistsAsync(db, index, tx, cancellationToken)
                .ConfigureAwait(false);
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
        schemaName = NormalizeSchemaName(schemaName);

        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToLikeString(tableNameFilter);

        return await QueryAsync<string>(
                db,
                $@"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND TABLE_NAME LIKE @where")}
                ORDER BY TABLE_NAME",
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
                    tc.constraint_name;
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
            .Where(t => !string.IsNullOrWhiteSpace(t.column_default))
            .Select(c =>
            {
                return new DxDefaultConstraint(
                    DefaultSchema,
                    c.table_name,
                    c.column_name,
                    ProviderUtils.GetDefaultConstraintName(c.table_name, c.column_name),
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
                    ProviderUtils.GetPrimaryKeyConstraintName(t.table_name, columnNames),
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
            SELECT
                kfk.TABLE_SCHEMA schema_name,
                kfk.TABLE_NAME table_name,
                kfk.COLUMN_NAME AS column_name,
                rc.CONSTRAINT_NAME AS constraint_name,
                kpk.TABLE_SCHEMA AS referenced_schema_name,
                kpk.TABLE_NAME AS referenced_table_name,
                kpk.COLUMN_NAME AS referenced_column_name,
                rc.UPDATE_RULE update_rule,
                rc.DELETE_RULE delete_rule
            FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kfk ON rc.CONSTRAINT_NAME = kfk.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kpk ON rc.UNIQUE_CONSTRAINT_NAME = kpk.CONSTRAINT_NAME
            WHERE 
                kfk.TABLE_SCHEMA = DATABASE()
                {(string.IsNullOrWhiteSpace(where) ? null : " AND kfk.TABLE_NAME LIKE @where")}
            ORDER BY kfk.TABLE_SCHEMA, kfk.TABLE_NAME, rc.CONSTRAINT_NAME
        ";
        var foreignKeyResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string column_name,
            string constraint_name,
            string referenced_schema_name,
            string referenced_table_name,
            string referenced_column_name,
            string update_rule,
            string delete_rule
        )>(db, foreignKeysSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);
        var allForeignKeyConstraints = foreignKeyResults
            .GroupBy(t => new
            {
                t.schema_name,
                t.table_name,
                t.constraint_name,
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
                {(string.IsNullOrWhiteSpace(where) ? null : " AND t.[name] LIKE @where")}
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
            .Where(t => !string.IsNullOrWhiteSpace(t.column_name))
            .Select(t =>
            {
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
                schemaName,
                tableNameFilter,
                tx,
                cancellationToken
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
