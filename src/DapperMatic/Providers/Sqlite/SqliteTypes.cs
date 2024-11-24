using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Providers.Sqlite;

/// <summary>
/// See: https://www.sqlite.org/datatype3.html
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class SqliteTypes
{
    // integers
    // If the declared type contains the string "INT" then it is assigned INTEGER affinity
    // Deviation from int or integer is only useful to allow backward determination of intended min/max values.
    public const string sql_integer = "integer";
    public const string sql_int = "int";
    public const string sql_tinyint = "tinyint";
    public const string sql_smallint = "smallint";
    public const string sql_mediumint = "mediumint";
    public const string sql_bigint = "bigint";
    public const string sql_unsigned_big_int = "unsigned big int";
    public const string sql_int2 = "int2";
    public const string sql_int4 = "int4";
    public const string sql_int8 = "int8";

    // real
    // If the declared type for a column contains any of the strings "REAL",
    // "FLOA", or "DOUB" then the column has REAL affinity.
    // If no rule applies, the affinity is NUMERIC.
    // Using a `precision/scale` is only useful to allow backward determination of intended precision/scale.
    public const string sql_real = "real";
    public const string sql_double = "double";
    public const string sql_double_precision = "double precision";
    public const string sql_float = "float";
    public const string sql_numeric = "numeric";
    public const string sql_decimal = "decimal";

    // bool
    // bool is not a valid type in sqlite, and therefore is stored as numeric
    public const string sql_bool = "bool";
    public const string sql_boolean = "boolean";

    // datetime
    // datetime is not a valid type in sqlite, and therefore is stored as numeric
    public const string sql_date = "date";
    public const string sql_datetime = "datetime";
    public const string sql_timestamp = "timestamp";
    public const string sql_time = "time";
    public const string sql_year = "year";

    // text
    // If the declared type of the column contains any of the strings "CHAR", "CLOB", or "TEXT" then that column has TEXT affinity.
    // Notice that the type VARCHAR contains the string "CHAR" and is thus assigned TEXT affinity.
    // Using a `length` is only useful to allow backward determination of intended length.
    public const string sql_char = "char";
    public const string sql_nchar = "nchar";
    public const string sql_varchar = "varchar";
    public const string sql_nvarchar = "nvarchar";
    public const string sql_character = "character";
    public const string sql_varying_character = "varying character";
    public const string sql_native_character = "native character";
    public const string sql_text = "text";
    public const string sql_clob = "clob";

    // binary
    // If the declared type for a column contains the string "BLOB" or if no type is specified then the column has affinity BLOB.
    public const string sql_blob = "blob";
}
