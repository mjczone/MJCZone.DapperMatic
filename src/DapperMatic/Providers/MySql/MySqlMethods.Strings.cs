using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    #region Schema Strings
    #endregion // Schema Strings

    #region Table Strings

    protected override string SqlInlineColumnNameAndType(DxColumn column, Version dbVersion)
    {
        var nameAndType = base.SqlInlineColumnNameAndType(column, dbVersion);
        if (
            nameAndType.Contains(" varchar", StringComparison.OrdinalIgnoreCase)
            || nameAndType.Contains(" text", StringComparison.OrdinalIgnoreCase)
        )
        {
            var doNotAddUtf8mb4 =
                (dbVersion < new Version(5, 5, 3))
                || (dbVersion.Major == 10 && dbVersion < new Version(10, 5, 25));

            if (!doNotAddUtf8mb4)
            {
                // make it unicode by default
                nameAndType += " CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci";
            }
        }
        return nameAndType;
    }

    // MySQL requires the AUTO_INCREMENT keyword to appear in the column definition, also
    // MySQL DOES NOT ALLOW a named constraint in the column definition, so we HAVE to create
    // the primary key constraint in the table constraints section
    protected override string SqlInlinePrimaryKeyColumnConstraint(
        string constraintName,
        bool isAutoIncrement,
        out bool useTableConstraint
    )
    {
        useTableConstraint = true;
        return isAutoIncrement ? "AUTO_INCREMENT" : "";

        // the following code doesn't work because MySQL doesn't allow named constraints in the column definition
        // return $"CONSTRAINT {NormalizeName(constraintName)} {(isAutoIncrement ? $"{SqlInlinePrimaryKeyAutoIncrementColumnConstraint()} " : "")}PRIMARY KEY".Trim();
    }

    protected override string SqlInlinePrimaryKeyAutoIncrementColumnConstraint()
    {
        return "AUTO_INCREMENT";
    }

    // MySQL doesn't allow default constraints to be named, so we just set the default without a name
    protected override string SqlInlineDefaultColumnConstraint(
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

        return $"DEFAULT {(addParentheses ? $"({defaultExpression})" : defaultExpression)}";
    }

    // MySQL DOES NOT ALLOW a named constraint in the column definition, so we HAVE to create
    // the check constraint in the table constraints section
    protected override string SqlInlineCheckColumnConstraint(
        string constraintName,
        string checkExpression,
        out bool useTableConstraint
    )
    {
        useTableConstraint = true;
        return "";
    }

    // MySQL DOES NOT ALLOW a named constraint in the column definition, so we HAVE to create
    // the unique constraint in the table constraints section
    protected override string SqlInlineUniqueColumnConstraint(
        string constraintName,
        out bool useTableConstraint
    )
    {
        useTableConstraint = true;
        return "";
    }

    // MySQL DOES NOT ALLOW a named constraint in the column definition, so we HAVE to create
    // the foreign key constraint in the table constraints section
    protected override string SqlInlineForeignKeyColumnConstraint(
        string? schemaName,
        string constraintName,
        string referencedTableName,
        DxOrderedColumn referencedColumn,
        DxForeignKeyAction? onDelete,
        DxForeignKeyAction? onUpdate,
        out bool useTableConstraint
    )
    {
        useTableConstraint = true;
        return "";
    }

    protected override (string sql, object parameters) SqlDoesTableExist(
        string? schemaName,
        string tableName
    )
    {
        var sql =
            @$"
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE' 
                and TABLE_SCHEMA = DATABASE()
                and TABLE_NAME = @tableName";

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName),
                tableName = NormalizeName(tableName)
            }
        );
    }

    protected override (string sql, object parameters) SqlGetTableNames(
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
                    AND TABLE_SCHEMA = DATABASE()
                    {(string.IsNullOrWhiteSpace(where) ? null : " AND TABLE_NAME LIKE @where")}
                ORDER BY TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }
    #endregion // Table Strings

    #region Column Strings
    #endregion // Column Strings

    #region Check Constraint Strings
    #endregion // Check Constraint Strings

    #region Default Constraint Strings
    protected override string SqlAlterTableAddDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression
    )
    {
        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        var defaultExpression = expression.Trim();
        var addParentheses =
            defaultExpression.Contains(' ')
            && !(defaultExpression.StartsWith("(") && defaultExpression.EndsWith(")"))
            && !(defaultExpression.StartsWith("\"") && defaultExpression.EndsWith("\""))
            && !(defaultExpression.StartsWith("'") && defaultExpression.EndsWith("'"));

        return @$"
            ALTER TABLE {schemaQualifiedTableName}
                ALTER COLUMN {NormalizeName(columnName)} SET DEFAULT {(addParentheses ? $"({defaultExpression})" : defaultExpression)}
        ";
    }

    protected override string SqlDropDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ALTER COLUMN {NormalizeName(columnName)} DROP DEFAULT";
    }
    #endregion // Default Constraint Strings

    #region Primary Key Strings
    protected override string SqlDropPrimaryKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP PRIMARY KEY";
    }
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    protected override string SqlDropUniqueConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP INDEX {NormalizeName(constraintName)}";
    }
    #endregion // Unique Constraint Strings

    #region Foreign Key Constraint Strings
    protected override string SqlDropForeignKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP FOREIGN KEY {NormalizeName(constraintName)}";
    }
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
    #endregion // Index Strings

    #region View Strings

    protected override (string sql, object parameters) SqlGetViewNames(
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
                    VIEW_DEFINITION IS NOT NULL
                    AND TABLE_SCHEMA = DATABASE()
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_SCHEMA, TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected override (string sql, object parameters) SqlGetViews(
        string? schemaName,
        string? viewNameFilter
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"SELECT 
                    NULL AS SchemaName,
                    TABLE_NAME AS ViewName,
                    VIEW_DEFINITION AS Definition
                FROM 
                    INFORMATION_SCHEMA.VIEWS
                WHERE 
                    VIEW_DEFINITION IS NOT NULL
                    AND TABLE_SCHEMA = DATABASE()
                    {(string.IsNullOrWhiteSpace(where) ? "" : "AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_SCHEMA, TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }
    #endregion // View Strings
}
