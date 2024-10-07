namespace DapperMatic.Providers.MySql;

public static class MySqlSqlParser
{
    public static Type GetDotnetTypeFromSqlType(string sqlType)
    {
        var simpleSqlType = sqlType.Split('(')[0].ToLower();

        var match = DataTypeMapFactory
            .GetDefaultDbProviderDataTypeMap(DbProviderType.MySql)
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
            case "integer":
            case "mediumint":
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
            case "tinytext":
            case "mediumtext":
            case "longtext":
            case "text":
            case "ntext":
            case "xml":
            case "json":
            case "enum":
            case "set":
                return typeof(string);
            case "image":
            case "binary":
            case "varbinary":
                return typeof(byte[]);
            case "real":
            case "double":
            case "double precision":
                return typeof(double);
            case "dec":
            case "fixed":
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
            case "timestamp":
            case "year":
                return typeof(DateTime);
            case "boolean":
            case "bool":
            case "bit":
                return typeof(bool);
            case "table":
            case "hierarchyid":
            case "tinyblob":
            case "mediumblob":
            case "longblob":
            case "blob":
            case "geometry":
            case "point":
            case "curve":
            case "linestring":
            case "surface":
            case "polygon":
            case "geometrycollection":
            case "multipoint":
            case "multicurve":
            case "multilinestring":
            case "multisurface":
            case "multipolygon":
            default:
                // If no match, default to object
                return typeof(object);
        }
    }
}
