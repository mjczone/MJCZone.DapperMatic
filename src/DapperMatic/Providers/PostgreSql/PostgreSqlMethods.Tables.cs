using System.Data;
using System.Text;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

// see: https://www.postgresql.org/docs/15/catalogs.html
public partial class PostgreSqlMethods
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
            @$"
            SELECT COUNT(*) 
            FROM pg_class 
                JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace 
            WHERE 
                relkind = 'r'
                AND lower(nspname) = @schemaName
                AND lower(relname) = @tableName";

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

        var supportsOrderedKeysInConstraints = await SupportsOrderedKeysInConstraintsAsync(
                db,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        // add single column primary key constraints as column definitions; and,
        // add multi column primary key constraints here
        if (primaryKey != null && primaryKey.Columns.Length > 1)
        {
            var pkColumns = primaryKey.Columns.Select(c =>
                c.ToString(supportsOrderedKeysInConstraints)
            );
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
                var checkConstraintName = NormalizeName(constraint.ConstraintName);
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
                var fkColumns = constraint.SourceColumns.Select(c =>
                    c.ToString(supportsOrderedKeysInConstraints)
                );
                var fkReferencedColumns = constraint.ReferencedColumns.Select(c =>
                    c.ToString(supportsOrderedKeysInConstraints)
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
                    c.ToString(supportsOrderedKeysInConstraints)
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
                WHERE TABLE_TYPE='BASE TABLE' AND lower(TABLE_SCHEMA) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND lower(TABLE_NAME) LIKE @where")}
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
        // we could use information_schema but it's SOOO SLOW! unbearable really,
        // so we will use pg_catalog instead
        var columnsSql =
            @$"
            SELECT 
                schemas.nspname as schema_name,
                tables.relname as table_name,
                columns.attname as column_name,
                columns.attnum as column_ordinal,
                pg_get_expr(column_defs.adbin, column_defs.adrelid) as column_default,
                case when (coalesce(primarykeys.conname, '') = '') then 0 else 1 end AS is_primary_key,
                primarykeys.conname as pk_constraint_name,
                case when columns.attnotnull then 0 else 1 end AS is_nullable,
                case when (columns.attidentity = '') then 0 else 1 end as is_identity,
                types.typname as data_type,
            	format_type(columns.atttypid, columns.atttypmod) as data_type_ext
            FROM pg_catalog.pg_attribute AS columns
                join pg_catalog.pg_type as types on columns.atttypid = types.oid
                JOIN pg_catalog.pg_class AS tables ON columns.attrelid = tables.oid and tables.relkind = 'r' and tables.relpersistence = 'p'
                JOIN pg_catalog.pg_namespace AS schemas ON tables.relnamespace = schemas.oid
                left outer join pg_catalog.pg_attrdef as column_defs on columns.attrelid = column_defs.adrelid and columns.attnum = column_defs.adnum
                left outer join pg_catalog.pg_constraint as primarykeys on columns.attnum=ANY(primarykeys.conkey) AND primarykeys.conrelid = tables.oid and primarykeys.contype = 'p'
            where
                schemas.nspname not like 'pg_%' and schemas.nspname != 'information_schema' and columns.attnum > 0 and not columns.attisdropped
                AND lower(schemas.nspname) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND lower(tables.relname) LIKE @where")}
            order by schema_name, table_name, column_ordinal;
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
            string data_type_ext
        )>(db, columnsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

        // get indexes
        var indexes = await GetIndexesInternalAsync(
                db,
                schemaName,
                tableNameFilter,
                null,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        // get primary key, unique key, foreign key and check constraints in a single query
        var constraintsSql =
            @$"
            select 
                schemas.nspname as schema_name,
                tables.relname as table_name,
                r.conname as constraint_name,
                indexes.relname as supporting_index_name,
                case 
                    when r.contype = 'c' then 'CHECK'
                    when r.contype = 'f' then 'FOREIGN KEY'
                    when r.contype = 'p' then 'PRIMARY KEY'
                    when r.contype = 'u' then 'UNIQUE'
                    else 'OTHER'
                end as constraint_type,
                pg_catalog.pg_get_constraintdef(r.oid, true) as constraint_definition,
                referenced_tables.relname as referenced_table_name,
                array_to_string(r.conkey, ',') as column_ordinals_csv,
                array_to_string(r.confkey, ',') as referenced_column_ordinals_csv,
                case
                    when r.confdeltype = 'a' then 'NO ACTION'
                    when r.confdeltype = 'r' then 'RESTRICT'
                    when r.confdeltype = 'c' then 'CASCADE'
                    when r.confdeltype = 'n' then 'SET NULL'
                    when r.confdeltype = 'd' then 'SET DEFAULT'
                    else null
                end as delete_rule,
                case
                    when r.confupdtype = 'a' then 'NO ACTION'
                    when r.confupdtype = 'r' then 'RESTRICT'
                    when r.confupdtype = 'c' then 'CASCADE'
                    when r.confupdtype = 'n' then 'SET NULL'
                    when r.confupdtype = 'd' then 'SET DEFAULT'
                    else null
                end as update_rule	
            from pg_catalog.pg_constraint r
                join pg_catalog.pg_namespace AS schemas ON r.connamespace = schemas.oid
                join pg_class as tables on r.conrelid = tables.oid
                left outer join pg_class as indexes on r.conindid = indexes.oid
                left outer join pg_class as referenced_tables on r.confrelid = referenced_tables.oid
            where
                schemas.nspname not like 'pg_%' 
                and schemas.nspname != 'information_schema'
                and r.contype in ('c', 'f', 'p', 'u')
                and lower(schemas.nspname) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND lower(tables.relname) LIKE @where")}
            order by schema_name, table_name, constraint_type, constraint_name
        ";
        var constraintResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string constraint_name,
            string supporting_index_name,
            /* CHECK, UNIQUE, FOREIGN KEY, PRIMARY KEY */
            string constraint_type,
            string constraint_definition,
            string referenced_table_name,
            string column_ordinals_csv,
            string referenced_column_ordinals_csv,
            string delete_rule,
            string update_rule
        )>(db, constraintsSql, new { schemaName, where }, transaction: tx).ConfigureAwait(false);

        var referencedTableNames = constraintResults
            .Where(c => c.constraint_type == "FOREIGN KEY")
            .Select(c => c.referenced_table_name.ToLowerInvariant())
            .Distinct()
            .ToArray();
        var referencedColumnsSql =
            @$"
            SELECT 
                schemas.nspname as schema_name,
                tables.relname as table_name,
                columns.attname as column_name,
                columns.attnum as column_ordinal
            FROM pg_catalog.pg_attribute AS columns
                JOIN pg_catalog.pg_class AS tables ON columns.attrelid = tables.oid and tables.relkind = 'r' and tables.relpersistence = 'p'
                JOIN pg_catalog.pg_namespace AS schemas ON tables.relnamespace = schemas.oid
            where
                schemas.nspname not like 'pg_%' and schemas.nspname != 'information_schema' and columns.attnum > 0 and not columns.attisdropped
                AND lower(schemas.nspname) = @schemaName
                AND lower(tables.relname) = ANY (@referencedTableNames)
            order by schema_name, table_name, column_ordinal;
        ";
        var referencedColumnsResults =
            referencedTableNames.Length == 0
                ? []
                : await QueryAsync<(
                    string schema_name,
                    string table_name,
                    string column_name,
                    int column_ordinal
                )>(
                        db,
                        referencedColumnsSql,
                        new { schemaName, referencedTableNames },
                        transaction: tx
                    )
                    .ConfigureAwait(false);

        var tables = new List<DxTable>();

        foreach (
            var tableColumnResults in columnResults.GroupBy(r => new
            {
                r.schema_name,
                r.table_name
            })
        )
        {
            schemaName = tableColumnResults.Key.schema_name;
            var tableName = tableColumnResults.Key.table_name;
            var tableConstraintResults = constraintResults
                .Where(t =>
                    t.schema_name.Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                    && t.table_name.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                )
                .ToArray();

            var tableForeignKeyConstraints = tableConstraintResults
                .Where(t =>
                    t.constraint_type.Equals("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
                )
                .Select(row =>
                {
                    var sourceColumns = row
                        .column_ordinals_csv.Split(',')
                        .Select(r =>
                        {
                            return new DxOrderedColumn(
                                tableColumnResults
                                    .First(c => c.column_ordinal == int.Parse(r))
                                    .column_name
                            );
                        })
                        .ToArray();
                    var referencedColumns = row
                        .referenced_column_ordinals_csv.Split(',')
                        .Select(r =>
                        {
                            return new DxOrderedColumn(
                                referencedColumnsResults
                                    .First(c =>
                                        c.table_name.Equals(
                                            row.referenced_table_name,
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                        && c.column_ordinal == int.Parse(r)
                                    )
                                    .column_name
                            );
                        })
                        .ToArray();
                    return new DxForeignKeyConstraint(
                        row.schema_name,
                        row.table_name,
                        row.constraint_name,
                        sourceColumns,
                        row.referenced_table_name,
                        referencedColumns,
                        row.delete_rule.ToForeignKeyAction(),
                        row.update_rule.ToForeignKeyAction()
                    );
                })
                .ToArray();

            var tableCheckConstraints = tableConstraintResults
                .Where(t =>
                    t.constraint_type.Equals("CHECK", StringComparison.OrdinalIgnoreCase)
                    && t.constraint_definition != null
                    && t.constraint_definition.StartsWith(
                        "CHECK (",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Select(c =>
                {
                    var columns = (c.column_ordinals_csv ?? "")
                        .Split(',')
                        .Select(r =>
                        {
                            return tableColumnResults
                                .First(c => c.column_ordinal == int.Parse(r))
                                .column_name;
                        })
                        .ToArray();
                    return new DxCheckConstraint(
                        c.schema_name,
                        c.table_name,
                        columns.Length == 1 ? columns[0] : null,
                        c.constraint_name,
                        c.constraint_definition.Substring(7).TrimEnd(')')
                    );
                })
                .ToArray();

            var tableDefaultConstraints = tableColumnResults
                // ignore default values that are sequences (from SERIAL columns)
                .Where(t =>
                    !string.IsNullOrWhiteSpace(t.column_default)
                    && !t.column_default.StartsWith("nextval()", StringComparison.OrdinalIgnoreCase)
                )
                .Select(c =>
                {
                    return new DxDefaultConstraint(
                        c.schema_name,
                        c.table_name,
                        c.column_name,
                        $"df_{c.table_name}_{c.column_name}",
                        c.column_default
                    );
                })
                .ToArray();

            var tablePrimaryKeyConstraint = tableConstraintResults
                .Where(t =>
                    t.constraint_type.Equals("PRIMARY KEY", StringComparison.OrdinalIgnoreCase)
                )
                .Select(row =>
                {
                    var columns = row
                        .column_ordinals_csv.Split(',')
                        .Select(r =>
                        {
                            return new DxOrderedColumn(
                                tableColumnResults
                                    .First(c => c.column_ordinal == int.Parse(r))
                                    .column_name
                            );
                        })
                        .ToArray();
                    return new DxPrimaryKeyConstraint(
                        row.schema_name,
                        row.table_name,
                        row.constraint_name,
                        columns
                    );
                })
                .FirstOrDefault();

            var tableUniqueConstraints = tableConstraintResults
                .Where(t => t.constraint_type.Equals("UNIQUE", StringComparison.OrdinalIgnoreCase))
                .Select(row =>
                {
                    var columns = row
                        .column_ordinals_csv.Split(',')
                        .Select(r =>
                        {
                            return new DxOrderedColumn(
                                tableColumnResults
                                    .First(c => c.column_ordinal == int.Parse(r))
                                    .column_name
                            );
                        })
                        .ToArray();
                    return new DxUniqueConstraint(
                        row.schema_name,
                        row.table_name,
                        row.constraint_name,
                        columns
                    );
                })
                .ToArray();

            var tableIndexes = indexes
                .Where(i =>
                    (i.SchemaName ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                    && i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                )
                .ToArray();

            var columns = new List<DxColumn>();
            foreach (var tableColumn in tableColumnResults)
            {
                var columnIsUniqueViaUniqueConstraintOrIndex =
                    tableUniqueConstraints.Any(c =>
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

                var columnIsForeignKey = tableForeignKeyConstraints.Any(c =>
                    c.SourceColumns.Any(c =>
                        c.ColumnName.Equals(
                            tableColumn.column_name,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );

                var foreignKeyConstraint = tableForeignKeyConstraints.FirstOrDefault(c =>
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

                ExtractColumnTypeInfoFromFullSqlType(
                    tableColumn.data_type,
                    tableColumn.data_type_ext,
                    out var dotnetType,
                    out var length,
                    out var precision,
                    out var scale
                );

                var column = new DxColumn(
                    tableColumn.schema_name,
                    tableColumn.table_name,
                    tableColumn.column_name,
                    dotnetType,
                    tableColumn.data_type,
                    length,
                    precision,
                    scale,
                    tableCheckConstraints
                        .FirstOrDefault(c =>
                            !string.IsNullOrWhiteSpace(c.ColumnName)
                            && c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        ?.Expression,
                    tableDefaultConstraints
                        .FirstOrDefault(c =>
                            !string.IsNullOrWhiteSpace(c.ColumnName)
                            && c.ColumnName.Equals(
                                tableColumn.column_name,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        ?.Expression,
                    tableColumn.is_nullable,
                    tablePrimaryKeyConstraint != null
                        && tablePrimaryKeyConstraint.Columns.Any(c =>
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
                tablePrimaryKeyConstraint,
                tableCheckConstraints,
                tableDefaultConstraints,
                tableUniqueConstraints,
                tableForeignKeyConstraints,
                tableIndexes
            );
            tables.Add(table);
        }

        return tables;
    }

    public override async Task<bool> DropTableIfExistsAsync(
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

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        // drop table
        await ExecuteAsync(
                db,
                $@"DROP TABLE IF EXISTS {schemaQualifiedTableName} CASCADE",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    private async Task<List<DxIndex>> GetIndexesInternalAsync(
        IDbConnection db,
        string? schemaNameFilter,
        string? tableNameFilter,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var whereSchemaLike = string.IsNullOrWhiteSpace(schemaNameFilter)
            ? null
            : ToLikeString(schemaNameFilter);
        var whereTableLike = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToLikeString(tableNameFilter);
        var whereIndexLike = string.IsNullOrWhiteSpace(indexNameFilter)
            ? null
            : ToLikeString(indexNameFilter);

        var indexesSql =
            @$"
                select
                    schemas.nspname AS schema_name,
                    tables.relname AS table_name,
                    indexes.relname AS index_name,
                    case when i.indisunique then 1 else 0 end as is_unique,
                    array_to_string(array_agg (
                        a.attname 
                        || ' ' || CASE o.option & 1 WHEN 1 THEN 'DESC' ELSE 'ASC' END
                        || ' ' || CASE o.option & 2 WHEN 2 THEN 'NULLS FIRST' ELSE 'NULLS LAST' END
                        ORDER BY c.ordinality
                    ),',') AS columns_csv
                from 
                    pg_index AS i
                    JOIN pg_class AS tables ON tables.oid = i.indrelid
                    JOIN pg_namespace AS schemas ON tables.relnamespace = schemas.oid
                    JOIN pg_class AS indexes ON indexes.oid = i.indexrelid
                    CROSS JOIN LATERAL unnest (i.indkey) WITH ORDINALITY AS c (colnum, ordinality)
                    LEFT JOIN LATERAL unnest (i.indoption) WITH ORDINALITY AS o (option, ordinality)
                    ON c.ordinality = o.ordinality
                    JOIN pg_attribute AS a ON tables.oid = a.attrelid AND a.attnum = c.colnum
                where
                    schemas.nspname not like 'pg_%' 
                    and schemas.nspname != 'information_schema'
                    and i.indislive
                    and not i.indisprimary
                    
                    {(string.IsNullOrWhiteSpace(whereSchemaLike) ? "" : " AND lower(schemas.nspname) LIKE @whereSchemaLike")}
                    {(string.IsNullOrWhiteSpace(whereTableLike) ? "" : " AND lower(tables.relname) LIKE @whereTableLike")}
                    {(string.IsNullOrWhiteSpace(whereIndexLike) ? "" : " AND lower(indexes.relname) LIKE @whereIndexLike")}

                    -- postgresql creates an index for primary key and unique constraints, so we don't need to include them in the results
                    and indexes.relname not in (select x.conname from pg_catalog.pg_constraint x 
                                join pg_catalog.pg_namespace AS x2 ON x.connamespace = x2.oid
                                join pg_class as x3 on x.conrelid = x3.oid
                                where x2.nspname = schemas.nspname and x3.relname = tables.relname)
                group by schemas.nspname, tables.relname, indexes.relname, i.indisunique
                order by schema_name, table_name, index_name
            ";

        var indexResults = await QueryAsync<(
            string schema_name,
            string table_name,
            string index_name,
            bool is_unique,
            string columns_csv
        )>(
                db,
                indexesSql,
                new
                {
                    whereSchemaLike,
                    whereTableLike,
                    whereIndexLike
                },
                tx
            )
            .ConfigureAwait(false);

        var indexes = new List<DxIndex>();
        foreach (var ir in indexResults)
        {
            var columns = ir
                .columns_csv.Split(',')
                .Select(c =>
                {
                    var columnName = c.Trim()
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                        )
                        .First();
                    var isDescending = c.Contains("desc", StringComparison.OrdinalIgnoreCase);
                    return new DxOrderedColumn(
                        columnName,
                        isDescending ? DxColumnOrder.Descending : DxColumnOrder.Ascending
                    );
                })
                .ToArray();

            var index = new DxIndex(
                ir.schema_name,
                ir.table_name,
                ir.index_name,
                columns,
                ir.is_unique
            );
            indexes.Add(index);
        }

        return [.. indexes];
    }
}
