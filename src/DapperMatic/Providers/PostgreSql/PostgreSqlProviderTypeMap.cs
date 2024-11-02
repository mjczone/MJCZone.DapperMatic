namespace DapperMatic.Providers.PostgreSql;

// https://www.npgsql.org/doc/types/basic.html#read-mappings
// https://www.npgsql.org/doc/types/basic.html#write-mappings
public sealed class PostgreSqlProviderTypeMap : ProviderTypeMapBase
{
    internal static readonly Lazy<PostgreSqlProviderTypeMap> Instance =
        new(() => new PostgreSqlProviderTypeMap());

    private PostgreSqlProviderTypeMap()
        : base() { }

    protected override DbProviderType ProviderType => DbProviderType.PostgreSql;

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override ProviderSqlType[] ProviderSqlTypes =>
        [
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_smallint,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int2,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_integer,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int4,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int8,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_bigint,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_smallserial,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 32767
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial2,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 32767
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 2147483647
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial4,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 2147483647
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_bigserial,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 9223372036854775807
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial8,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 9223372036854775807
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_real,
                minValue: float.MinValue,
                maxValue: float.MaxValue
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_double_precision,
                minValue: double.MinValue,
                maxValue: double.MaxValue
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_float4,
                minValue: float.MinValue,
                maxValue: float.MaxValue
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_float8,
                minValue: double.MinValue,
                maxValue: double.MaxValue
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_money,
                formatWithPrecision: "money({0})",
                defaultPrecision: 19,
                minValue: -92233720368547758.08,
                maxValue: 92233720368547758.07
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_numeric,
                formatWithPrecision: "numeric({0})",
                formatWithPrecisionAndScale: "numeric({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_decimal,
                formatWithPrecision: "decimal({0})",
                formatWithPrecisionAndScale: "decimal({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                ProviderSqlTypeAffinity.Boolean,
                PostgreSqlTypes.sql_bool,
                canUseToAutoIncrement: false
            ),
            new(ProviderSqlTypeAffinity.Boolean, PostgreSqlTypes.sql_boolean),
            new(ProviderSqlTypeAffinity.DateTime, PostgreSqlTypes.sql_date, isDateOnly: true),
            new(ProviderSqlTypeAffinity.DateTime, PostgreSqlTypes.sql_interval),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_time_without_timezone,
                formatWithPrecision: "time({0}) without timezone",
                defaultPrecision: 6
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_time,
                formatWithPrecision: "time({0})",
                defaultPrecision: 6,
                isTimeOnly: true
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_time_with_time_zone,
                formatWithPrecision: "time({0}) with time zone",
                defaultPrecision: 6
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timetz,
                formatWithPrecision: "timetz({0})",
                defaultPrecision: 6,
                isTimeOnly: true
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamp_without_time_zone,
                formatWithPrecision: "timestamp({0}) without time zone",
                defaultPrecision: 6
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamp,
                formatWithPrecision: "timestamp({0})",
                defaultPrecision: 6
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamp_with_time_zone,
                formatWithPrecision: "timestamp({0}) with time zone",
                defaultPrecision: 6
            ),
            new(
                ProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamptz,
                formatWithPrecision: "timestamptz({0})",
                defaultPrecision: 6
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_bit,
                formatWithPrecision: "bit({0})",
                defaultPrecision: 1,
                minValue: 0,
                maxValue: 1
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_bit_varying,
                formatWithPrecision: "bit varying({0})",
                defaultPrecision: 63
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_varbit,
                formatWithPrecision: "varbit({0})",
                defaultPrecision: 63
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_character_varying,
                formatWithLength: "character varying({0})",
                defaultLength: 255
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_varchar,
                formatWithLength: "varchar({0})",
                defaultLength: 255
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_character,
                formatWithLength: "character({0})",
                defaultLength: 1
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_char,
                formatWithLength: "char({0})",
                defaultLength: 1
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_bpchar,
                formatWithLength: "bpchar({0})",
                defaultLength: 1
            ),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_text),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_name),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_uuid, isGuidOnly: true),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_json),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_jsonb),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_jsonpath),
            new(ProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_xml),
            new(ProviderSqlTypeAffinity.Binary, PostgreSqlTypes.sql_bytea),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_box),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_circle),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_geography),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_geometry),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_line),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_lseg),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_path),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_point),
            new(ProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_polygon),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_datemultirange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_daterange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int4multirange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int4range),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int8multirange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int8range),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_nummultirange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_numrange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tsmultirange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tsrange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tstzmultirange),
            new(ProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tstzrange),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_cidr),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_citext),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_hstore),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_inet),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_int2vector),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_lquery),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_ltree),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_ltxtquery),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_macaddr),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_macaddr8),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_oid),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_oidvector),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_pg_lsn),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_pg_snapshot),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_refcursor),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regclass),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regcollation),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regconfig),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regdictionary),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regnamespace),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regrole),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regtype),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_tid),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_tsquery),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_tsvector),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_txid_snapshot),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_xid),
            new(ProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_xid8),
        ];
}
