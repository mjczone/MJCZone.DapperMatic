using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Providers.PostgreSql;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class PostgreSqlTypes
{
    // integers
    public const string sql_smallint = "smallint";
    public const string sql_int2 = "int2";
    public const string sql_smallserial = "smallserial";
    public const string sql_serial2 = "serial2";
    public const string sql_integer = "integer";
    public const string sql_int = "int";
    public const string sql_int4 = "int4";
    public const string sql_serial = "serial";
    public const string sql_serial4 = "serial4";
    public const string sql_bigint = "bigint";
    public const string sql_int8 = "int8";
    public const string sql_bigserial = "bigserial";
    public const string sql_serial8 = "serial8";

    // real
    public const string sql_float4 = "float4";
    public const string sql_real = "real";
    public const string sql_double_precision = "double precision";
    public const string sql_float8 = "float8";
    public const string sql_money = "money";
    public const string sql_numeric = "numeric";
    public const string sql_decimal = "decimal";

    // bool
    public const string sql_bool = "bool";
    public const string sql_boolean = "boolean";

    // datetime
    public const string sql_date = "date";
    public const string sql_interval = "interval";
    public const string sql_time_without_timezone = "time without timezone";
    public const string sql_time = "time";
    public const string sql_time_with_time_zone = "time with time zone";
    public const string sql_timetz = "timetz";
    public const string sql_timestamp_without_time_zone = "timestamp without time zone";
    public const string sql_timestamp = "timestamp";
    public const string sql_timestamp_with_time_zone = "timestamp with time zone";
    public const string sql_timestamptz = "timestamptz";

    // text
    public const string sql_bit = "bit";
    public const string sql_bit_varying = "bit varying";
    public const string sql_varbit = "varbit";
    public const string sql_character_varying = "character varying";
    public const string sql_varchar = "varchar";
    public const string sql_character = "character";
    public const string sql_char = "char";
    public const string sql_bpchar = "bpchar";
    public const string sql_text = "text";
    public const string sql_name = "name";
    public const string sql_uuid = "uuid";
    public const string sql_json = "json";
    public const string sql_jsonb = "jsonb";
    public const string sql_jsonpath = "jsonpath";
    public const string sql_xml = "xml";

    // binary
    public const string sql_bytea = "bytea";

    // geometry
    public const string sql_box = "box";
    public const string sql_circle = "circle";
    public const string sql_geography = "geography";
    public const string sql_geometry = "geometry";
    public const string sql_line = "line";
    public const string sql_lseg = "lseg";
    public const string sql_path = "path";
    public const string sql_point = "point";
    public const string sql_polygon = "polygon";

    // range types
    public const string sql_datemultirange = "datemultirange";
    public const string sql_daterange = "daterange";
    public const string sql_int4multirange = "int4multirange";
    public const string sql_int4range = "int4range";
    public const string sql_int8multirange = "int8multirange";
    public const string sql_int8range = "int8range";
    public const string sql_nummultirange = "nummultirange";
    public const string sql_numrange = "numrange";
    public const string sql_tsmultirange = "tsmultirange";
    public const string sql_tsrange = "tsrange";
    public const string sql_tstzmultirange = "tstzmultirange";
    public const string sql_tstzrange = "tstzrange";

    // other data types
    public const string sql_cidr = "cidr";
    public const string sql_citext = "citext";
    public const string sql_hstore = "hstore";
    public const string sql_inet = "inet";
    public const string sql_int2vector = "int2vector";
    public const string sql_lquery = "lquery";
    public const string sql_ltree = "ltree";
    public const string sql_ltxtquery = "ltxtquery";
    public const string sql_macaddr = "macaddr";
    public const string sql_macaddr8 = "macaddr8";
    public const string sql_oid = "oid";
    public const string sql_oidvector = "oidvector";
    public const string sql_pg_lsn = "pg_lsn";
    public const string sql_pg_snapshot = "pg_snapshot";
    public const string sql_refcursor = "refcursor";
    public const string sql_regclass = "regclass";
    public const string sql_regcollation = "regcollation";
    public const string sql_regconfig = "regconfig";
    public const string sql_regdictionary = "regdictionary";
    public const string sql_regnamespace = "regnamespace";
    public const string sql_regrole = "regrole";
    public const string sql_regtype = "regtype";
    public const string sql_tid = "tid";
    public const string sql_tsquery = "tsquery";
    public const string sql_tsvector = "tsvector";
    public const string sql_txid_snapshot = "txid_snapshot";
    public const string sql_xid = "xid";
    public const string sql_xid8 = "xid8";
}
