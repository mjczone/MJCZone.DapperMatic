using System.Data;
using System.Text;
using DapperMatic.Models;
using Microsoft.VisualBasic;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase
{
    #region Schema Strings
    protected virtual string SqlCreateSchema(string schemaName)
    {
        return @$"CREATE SCHEMA {NormalizeSchemaName(schemaName)}";
    }

    protected virtual (string sql, object parameters) SqlGetSchemaNames(
        string? schemaNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(schemaNameFilter)
            ? ""
            : ToLikeString(schemaNameFilter);

        var sql =
            $@"
            SELECT SCHEMA_NAME
            FROM INFORMATION_SCHEMA.SCHEMATA
            {(string.IsNullOrWhiteSpace(where) ? "" : $"WHERE SCHEMA_NAME LIKE @where")}
            ORDER BY SCHEMA_NAME";

        return (sql, new { where });
    }

    protected virtual string SqlDropSchema(string schemaName)
    {
        return @$"DROP SCHEMA {NormalizeSchemaName(schemaName)}";
    }
    #endregion // Schema Strings

    #region Table Strings
    protected virtual (string sql, object parameters) SqlDoesTableExist(
        string? schemaName,
        string tableName
    )
    {
        var sql =
            @$"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE 
                TABLE_TYPE='BASE TABLE'
                {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND TABLE_SCHEMA = @schemaName")}
                AND TABLE_NAME = @tableName";

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName),
                tableName = NormalizeName(tableName)
            }
        );
    }

    protected virtual (string sql, object parameters) SqlGetTableNames(
        string? schemaName,
        string? tableNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(tableNameFilter) ? "" : ToLikeString(tableNameFilter);

        var sql =
            $@"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE 
                    TABLE_TYPE = 'BASE TABLE' 
                    AND TABLE_SCHEMA = @schemaName
                    {(string.IsNullOrWhiteSpace(where) ? null : " AND TABLE_NAME LIKE @where")}
                ORDER BY TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected virtual string SqlDropTable(string? schemaName, string tableName)
    {
        return @$"DROP TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }

    protected virtual string SqlRenameTable(
        string? schemaName,
        string tableName,
        string newTableName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} RENAME TO {NormalizeName(newTableName)}";
    }

    protected virtual string SqlTruncateTable(string? schemaName, string tableName)
    {
        return @$"TRUNCATE TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }

    /// <summary>
    /// Anything inside of tableConstraints does NOT get added to the column definition.
    /// Anything added to the column definition should be added to the tableConstraints object.
    /// </summary>
    /// <param name="existingTable">The existing table WITHOUT the column being added</param>
    /// <param name="column">The new column</param>
    /// <param name="tableConstraints">Table constraints that will get added after the column definitions clauses in the CREATE TABLE or ALTER TABLE commands.</param>
    /// <returns></returns>
    protected virtual string SqlInlineColumnDefinition(
        DxTable existingTable,
        DxColumn column,
        DxTable tableConstraints
    )
    {
        var (schemaName, tableName, columnName) = NormalizeNames(
            existingTable.SchemaName,
            existingTable.TableName,
            column.ColumnName
        );

        var columnType = string.IsNullOrWhiteSpace(column.ProviderDataType)
            ? GetSqlTypeFromDotnetType(
                column.DotnetType,
                column.Length,
                column.Precision,
                column.Scale
            )
            : column.ProviderDataType;

        var sql = new StringBuilder();
        sql.Append($"{columnName} {columnType}");

        sql.Append(column.IsNullable ? " NULL" : " NOT NULL");

        // Only add the primary key here if the primary key is a single column key
        // and doesn't already exist in the existing table constraints
        var tpkc = tableConstraints.PrimaryKeyConstraint;
        if (
            column.IsPrimaryKey
            && (
                tpkc == null
                || (
                    tpkc.Columns.Count() == 1
                    && tpkc.Columns[0]
                        .ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            var pkConstraintName = ProviderUtils.GeneratePrimaryKeyConstraintName(
                tableName,
                columnName
            );
            var pkInlineSql = SqlInlinePrimaryKeyColumnConstraint(
                pkConstraintName,
                column.IsAutoIncrement,
                out var useTableConstraint
            );
            if (!string.IsNullOrWhiteSpace(pkInlineSql))
                sql.Append($" {pkInlineSql}");

            if (useTableConstraint)
            {
                tableConstraints.PrimaryKeyConstraint = new DxPrimaryKeyConstraint(
                    schemaName,
                    tableName,
                    pkConstraintName,
                    [new DxOrderedColumn(columnName)]
                );
            }
            else
            { // since we added the PK inline, we're going to remove it from the table constraints
                tableConstraints.PrimaryKeyConstraint = null;
            }
        }
#if DEBUG
        else if (column.IsPrimaryKey)
        {
            // PROVIDED FOR BREAKPOINT PURPOSES WHILE DEBUGGING: Primary key will be added as a table constraint
            sql.Append("");
        }
#endif

        if (
            !string.IsNullOrWhiteSpace(column.DefaultExpression)
            && tableConstraints.DefaultConstraints.All(dc =>
                !dc.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            var defConstraintName = ProviderUtils.GenerateDefaultConstraintName(
                tableName,
                columnName
            );
            sql.Append(
                $" {SqlInlineDefaultColumnConstraint(defConstraintName, column.DefaultExpression)}"
            );
        }
        else
        {
            // DEFAULT EXPRESSIONS ARE A LITTLE DIFFERENT
            // In our case, we're always going to add them via the column definition.
            // SQLite ONLY allows default expressions to be added via the column definition.
            // Other providers also allow it, so let's just do them all here
            var defaultConstraint = tableConstraints.DefaultConstraints.FirstOrDefault(dc =>
                dc.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
            );
            if (defaultConstraint != null)
            {
                sql.Append(
                    $" {SqlInlineDefaultColumnConstraint(defaultConstraint.ConstraintName, defaultConstraint.Expression)}"
                );
            }
        }

        if (
            !string.IsNullOrWhiteSpace(column.CheckExpression)
            && tableConstraints.CheckConstraints.All(ck =>
                string.IsNullOrWhiteSpace(ck.ColumnName)
                || !ck.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            var ckConstraintName = ProviderUtils.GenerateCheckConstraintName(tableName, columnName);
            var ckInlineSql = SqlInlineCheckColumnConstraint(
                ckConstraintName,
                column.CheckExpression,
                out var useTableConstraint
            );

            if (!string.IsNullOrWhiteSpace(ckInlineSql))
                sql.Append($" {ckInlineSql}");

            if (useTableConstraint)
            {
                tableConstraints.CheckConstraints.Add(
                    new DxCheckConstraint(
                        schemaName,
                        tableName,
                        columnName,
                        ckConstraintName,
                        column.CheckExpression
                    )
                );
            }
        }

        if (
            column.IsUnique
            && !column.IsIndexed
            && tableConstraints.UniqueConstraints.All(uc =>
                !uc.Columns.Any(c =>
                    c.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            var ucConstraintName = ProviderUtils.GenerateUniqueConstraintName(
                tableName,
                columnName
            );
            var ucInlineSql = SqlInlineUniqueColumnConstraint(
                ucConstraintName,
                out var useTableConstraint
            );

            if (!string.IsNullOrWhiteSpace(ucInlineSql))
                sql.Append($" {ucInlineSql}");

            if (useTableConstraint)
            {
                tableConstraints.UniqueConstraints.Add(
                    new DxUniqueConstraint(
                        schemaName,
                        tableName,
                        ucConstraintName,
                        [new DxOrderedColumn(columnName)]
                    )
                );
            }
        }

        if (
            column.IsForeignKey
            && !string.IsNullOrWhiteSpace(column.ReferencedTableName)
            && !string.IsNullOrWhiteSpace(column.ReferencedColumnName)
            && tableConstraints.ForeignKeyConstraints.All(fk =>
                !fk.SourceColumns.Any(c =>
                    c.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            var fkConstraintName = ProviderUtils.GenerateForeignKeyConstraintName(
                tableName,
                columnName,
                NormalizeName(column.ReferencedTableName),
                NormalizeName(column.ReferencedColumnName)
            );
            var fkInlineSql = SqlInlineForeignKeyColumnConstraint(
                schemaName,
                fkConstraintName,
                column.ReferencedTableName,
                new DxOrderedColumn(column.ReferencedColumnName),
                column.OnDelete,
                column.OnUpdate,
                out var useTableConstraint
            );

            if (!string.IsNullOrWhiteSpace(fkInlineSql))
                sql.Append($" {fkInlineSql}");

            if (useTableConstraint)
            {
                tableConstraints.ForeignKeyConstraints.Add(
                    new DxForeignKeyConstraint(
                        schemaName,
                        tableName,
                        fkConstraintName,
                        [new DxOrderedColumn(columnName)],
                        column.ReferencedTableName,
                        [new DxOrderedColumn(column.ReferencedColumnName)],
                        column.OnDelete ?? DxForeignKeyAction.NoAction,
                        column.OnUpdate ?? DxForeignKeyAction.NoAction
                    )
                );
            }
        }

        if (
            column.IsIndexed
            && tableConstraints.Indexes.All(i =>
                !i.Columns.Any(c =>
                    c.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            var indexName = ProviderUtils.GenerateIndexName(tableName, columnName);
            tableConstraints.Indexes.Add(
                new DxIndex(
                    schemaName,
                    tableName,
                    indexName,
                    [new DxOrderedColumn(columnName)],
                    column.IsUnique
                )
            );
        }

        return sql.ToString();
    }

    protected virtual string SqlInlinePrimaryKeyColumnConstraint(
        string constraintName,
        bool isAutoIncrement,
        out bool useTableConstraint
    )
    {
        useTableConstraint = false;
        return $"CONSTRAINT {NormalizeName(constraintName)} PRIMARY KEY {(isAutoIncrement ? SqlInlinePrimaryKeyAutoIncrementColumnConstraint() : "")}".Trim();
    }

    protected virtual string SqlInlinePrimaryKeyAutoIncrementColumnConstraint()
    {
        return "IDENTITY(1,1)";
    }

    protected virtual string SqlInlineDefaultColumnConstraint(
        string constraintName,
        string defaultExpression
    )
    {
        defaultExpression = defaultExpression.Trim();
        var addParentheses =
            defaultExpression.Contains(' ')
            && !(defaultExpression.StartsWith("(") && defaultExpression.EndsWith(")"))
            && !(defaultExpression.StartsWith("\"") && defaultExpression.EndsWith("\""))
            && !(defaultExpression.StartsWith("'") && defaultExpression.EndsWith("'"));

        return $"CONSTRAINT {NormalizeName(constraintName)} DEFAULT {(addParentheses ? $"({defaultExpression})" : defaultExpression)}";
    }

    protected virtual string SqlInlineCheckColumnConstraint(
        string constraintName,
        string checkExpression,
        out bool useTableConstraint
    )
    {
        useTableConstraint = false;
        return $"CONSTRAINT {NormalizeName(constraintName)} CHECK ({checkExpression})";
    }

    protected virtual string SqlInlineUniqueColumnConstraint(
        string constraintName,
        out bool useTableConstraint
    )
    {
        useTableConstraint = false;
        return $"CONSTRAINT {NormalizeName(constraintName)} UNIQUE";
    }

    protected virtual string SqlInlineForeignKeyColumnConstraint(
        string? schemaName,
        string constraintName,
        string referencedTableName,
        DxOrderedColumn referencedColumn,
        DxForeignKeyAction? onDelete,
        DxForeignKeyAction? onUpdate,
        out bool useTableConstraint
    )
    {
        useTableConstraint = false;
        return @$"CONSTRAINT {NormalizeName(constraintName)} REFERENCES {GetSchemaQualifiedIdentifierName(schemaName, referencedTableName)} ({NormalizeName(referencedColumn.ColumnName)})"
            + (onDelete.HasValue ? $" ON DELETE {onDelete.Value.ToSql()}" : "")
            + (onUpdate.HasValue ? $" ON UPDATE {onUpdate.Value.ToSql()}" : "");
    }

    protected virtual string SqlInlinePrimaryKeyTableConstraint(
        DxTable table,
        DxPrimaryKeyConstraint primaryKeyConstraint
    )
    {
        var pkColumns = primaryKeyConstraint.Columns.Select(c => c.ToString());
        var pkColumnNames = primaryKeyConstraint.Columns.Select(c => c.ColumnName);
        var pkConstrainName = !string.IsNullOrWhiteSpace(primaryKeyConstraint.ConstraintName)
            ? primaryKeyConstraint.ConstraintName
            : ProviderUtils.GeneratePrimaryKeyConstraintName(
                table.TableName,
                pkColumnNames.ToArray()
            );
        return $"CONSTRAINT {NormalizeName(pkConstrainName)} PRIMARY KEY ({string.Join(", ", pkColumnNames)})".Trim();
    }

    protected virtual string SqlInlineCheckTableConstraint(DxTable table, DxCheckConstraint check)
    {
        var ckConstraintName = !string.IsNullOrWhiteSpace(check.ConstraintName)
            ? check.ConstraintName
            : (
                string.IsNullOrWhiteSpace(check.ColumnName)
                    ? ProviderUtils.GenerateCheckConstraintName(
                        table.TableName,
                        DateTime.Now.Ticks.ToString()
                    )
                    : ProviderUtils.GenerateCheckConstraintName(table.TableName, check.ColumnName)
            );

        return $"CONSTRAINT {NormalizeName(ckConstraintName)} CHECK ({check.Expression})";
    }

    // protected virtual string SqlInlineDefaultTableConstraint(DxTable table, DxDefaultConstraint def)
    // {
    //     var defaultExpression = def.Expression.Trim();
    //     var addParentheses =
    //         defaultExpression.Contains(' ')
    //         && !(defaultExpression.StartsWith("(") && defaultExpression.EndsWith(")"))
    //         && !(defaultExpression.StartsWith("\"") && defaultExpression.EndsWith("\""))
    //         && !(defaultExpression.StartsWith("'") && defaultExpression.EndsWith("'"));

    //     var constraintName = !string.IsNullOrWhiteSpace(def.ConstraintName)
    //         ? def.ConstraintName
    //         : ProviderUtils.GenerateDefaultConstraintName(table.TableName, def.ColumnName);

    //     return $"CONSTRAINT {NormalizeName(constraintName)} DEFAULT {(addParentheses ? $"({defaultExpression})" : defaultExpression)}";
    // }

    protected virtual string SqlInlineUniqueTableConstraint(
        DxTable table,
        DxUniqueConstraint uc,
        bool supportsOrderedKeysInConstraints
    )
    {
        var ucConstraintName = !string.IsNullOrWhiteSpace(uc.ConstraintName)
            ? uc.ConstraintName
            : ProviderUtils.GenerateUniqueConstraintName(
                table.TableName,
                uc.Columns.Select(c => NormalizeName(c.ColumnName)).ToArray()
            );

        var uniqueColumns = uc.Columns.Select(c =>
            supportsOrderedKeysInConstraints
                ? new DxOrderedColumn(NormalizeName(c.ColumnName), c.Order).ToString()
                : new DxOrderedColumn(NormalizeName(c.ColumnName)).ToString()
        );
        return $"CONSTRAINT {NormalizeName(ucConstraintName)} UNIQUE ({string.Join(", ", uniqueColumns)})";
    }

    protected virtual string SqlInlineForeignKeyTableConstraint(
        DxTable table,
        DxForeignKeyConstraint fk
    )
    {
        return @$"
            CONSTRAINT {NormalizeName(fk.ConstraintName)} 
                FOREIGN KEY ({string.Join(", ", fk.SourceColumns.Select(c => NormalizeName(c.ColumnName)))}) 
                    REFERENCES {GetSchemaQualifiedIdentifierName(table.SchemaName, fk.ReferencedTableName)} ({string.Join(", ", fk.ReferencedColumns.Select(c => NormalizeName(c.ColumnName)))})
                        ON DELETE {fk.OnDelete.ToSql()}
                        ON UPDATE {fk.OnUpdate.ToSql()}".Trim();
    }

    #endregion // Table Strings

    #region Column Strings
    protected virtual string SqlDropColumn(string? schemaName, string tableName, string columnName)
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP COLUMN {NormalizeName(columnName)}";
    }
    #endregion // Column Strings

    #region Check Constraint Strings
    protected virtual string SqlAlterTableAddCheckConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        string expression
    )
    {
        if (expression.Trim().StartsWith('(') && expression.Trim().EndsWith(')'))
            expression = expression.Trim().Substring(1, expression.Length - 2);

        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ADD CONSTRAINT {NormalizeName(constraintName)} CHECK ({expression})";
    }

    protected virtual string SqlDropCheckConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Check Constraint Strings

    #region Default Constraint Strings
    protected virtual string SqlAlterTableAddDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression
    )
    {
        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        return @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {NormalizeName(constraintName)} DEFAULT {expression} FOR {NormalizeName(columnName)}
        ";
    }

    protected virtual string SqlDropDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Default Constraint Strings

    #region Primary Key Strings
    protected virtual string SqlAlterTableAddPrimaryKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        bool supportsOrderedKeysInConstraints
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} 
                    ADD CONSTRAINT {NormalizeName(constraintName)} 
                        PRIMARY KEY ({string.Join(", ", columns.Select(c => {
                            var columnName = NormalizeName(c.ColumnName);
                            return c.Order == DxColumnOrder.Ascending
                                ? columnName
                                : $"{columnName} DESC";
                        }))})";
    }

    protected virtual string SqlDropPrimaryKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    protected virtual string SqlAlterTableAddUniqueConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        bool supportsOrderedKeysInConstraints
    )
    {
        var uniqueColumns = columns.Select(c =>
            supportsOrderedKeysInConstraints
                ? new DxOrderedColumn(NormalizeName(c.ColumnName), c.Order).ToString()
                : new DxOrderedColumn(NormalizeName(c.ColumnName)).ToString()
        );
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)}
                    ADD CONSTRAINT {NormalizeName(constraintName)} UNIQUE ({string.Join(", ", uniqueColumns)})";
    }

    protected virtual string SqlDropUniqueConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Unique Constraint Strings

    #region Foreign Key Constraint Strings
    protected virtual string SqlAlterTableAddForeignKeyConstraint(
        string? schemaName,
        string constraintName,
        string tableName,
        DxOrderedColumn[] columns,
        string referencedTableName,
        DxOrderedColumn[] referencedColumns,
        DxForeignKeyAction onDelete,
        DxForeignKeyAction onUpdate
    )
    {
        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);
        var schemaQualifiedReferencedTableName = GetSchemaQualifiedIdentifierName(
            schemaName,
            referencedTableName
        );
        var columnNames = columns.Select(c => NormalizeName(c.ColumnName));
        var referencedColumnNames = referencedColumns.Select(c => NormalizeName(c.ColumnName));

        return @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {NormalizeName(constraintName)} 
                    FOREIGN KEY ({string.Join(", ", columnNames)})
                        REFERENCES {schemaQualifiedReferencedTableName} ({string.Join(", ", referencedColumnNames)})
                            ON DELETE {onDelete.ToSql()}
                            ON UPDATE {onUpdate.ToSql()}
        ";
    }

    protected virtual string SqlDropForeignKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
    protected virtual string SqlCreateIndex(
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false
    )
    {
        return @$"CREATE {(isUnique ? "UNIQUE " : "")}INDEX {NormalizeName(indexName)} ON {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ({string.Join(", ", columns.Select(c => c.ToString()))})";
    }

    protected virtual string SqlDropIndex(string? schemaName, string tableName, string indexName)
    {
        return @$"DROP INDEX {NormalizeName(indexName)} ON {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }
    #endregion // Index Strings

    #region View Strings

    protected virtual string SqlCreateView(string? schemaName, string viewName, string definition)
    {
        return @$"CREATE VIEW {GetSchemaQualifiedIdentifierName(schemaName, viewName)} AS {definition}";
    }

    protected virtual (string sql, object parameters) SqlGetViewNames(
        string? schemaName,
        string? viewNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"SELECT 
                    TABLE_NAME AS ViewName
                FROM 
                    INFORMATION_SCHEMA.VIEWS
                WHERE 
                    TABLE_NAME IS NOT NULL
                    {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND TABLE_SCHEMA = @schemaName")}
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected virtual (string sql, object parameters) SqlGetViews(
        string? schemaName,
        string? viewNameFilter
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"SELECT 
                    TABLE_SCHEMA AS SchemaName
                    TABLE_NAME AS ViewName,
                    VIEW_DEFINITION AS Definition
                FROM 
                    INFORMATION_SCHEMA.VIEWS
                WHERE 
                    TABLE_NAME IS NOT NULL
                    {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND TABLE_SCHEMA = @schemaName")}
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected virtual string NormalizeViewDefinition(string definition)
    {
        return definition;
    }

    protected virtual string SqlDropView(string? schemaName, string viewName)
    {
        return @$"DROP VIEW {GetSchemaQualifiedIdentifierName(schemaName, viewName)}";
    }
    #endregion // View Strings
}
