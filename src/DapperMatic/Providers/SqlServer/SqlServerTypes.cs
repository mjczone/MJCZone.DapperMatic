using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Providers.SqlServer;

/// <summary>
/// See: https://learn.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql?view=sql-server-ver16
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class SqlServerTypes
{
    // bool (in SQL Server, bit is a 0 or 1)
    public const string sql_bit = "bit";

    // integers
    public const string sql_tinyint = "tinyint";
    public const string sql_smallint = "smallint";
    public const string sql_int = "int";
    public const string sql_bigint = "bigint";

    // real
    public const string sql_float = "float";
    public const string sql_real = "real";
    public const string sql_decimal = "decimal";
    public const string sql_numeric = "numeric";
    public const string sql_money = "money";
    public const string sql_smallmoney = "smallmoney";

    // datetime
    public const string sql_date = "date";
    public const string sql_datetime = "datetime";
    public const string sql_smalldatetime = "smalldatetime";
    public const string sql_datetime2 = "datetime2";
    public const string sql_datetimeoffset = "datetimeoffset";
    public const string sql_time = "time";
    public const string sql_timestamp = "timestamp";
    public const string sql_rowversion = "rowversion";

    // guid
    public const string sql_uniqueidentifier = "uniqueidentifier";

    // text
    public const string sql_char = "char";
    public const string sql_varchar = "varchar";
    public const string sql_text = "text";
    public const string sql_nchar = "nchar";
    public const string sql_nvarchar = "nvarchar";
    public const string sql_ntext = "ntext";

    // binary
    public const string sql_binary = "binary";
    public const string sql_varbinary = "varbinary";
    public const string sql_image = "image";

    // geometry
    public const string sql_geometry = "geometry";
    public const string sql_geography = "geography";
    public const string sql_hierarchyid = "hierarchyid";

    // other data types
    public const string sql_variant = "sql_variant";
    public const string sql_xml = "xml";
    public const string sql_cursor = "cursor";
    public const string sql_table = "table";
    public const string sql_json = "json";
}
