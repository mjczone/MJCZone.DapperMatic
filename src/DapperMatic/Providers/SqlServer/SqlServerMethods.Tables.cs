using System.Data;
using System.Text;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods
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
        );

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
                c.ORDINAL_POSITION AS column_ordinal,
                c.COLUMN_DEFAULT AS column_default,
                case when (ISNULL(pk.CONSTRAINT_NAME, '') = '') then 0 else 1 end AS is_primary_key,
                pk.CONSTRAINT_NAME AS pk_constraint_name,
                case when (c.IS_NULLABLE = 'YES') then 1 else 0 end AS is_nullable,
                COLUMNPROPERTY(object_id(t.TABLE_SCHEMA+'.'+t.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS is_identity,
                c.DATA_TYPE AS data_type,
                c.CHARACTER_MAXIMUM_LENGTH AS max_length,
                c.NUMERIC_PRECISION AS numeric_precision,
                c.NUMERIC_SCALE AS numeric_scale

            FROM INFORMATION_SCHEMA.TABLES t
                LEFT OUTER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_SCHEMA = c.TABLE_SCHEMA and t.TABLE_NAME = c.TABLE_NAME
                LEFT OUTER JOIN (
                    SELECT tc.TABLE_SCHEMA, tc.TABLE_NAME, ccu.COLUMN_NAME, ccu.CONSTRAINT_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                        INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ccu
                            ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
                    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) pk ON t.TABLE_SCHEMA = pk.TABLE_SCHEMA and t.TABLE_NAME = pk.TABLE_NAME and c.COLUMN_NAME = pk.COLUMN_NAME

            WHERE t.TABLE_TYPE = 'BASE TABLE'
                AND t.TABLE_SCHEMA = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND t.TABLE_NAME LIKE @where")}
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION
        ";
        var columnResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string column_name,
            int column_ordinal,
            string column_default,
            bool is_primary_key,
            string pk_constraint_name,
            bool is_nullable,
            bool is_identity,
            string data_type,
            int? max_length,
            int? numeric_precision,
            int? numeric_scale
        )>(db, columnsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

        // get primary key, unique key, and indexes in a single query
        var constraintsSql =
            @$"
            SELECT sh.name AS schema_name,
                i.name AS constraint_name,
                t.name AS table_name,
                c.name AS column_name,
                ic.key_ordinal AS column_key_ordinal,
                ic.is_descending_key AS is_desc,
                i.is_unique,
                i.is_primary_key,
                i.is_unique_constraint
            FROM sys.indexes i
                INNER JOIN sys.index_columns ic
                    ON i.index_id = ic.index_id AND i.object_id = ic.object_id
                INNER JOIN sys.tables AS t 
                    ON t.object_id = i.object_id
                INNER JOIN sys.columns c
                    ON t.object_id = c.object_id AND ic.column_id = c.column_id
                INNER JOIN sys.objects AS syso 
                    ON syso.object_id = t.object_id AND syso.is_ms_shipped = 0 
                INNER JOIN sys.schemas AS sh
                    ON sh.schema_id = t.schema_id 
                INNER JOIN information_schema.schemata sch
                    ON sch.schema_name = sh.name
            WHERE 
                sh.name = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND t.name LIKE @where")}
            ORDER BY sh.name, i.name, ic.key_ordinal
        ";
        var constraintResults = await QueryAsync<(
            string schema_name,
            string constraint_name,
            string table_name,
            string column_name,
            int column_key_ordinal,
            bool is_desc,
            bool is_unique,
            bool is_primary_key,
            bool is_unique_constraint
        )>(db, constraintsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

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
                kfk.TABLE_SCHEMA = @schemaName
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

        var checkConstraintsSql =
            @$"
            select 
                schema_name(t.schema_id) AS schema_name,
                t.[name] AS table_name,
                col.[name] AS column_name,
                con.[name] AS constraint_name,
                con.[definition] AS check_expression
            from sys.check_constraints con
                left outer join sys.objects t on con.parent_object_id = t.object_id
                left outer join sys.all_columns col on con.parent_column_id = col.column_id and con.parent_object_id = col.object_id
            where 
                con.[definition] IS NOT NULL
                and schema_name(t.schema_id) = @schemaName
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

        var defaultConstraintsSql =
            @$"
            select 
                schema_name(t.schema_id) AS schema_name,
                t.[name] AS table_name,
                col.[name] AS column_name,
                con.[name] AS constraint_name,
                con.[definition] AS default_expression
            from sys.default_constraints con
                left outer join sys.objects t on con.parent_object_id = t.object_id
                left outer join sys.all_columns col on con.parent_column_id = col.column_id and con.parent_object_id = col.object_id
            where 
                schema_name(t.schema_id) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND t.[name] LIKE @where")}
            order by schema_name, table_name, column_name, constraint_name            
            ";
        var defaultConstraintResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string column_name,
            string constraint_name,
            string default_expression
        )>(db, defaultConstraintsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

        var tables = new List<DxTable>();

        foreach (
            var tableColumns in columnResults.GroupBy(r => new { r.schema_name, r.table_name })
        )
        {
            var tableName = tableColumns.Key.table_name;
            var tableConstraints = constraintResults
                .Where(t =>
                    (t.schema_name ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                    && t.table_name.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                )
                .ToArray();
            var foreignKeyConstraints = foreignKeyResults
                .Where(t =>
                    (t.schema_name ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                    && t.table_name.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                )
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
                        gb.Key.schema_name,
                        gb.Key.table_name,
                        gb.Key.constraint_name,
                        gb.Select(c => new DxOrderedColumn(c.column_name, DxColumnOrder.Ascending))
                            .ToArray(),
                        gb.Key.referenced_table_name,
                        gb.Select(c => new DxOrderedColumn(
                                c.referenced_column_name,
                                DxColumnOrder.Ascending
                            ))
                            .ToArray(),
                        gb.Key.delete_rule.ToForeignKeyAction(),
                        gb.Key.update_rule.ToForeignKeyAction()
                    );
                })
                .ToArray();
            var checkConstraints = checkConstraintResults
                .Where(t =>
                    (t.schema_name ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                    && t.table_name.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                )
                .Select(c =>
                {
                    return new DxCheckConstraint(
                        c.schema_name,
                        c.table_name,
                        c.column_name,
                        c.constraint_name,
                        c.check_expression
                    );
                })
                .ToArray();
            var defaultConstraints = defaultConstraintResults
                .Where(t =>
                    (t.schema_name ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                    && t.table_name.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                )
                .Select(c =>
                {
                    return new DxDefaultConstraint(
                        c.schema_name,
                        c.table_name,
                        c.column_name,
                        c.constraint_name,
                        c.default_expression
                    );
                })
                .ToArray();

            // extract primary key information from constraints query
            var primaryKeyConstraintInfo = tableConstraints.Where(t => t.is_primary_key).ToArray();
            var primaryKeyConstraint =
                primaryKeyConstraintInfo.Length > 0
                    ? new DxPrimaryKeyConstraint(
                        primaryKeyConstraintInfo[0].schema_name,
                        primaryKeyConstraintInfo[0].table_name,
                        primaryKeyConstraintInfo[0].constraint_name,
                        primaryKeyConstraintInfo
                            .OrderBy(t => t.column_key_ordinal)
                            .Select(t => new DxOrderedColumn(
                                t.column_name,
                                t.is_desc ? DxColumnOrder.Descending : DxColumnOrder.Ascending
                            ))
                            .ToArray()
                    )
                    : null;

            // extract unique constraint information from constraints query
            var uniqueConstraintsInfo = tableConstraints
                .Where(t => t.is_unique_constraint && !t.is_primary_key)
                .GroupBy(t => new
                {
                    t.schema_name,
                    t.table_name,
                    t.constraint_name
                })
                .ToArray();
            var uniqueConstraints = uniqueConstraintsInfo
                .Select(t => new DxUniqueConstraint(
                    t.Key.schema_name,
                    t.Key.table_name,
                    t.Key.constraint_name,
                    t.OrderBy(c => c.column_key_ordinal)
                        .Select(c => new DxOrderedColumn(
                            c.column_name,
                            c.is_desc ? DxColumnOrder.Descending : DxColumnOrder.Ascending
                        ))
                        .ToArray()
                ))
                .ToArray();

            // extract index information from constraints query
            var indexesInfo = tableConstraints
                .Where(t => !t.is_primary_key && !t.is_unique_constraint)
                .GroupBy(t => new
                {
                    t.schema_name,
                    t.table_name,
                    t.constraint_name
                })
                .ToArray();
            var indexes = indexesInfo
                .Select(t => new DxIndex(
                    t.Key.schema_name,
                    t.Key.table_name,
                    t.Key.constraint_name,
                    t.OrderBy(c => c.column_key_ordinal)
                        .Select(c => new DxOrderedColumn(
                            c.column_name,
                            c.is_desc ? DxColumnOrder.Descending : DxColumnOrder.Ascending
                        ))
                        .ToArray(),
                    t.First().is_unique
                ))
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
                    primaryKeyConstraint == null
                        ? false
                        : primaryKeyConstraint.Columns.Any(c =>
                            c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        ),
                    tableColumn.is_identity,
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

    public override async Task<bool> RenameTableIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string newTableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(newTableName))
        {
            throw new ArgumentException("New table name is required.", nameof(newTableName));
        }

        if (
            !await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            return false;
        }

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"EXEC sp_rename '{schemaQualifiedTableName}', '{newTableName}'",
                new
                {
                    schemaName,
                    tableName,
                    newTableName
                },
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
