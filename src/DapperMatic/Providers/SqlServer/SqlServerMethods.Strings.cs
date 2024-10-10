namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods
{
    #region Schema Strings
    #endregion // Schema Strings

    #region Table Strings
    protected override string SqlRenameTable(
        string? schemaName,
        string tableName,
        string newTableName
    )
    {
        return $@"EXEC sp_rename '{GetSchemaQualifiedIdentifierName(schemaName, tableName)}', '{NormalizeName(newTableName)}'";
    }
    #endregion // Table Strings

    #region Column Strings
    #endregion // Column Strings

    #region Check Constraint Strings
    #endregion // Check Constraint Strings

    #region Default Constraint Strings
    #endregion // Default Constraint Strings

    #region Primary Key Strings
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    #endregion // Unique Constraint Strings

    #region Foreign Key Constraint Strings
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
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
                v.[name] AS ViewName
            FROM sys.objects v
                INNER JOIN sys.sql_modules m ON v.object_id = m.object_id
            WHERE
                v.[type] = 'V'
                AND v.is_ms_shipped = 0                
                AND SCHEMA_NAME(v.schema_id) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? "" : " AND v.[name] LIKE @where")}
            ORDER BY
                SCHEMA_NAME(v.schema_id),
                v.[name]";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
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
                SCHEMA_NAME(v.schema_id) AS SchemaName,
                v.[name] AS ViewName,
                m.definition AS Definition
            FROM sys.objects v
                INNER JOIN sys.sql_modules m ON v.object_id = m.object_id
            WHERE
                v.[type] = 'V'
                AND v.is_ms_shipped = 0                
                AND SCHEMA_NAME(v.schema_id) = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? "" : " AND v.[name] LIKE @where")}
            ORDER BY
                SCHEMA_NAME(v.schema_id),
                v.[name]";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    static readonly char[] WhiteSpaceCharacters = [' ', '\t', '\n', '\r'];

    protected override string NormalizeViewDefinition(string definition)
    {
        definition = definition.Trim();

        // strip off the CREATE VIEW statement ending with the AS
        var indexOfAs = -1;
        for (var i = 0; i < definition.Length; i++)
        {
            if (i == 0)
                continue;
            if (i == definition.Length - 2)
                break;

            if (
                WhiteSpaceCharacters.Contains(definition[i - 1])
                && char.ToUpperInvariant(definition[i]) == 'A'
                && char.ToUpperInvariant(definition[i + 1]) == 'S'
                && WhiteSpaceCharacters.Contains(definition[i + 2])
            )
            {
                indexOfAs = i;
                break;
            }
        }
        if (indexOfAs == -1)
            throw new Exception("Could not parse view definition: " + definition);

        return definition[(indexOfAs + 3)..].Trim();
    }
    #endregion // View Strings
}
