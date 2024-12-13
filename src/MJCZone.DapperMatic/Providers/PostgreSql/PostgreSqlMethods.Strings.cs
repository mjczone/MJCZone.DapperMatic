using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    #region Schema Strings

    /// <summary>
    /// Generates SQL to get schema names with an optional filter.
    /// </summary>
    /// <param name="schemaNameFilter">Optional filter for schema names.</param>
    /// <returns>SQL query and parameters.</returns>
    protected override (string sql, object parameters) SqlGetSchemaNames(
        string? schemaNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(schemaNameFilter)
            ? string.Empty
            : ToLikeString(schemaNameFilter);

        var sql = $"""
            SELECT DISTINCT nspname
            FROM pg_catalog.pg_namespace
            {(string.IsNullOrWhiteSpace(where) ? string.Empty : "WHERE lower(nspname) LIKE @where")}
            ORDER BY nspname
            """;

        return (sql, new { where });
    }

    /// <summary>
    /// Generates SQL to drop a schema.
    /// </summary>
    /// <param name="schemaName">Name of the schema to drop.</param>
    /// <returns>SQL query string.</returns>
    protected override string SqlDropSchema(string schemaName)
    {
        return $"DROP SCHEMA IF EXISTS {NormalizeSchemaName(schemaName)} CASCADE";
    }
    #endregion // Schema Strings

    #region Table Strings

    /// <summary>
    /// Generates SQL to define column nullability.
    /// </summary>
    /// <param name="column">Column definition.</param>
    /// <returns>SQL fragment for column nullability.</returns>
    protected override string SqlInlineColumnNullable(DmColumn column)
    {
        // serial columns are implicitly NOT NULL
        if (
            column.IsNullable
            && (column.GetProviderDataType(ProviderType) ?? string.Empty).Contains(
                "serial",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return string.Empty;
        }

        return column.IsNullable && !column.IsUnique && !column.IsPrimaryKey
            ? " NULL"
            : " NOT NULL";
    }

    /// <summary>
    /// Generates SQL for primary key auto-increment constraint.
    /// </summary>
    /// <param name="column">Column definition.</param>
    /// <returns>SQL fragment for primary key auto-increment constraint.</returns>
    protected override string SqlInlinePrimaryKeyAutoIncrementColumnConstraint(DmColumn column)
    {
        if (
            (column.GetProviderDataType(ProviderType) ?? string.Empty).Contains(
                "serial",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return string.Empty;
        }

        return "GENERATED BY DEFAULT AS IDENTITY";
    }

    /// <summary>
    /// Generates SQL to check if a table exists.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="tableName">Table name.</param>
    /// <returns>SQL query and parameters.</returns>
    protected override (string sql, object parameters) SqlDoesTableExist(
        string? schemaName,
        string tableName
    )
    {
        var sql = $"""
               SELECT COUNT(*)
               FROM pg_class pgc
                   JOIN pg_catalog.pg_namespace n ON n.oid = pgc.relnamespace
               WHERE
                   pgc.relkind = 'r'
                   {(
                        string.IsNullOrWhiteSpace(schemaName) ? string.Empty : " AND lower(n.nspname) = @schemaName"
                    )}
                   AND lower(pgc.relname) = @tableName
            """;

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName),
                tableName = NormalizeName(tableName),
            }
        );
    }

    /// <summary>
    /// Generates SQL to get table names with optional filters.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="tableNameFilter">Optional filter for table names.</param>
    /// <returns>SQL query and parameters.</returns>
    protected override (string sql, object parameters) SqlGetTableNames(
        string? schemaName,
        string? tableNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? string.Empty
            : ToLikeString(tableNameFilter);

        var sql = $"""
                    SELECT TABLE_NAME
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE
                        TABLE_TYPE = 'BASE TABLE'
                        AND lower(TABLE_SCHEMA) = @schemaName
                        AND TABLE_NAME NOT IN ('spatial_ref_sys', 'geometry_columns', 'geography_columns', 'raster_columns', 'raster_overviews')
                        {(
                            string.IsNullOrWhiteSpace(where) ? null : " AND lower(TABLE_NAME) LIKE @where"
                        )}
                        ORDER BY TABLE_NAME
            """;

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName).ToLowerInvariant(),
                where = where.ToLowerInvariant(),
            }
        );
    }

    /// <summary>
    /// Generates SQL to drop a table.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="tableName">Table name.</param>
    /// <returns>SQL query string.</returns>
    protected override string SqlDropTable(string? schemaName, string tableName)
    {
        return $"DROP TABLE IF EXISTS {GetSchemaQualifiedIdentifierName(schemaName, tableName)} CASCADE";
    }
    #endregion // Table Strings

    #region Column Strings
    #endregion // Column Strings

    #region Check Constraint Strings
    #endregion // Check Constraint Strings

    #region Default Constraint Strings

    /// <summary>
    /// Generates SQL to add a default constraint to a table.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="columnName">Column name.</param>
    /// <param name="constraintName">Constraint name.</param>
    /// <param name="expression">Default expression.</param>
    /// <returns>SQL query string.</returns>
    protected override string SqlAlterTableAddDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression
    )
    {
        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        return $"""

                        ALTER TABLE {schemaQualifiedTableName}
                            ALTER COLUMN {NormalizeName(columnName)} SET DEFAULT {expression}
            
            """;
    }

    /// <summary>
    /// Generates SQL to drop a default constraint from a table.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="columnName">Column name.</param>
    /// <param name="constraintName">Constraint name.</param>
    /// <returns>SQL query string.</returns>
    protected override string SqlDropDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName
    )
    {
        return $"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ALTER COLUMN {NormalizeName(columnName)} DROP DEFAULT";
    }
    #endregion // Default Constraint Strings

    #region Primary Key Strings
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    #endregion // Unique Constraint Strings

    #region Foreign Key Constraint Strings
    #endregion // Foreign Key Constraint Strings

    #region Index Strings

    /// <summary>
    /// Generates SQL to drop an index.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="tableName">Table name.</param>
    /// <param name="indexName">Index name.</param>
    /// <returns>SQL query string.</returns>
    protected override string SqlDropIndex(string? schemaName, string tableName, string indexName)
    {
        return $"DROP INDEX {GetSchemaQualifiedIdentifierName(schemaName, indexName)} CASCADE";
    }
    #endregion // Index Strings

    #region View Strings

    /// <summary>
    /// Generates SQL to get view names with optional filters.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="viewNameFilter">Optional filter for view names.</param>
    /// <returns>SQL query and parameters.</returns>
    protected override (string sql, object parameters) SqlGetViewNames(
        string? schemaName,
        string? viewNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter)
            ? string.Empty
            : ToLikeString(viewNameFilter);

        var sql = $"""

                            SELECT
                                v.viewname as ViewName
                            from pg_views as v
                            where
                                v.schemaname not like 'pg_%'
                                and v.schemaname != 'information_schema'
                                and v.viewname not in ('geography_columns', 'geometry_columns', 'raster_columns', 'raster_overviews')
                                and lower(v.schemaname) = @schemaName
                                {(
                string.IsNullOrWhiteSpace(where) ? string.Empty : " AND lower(v.viewname) LIKE @where"
            )}
                            ORDER BY
                                v.schemaname, v.viewname
            """;

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName).ToLowerInvariant(),
                where = where.ToLowerInvariant(),
            }
        );
    }

    /// <summary>
    /// Generates SQL to get views with optional filters.
    /// </summary>
    /// <param name="schemaName">Schema name.</param>
    /// <param name="viewNameFilter">Optional filter for view names.</param>
    /// <returns>SQL query and parameters.</returns>
    protected override (string sql, object parameters) SqlGetViews(
        string? schemaName,
        string? viewNameFilter
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter)
            ? string.Empty
            : ToLikeString(viewNameFilter);

        var sql = $"""

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
                                {(
                string.IsNullOrWhiteSpace(where) ? string.Empty : " AND lower(v.viewname) LIKE @where"
            )}
                            ORDER BY
                                v.schemaname, v.viewname
            """;

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName).ToLowerInvariant(),
                where = where.ToLowerInvariant(),
            }
        );
    }
    #endregion // View Strings
}