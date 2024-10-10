namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    #region Schema Strings
    #endregion // Schema Strings

    #region Table Strings
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
