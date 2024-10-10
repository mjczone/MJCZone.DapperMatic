namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    #region Schema Strings
    protected override (string sql, object parameters) SqlGetSchemaNames(
        string? schemaNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(schemaNameFilter)
            ? ""
            : ToLikeString(schemaNameFilter);

        var sql =
            $@"
            SELECT DISTINCT nspname
            FROM pg_catalog.pg_namespace
            {(string.IsNullOrWhiteSpace(where) ? "" : $"WHERE lower(nspname) LIKE @where")}
            ORDER BY nspname";

        return (sql, new { where });
    }

    protected override string SqlDropSchema(string schemaName)
    {
        return @$"DROP SCHEMA IF EXISTS {NormalizeSchemaName(schemaName)} CASCADE";
    }
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
            FROM pg_class 
                JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace 
            WHERE 
                relkind = 'r'
                {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND lower(nspname) = @schemaName")}
                AND lower(relname) = @tableName";

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
                    AND lower(TABLE_SCHEMA) = @schemaName
                    AND TABLE_NAME NOT IN ('spatial_ref_sys', 'geometry_columns', 'geography_columns', 'raster_columns', 'raster_overviews')
                    {(string.IsNullOrWhiteSpace(where) ? null : " AND lower(TABLE_NAME) LIKE @where")}
                ORDER BY TABLE_NAME";

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName)?.ToLowerInvariant(),
                where = where?.ToLowerInvariant()
            }
        );
    }

    protected override string SqlDropTable(string? schemaName, string tableName)
    {
        return @$"DROP TABLE IF EXISTS {GetSchemaQualifiedIdentifierName(schemaName, tableName)} CASCADE";
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

        return @$"
            ALTER TABLE {schemaQualifiedTableName}
                ALTER COLUMN {NormalizeName(columnName)} SET DEFAULT {expression}
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
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    #endregion // Unique Constraint Strings

    #region Foreign Key Constraint Strings
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
    protected override string SqlDropIndex(string? schemaName, string tableName, string indexName)
    {
        return @$"DROP INDEX {GetSchemaQualifiedIdentifierName(schemaName, indexName)} CASCADE";
    }
    #endregion // Index Strings

    #region View Strings

    protected override (string sql, object parameters) SqlGetViewNames(
        string? schemaName,
        string? viewNameFilter
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"
                SELECT
                    v.viewname as ViewName
                from pg_views as v
                where 
                    v.schemaname not like 'pg_%' 
                    and v.schemaname != 'information_schema'
                    and v.viewname not in ('geography_columns', 'geometry_columns', 'raster_columns', 'raster_overviews')
                    and lower(v.schemaname) = @schemaName
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND lower(v.viewname) LIKE @where")}
                ORDER BY
                    v.schemaname, v.viewname";

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName).ToLowerInvariant(),
                where = where.ToLowerInvariant()
            }
        );
    }

    protected override (string sql, object parameters) SqlGetViews(
        string? schemaName,
        string? viewNameFilter
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"
                SELECT 
                    v.schemaname as SchemaName,
                    v.viewname as ViewName,
                    v.definition as Definition
                from pg_views as v
                where 
                    v.schemaname not like 'pg_%' 
                    and v.schemaname != 'information_schema'     
                    and v.viewname not in ('geography_columns', 'geometry_columns', 'raster_columns', 'raster_overviews')       
                    and lower(v.schemaname) = @schemaName
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND lower(v.viewname) LIKE @where")}
                ORDER BY
                    v.schemaname, v.viewname";

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName).ToLowerInvariant(),
                where = where.ToLowerInvariant()
            }
        );
    }
    #endregion // View Strings
}
