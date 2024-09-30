namespace DapperMatic.Providers.PostgreSql;

public static class PostgreSqlSqlParser
{
    public static Type GetDotnetTypeFromSqlType(string sqlType)
    {
        var simpleSqlType = sqlType.Split('(')[0].ToLower();

        var match = DataTypeMapFactory
            .GetDefaultDbProviderDataTypeMap(DbProviderType.PostgreSql)
            .FirstOrDefault(x =>
                x.SqlType.Equals(simpleSqlType, StringComparison.OrdinalIgnoreCase)
            )
            ?.DotnetType;

        if (match != null)
            return match;

        // SQLServer specific types, see https://learn.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql?view=sql-server-ver16
        switch (simpleSqlType)
        {
            case "uniqueidentifier":
                return typeof(Guid);
            case "int":
                return typeof(int);
            case "tinyint":
            case "smallint":
                return typeof(short);
            case "bigint":
                return typeof(long);
            case "char":
            case "nchar":
            case "varchar":
            case "nvarchar":
            case "text":
            case "ntext":
            case "xml":
            case "json":
                return typeof(string);
            case "image":
            case "binary":
            case "varbinary":
                return typeof(byte[]);
            case "real":
            case "double":
                return typeof(double);
            case "decimal":
            case "numeric":
            case "money":
            case "smallmoney":
            case "float":
                return typeof(decimal);
            case "date":
            case "time":
            case "datetime2":
            case "datetimeoffset":
            case "datetime":
            case "smalldatetime":
                return typeof(DateTime);
            case "boolean":
            case "bool":
            case "bit":
                return typeof(bool);
            case "sql_variant":
            case "table":
            case "hierarchyid":
            case "geometry":
            case "geography":
            case "cursor":
            default:
                // If no match, default to object
                return typeof(object);
        }
    }
}
