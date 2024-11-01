using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Providers.MySql;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class MySqlTypes
{
    // integers
    public const string sql_tinyint = "tinyint";
    public const string sql_tinyint_unsigned = "tinyint unsigned";
    public const string sql_smallint = "smallint";
    public const string sql_smallint_unsigned = "smallint unsigned";
    public const string sql_mediumint = "mediumint";
    public const string sql_mediumint_unsigned = "mediumint unsigned";
    public const string sql_integer = "integer";
    public const string sql_integer_unsigned = "integer unsigned";
    public const string sql_int = "int";
    public const string sql_int_unsigned = "int unsigned";
    public const string sql_bigint = "bigint";
    public const string sql_bigint_unsigned = "bigint unsigned";
    public const string sql_serial = "serial";
    
    // real
    public const string sql_decimal = "decimal";
    public const string sql_dec = "dec";
    public const string sql_fixed = "fixed";
    public const string sql_numeric = "numeric";
    public const string sql_float = "float";
    public const string sql_real = "real";
    public const string sql_double_precision = "double precision";
    public const string sql_double_precision_unsigned = "double precision unsigned";
    public const string sql_double = "double";
    public const string sql_double_unsigned = "double unsigned";
    public const string sql_bit = "bit";
    
    // bool
    public const string sql_bool = "bool";
    public const string sql_boolean = "boolean";
    
    // datetime
    public const string sql_datetime = "datetime";
    public const string sql_timestamp = "timestamp";
    public const string sql_time = "time";
    public const string sql_date = "date";
    public const string sql_year = "year";
    
    // text
    public const string sql_char = "char";
    public const string sql_varchar = "varchar";
    public const string sql_long_varchar = "long varchar";
    public const string sql_tinytext = "tinytext";
    public const string sql_text = "text";
    public const string sql_mediumtext = "mediumtext";
    public const string sql_longtext = "longtext";
    public const string sql_enum = "enum";
    public const string sql_set = "set";
    public const string sql_json = "json";
    
    // binary
    public const string sql_binary = "binary";
    public const string sql_varbinary = "varbinary";
    public const string sql_long_varbinary = "long varbinary";
    public const string sql_tinyblob = "tinyblob";
    public const string sql_blob = "blob";
    public const string sql_mediumblob = "mediumblob";
    public const string sql_longblob = "longblob";
    
    // geometry
    public const string sql_geometry = "geometry";
    public const string sql_point = "point";
    public const string sql_linestring = "linestring";
    public const string sql_polygon = "polygon";
    public const string sql_multipoint = "multipoint";
    public const string sql_multilinestring = "multilinestring";
    public const string sql_multipolygon = "multipolygon";
    public const string sql_geomcollection = "geomcollection";
    public const string sql_geometrycollection = "geometrycollection";
}