using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DapperMatic.Providers.PostgreSql;

// https://www.npgsql.org/doc/types/basic.html#read-mappings
// https://www.npgsql.org/doc/types/basic.html#write-mappings
public sealed class PostgreSqlProviderTypeMap : DbProviderTypeMapBase
{
    internal static readonly Lazy<PostgreSqlProviderTypeMap> Instance =
        new(() => new PostgreSqlProviderTypeMap());

    private PostgreSqlProviderTypeMap()
        : base() { }

    protected override DbProviderType ProviderType => DbProviderType.PostgreSql;

    public override string SqTypeForStringLengthMax => "text";

    public override string SqTypeForBinaryLengthMax => "bytea";

    public override string SqlTypeForJson => "jsonb";

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override DbProviderSqlType[] ProviderSqlTypes =>
        [
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_smallint,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int2,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_integer,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int4,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_int8,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_bigint,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_smallserial,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial2,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial4,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_bigserial,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 9223372036854775807
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                PostgreSqlTypes.sql_serial8,
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: 9223372036854775807
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_real,
                minValue: float.MinValue,
                maxValue: float.MaxValue
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_double_precision,
                minValue: double.MinValue,
                maxValue: double.MaxValue
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_float4,
                minValue: float.MinValue,
                maxValue: float.MaxValue
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_float8,
                minValue: double.MinValue,
                maxValue: double.MaxValue
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_money,
                formatWithPrecision: "money({0})",
                defaultPrecision: 19,
                minValue: -92233720368547758.08,
                maxValue: 92233720368547758.07
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_numeric,
                formatWithPrecision: "numeric({0})",
                formatWithPrecisionAndScale: "numeric({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                PostgreSqlTypes.sql_decimal,
                formatWithPrecision: "decimal({0})",
                formatWithPrecisionAndScale: "decimal({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Boolean,
                PostgreSqlTypes.sql_bool,
                canUseToAutoIncrement: false
            ),
            new(DbProviderSqlTypeAffinity.Boolean, PostgreSqlTypes.sql_boolean),
            new(DbProviderSqlTypeAffinity.DateTime, PostgreSqlTypes.sql_date, isDateOnly: true),
            new(DbProviderSqlTypeAffinity.DateTime, PostgreSqlTypes.sql_interval),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_time_without_timezone,
                formatWithPrecision: "time({0}) without timezone",
                defaultPrecision: 6
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_time,
                formatWithPrecision: "time({0})",
                defaultPrecision: 6,
                isTimeOnly: true
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_time_with_time_zone,
                formatWithPrecision: "time({0}) with time zone",
                defaultPrecision: 6
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timetz,
                formatWithPrecision: "timetz({0})",
                defaultPrecision: 6,
                isTimeOnly: true
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamp_without_time_zone,
                formatWithPrecision: "timestamp({0}) without time zone",
                defaultPrecision: 6
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamp,
                formatWithPrecision: "timestamp({0})",
                defaultPrecision: 6
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamp_with_time_zone,
                formatWithPrecision: "timestamp({0}) with time zone",
                defaultPrecision: 6
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                PostgreSqlTypes.sql_timestamptz,
                formatWithPrecision: "timestamptz({0})",
                defaultPrecision: 6
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_varchar,
                formatWithLength: "varchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_character_varying,
                formatWithLength: "character varying({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_character,
                formatWithLength: "character({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_char,
                formatWithLength: "char({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_bpchar,
                formatWithLength: "bpchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_bit,
                formatWithPrecision: "bit({0})",
                defaultPrecision: 1,
                minValue: 0,
                maxValue: 1
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_bit_varying,
                formatWithPrecision: "bit varying({0})",
                defaultPrecision: 63
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                PostgreSqlTypes.sql_varbit,
                formatWithPrecision: "varbit({0})",
                defaultPrecision: 63
            ),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_text),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_name),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_uuid, isGuidOnly: true),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_json),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_jsonb),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_jsonpath),
            new(DbProviderSqlTypeAffinity.Text, PostgreSqlTypes.sql_xml),
            new(DbProviderSqlTypeAffinity.Binary, PostgreSqlTypes.sql_bytea),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_box),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_circle),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_geography),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_geometry),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_line),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_lseg),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_path),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_point),
            new(DbProviderSqlTypeAffinity.Geometry, PostgreSqlTypes.sql_polygon),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_datemultirange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_daterange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int4multirange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int4range),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int8multirange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_int8range),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_nummultirange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_numrange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tsmultirange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tsrange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tstzmultirange),
            new(DbProviderSqlTypeAffinity.RangeType, PostgreSqlTypes.sql_tstzrange),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_cidr),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_citext),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_hstore),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_inet),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_int2vector),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_lquery),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_ltree),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_ltxtquery),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_macaddr),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_macaddr8),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_oid),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_oidvector),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_pg_lsn),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_pg_snapshot),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_refcursor),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regclass),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regcollation),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regconfig),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regdictionary),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regnamespace),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regrole),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_regtype),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_tid),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_tsquery),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_tsvector),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_txid_snapshot),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_xid),
            new(DbProviderSqlTypeAffinity.Other, PostgreSqlTypes.sql_xid8),
        ];

    protected override bool TryGetProviderSqlTypeMatchingDotnetTypeInternal(
        DbProviderDotnetTypeDescriptor descriptor,
        out DbProviderSqlType? providerSqlType
    )
    {
        providerSqlType = null;

        var dotnetType = descriptor.DotnetType;

        // handle well-known types first
        providerSqlType = dotnetType.IsGenericType
            ? null
            : dotnetType switch
            {
                Type t when t == typeof(bool) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_boolean],
                Type t when t == typeof(byte) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                Type t when t == typeof(ReadOnlyMemory<byte>)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                Type t when t == typeof(sbyte) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                // it's no longer recommended to use SERIAL auto-incrementing columns
                // Type t when t == typeof(short)
                //     => descriptor.AutoIncrement.GetValueOrDefault(false)
                //         ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_serial2]
                //         : ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                // Type t when t == typeof(ushort)
                //     => descriptor.AutoIncrement.GetValueOrDefault(false)
                //         ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_serial2]
                //         : ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                // Type t when t == typeof(int)
                //     => descriptor.AutoIncrement.GetValueOrDefault(false)
                //         ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_serial4]
                //         : ProviderSqlTypeLookup[PostgreSqlTypes.sql_int4],
                // Type t when t == typeof(uint)
                //     => descriptor.AutoIncrement.GetValueOrDefault(false)
                //         ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_serial4]
                //         : ProviderSqlTypeLookup[PostgreSqlTypes.sql_int4],
                // Type t when t == typeof(long)
                //     => descriptor.AutoIncrement.GetValueOrDefault(false)
                //         ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_serial8]
                //         : ProviderSqlTypeLookup[PostgreSqlTypes.sql_int8],
                // Type t when t == typeof(ulong)
                //     => descriptor.AutoIncrement.GetValueOrDefault(false)
                //         ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_serial8]
                //         : ProviderSqlTypeLookup[PostgreSqlTypes.sql_int8],
                Type t when t == typeof(short)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                Type t when t == typeof(ushort) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int2],
                Type t when t == typeof(int) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int4],
                Type t when t == typeof(uint) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int4],
                Type t when t == typeof(long) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int8],
                Type t when t == typeof(ulong) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int8],
                Type t when t == typeof(float) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_float4],
                Type t when t == typeof(double)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_float8],
                Type t when t == typeof(decimal)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_numeric],
                Type t when t == typeof(char) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar],
                Type t when t == typeof(string)
                    => descriptor.Length.GetValueOrDefault(255) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(char[])
                    => descriptor.Length.GetValueOrDefault(255) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(ReadOnlyMemory<byte>[])
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(Stream)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(TextReader)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(byte[]) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_bytea],
                Type t when t == typeof(object)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(object[])
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[PostgreSqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[PostgreSqlTypes.sql_text],
                Type t when t == typeof(Guid) => ProviderSqlTypeLookup[PostgreSqlTypes.sql_uuid],
                Type t when t == typeof(DateTime)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_timestamp],
                Type t when t == typeof(DateTimeOffset)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_timestamptz],
                Type t when t == typeof(TimeSpan)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_interval],
                Type t when t == typeof(DateOnly)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_date],
                Type t when t == typeof(TimeOnly)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_time],
                Type t when t == typeof(BitArray) || t == typeof(BitVector32)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_varbit],
                Type t
                    when t == typeof(ImmutableDictionary<string, string>)
                        || t == typeof(Dictionary<string, string>)
                        || t == typeof(IDictionary<string, string>)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_hstore],
                Type t
                    when t == typeof(JsonNode)
                        || t == typeof(JsonObject)
                        || t == typeof(JsonArray)
                        || t == typeof(JsonValue)
                        || t == typeof(JsonDocument)
                        || t == typeof(JsonElement)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_jsonb],
                _ => null
            };

        if (providerSqlType != null)
            return true;

        // handle generic types
        providerSqlType = !dotnetType.IsGenericType
            ? null
            : dotnetType.GetGenericTypeDefinition() switch
            {
                Type t
                    when t == typeof(Dictionary<,>)
                        || t == typeof(IDictionary<,>)
                        || t == typeof(List<>)
                        || t == typeof(IList<>)
                        || t == typeof(Collection<>)
                        || t == typeof(ICollection<>)
                        || t == typeof(IEnumerable<>)
                    => ProviderSqlTypeLookup[PostgreSqlTypes.sql_jsonb],
                Type t when t.Name.StartsWith("NpgsqlRange")
                    => dotnetType.GetGenericArguments().First() switch
                    {
                        Type at when at == typeof(DateOnly)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_daterange],
                        Type at when at == typeof(int)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int4range],
                        Type at when at == typeof(long)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_int8range],
                        Type at when at == typeof(decimal)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_numrange],
                        Type at when at == typeof(float)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_numrange],
                        Type at when at == typeof(double)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_numrange],
                        Type at when at == typeof(DateTime)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_tsrange],
                        Type at when at == typeof(DateTimeOffset)
                            => ProviderSqlTypeLookup[PostgreSqlTypes.sql_tstzrange],
                        _ => null
                    },
                _ => null
            };

        if (providerSqlType != null)
            return true;

        // Handle Npgsql types
        switch (dotnetType.FullName)
        {
            case "System.Net.IPAddress":
            case "NpgsqlTypes.NpgsqlInet":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_inet];
                break;
            case "NpgsqlTypes.NpgsqlCidr":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_cidr];
                break;
            case "System.Net.NetworkInformation.PhysicalAddress":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_macaddr8];
                // providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_macaddr];
                break;
            case "NpgsqlTypes.NpgsqlPoint":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_point];
                break;
            case "NpgsqlTypes.NpgsqlLSeg":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_lseg];
                break;
            case "NpgsqlTypes.NpgsqlPath":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_path];
                break;
            case "NpgsqlTypes.NpgsqlPolygon":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_polygon];
                break;
            case "NpgsqlTypes.NpgsqlLine":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_line];
                break;
            case "NpgsqlTypes.NpgsqlCircle":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_circle];
                break;
            case "NpgsqlTypes.NpgsqlBox":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_box];
                break;
            case "NetTopologySuite.Geometries.Geometry":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_geometry];
                break;
            case "NpgsqlTypes.NpgsqlInterval":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_interval];
                break;
            case "NpgsqlTypes.NpgsqlTid":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_tid];
                break;
            case "NpgsqlTypes.NpgsqlTsQuery":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_tsquery];
                break;
            case "NpgsqlTypes.NpgsqlTsVector":
                providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_tsvector];
                break;
        }

        if (providerSqlType != null)
            return true;

        // handle array types
        var elementType = dotnetType.IsArray ? dotnetType.GetElementType() : null;
        if (
            elementType != null
            && TryGetProviderSqlTypeMatchingDotnetTypeInternal(
                new DbProviderDotnetTypeDescriptor(elementType),
                out var elementProviderSqlType
            )
            && elementProviderSqlType != null
        )
        {
            switch (elementProviderSqlType.Name)
            {
                case PostgreSqlTypes.sql_tsrange:
                    providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_tsmultirange];
                    break;
                case PostgreSqlTypes.sql_numrange:
                    providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_nummultirange];
                    break;
                case PostgreSqlTypes.sql_daterange:
                    providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_datemultirange];
                    break;
                case PostgreSqlTypes.sql_int4range:
                    providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_int4multirange];
                    break;
                case PostgreSqlTypes.sql_int8range:
                    providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_int8multirange];
                    break;
                case PostgreSqlTypes.sql_tstzrange:
                    providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_tstzmultirange];
                    break;
                default:
                    // in postgresql, we can have array types that end with [] or ARRA
                    providerSqlType = new DbProviderSqlType(
                        DbProviderSqlTypeAffinity.Other,
                        $"{elementProviderSqlType.Name}[]"
                    );
                    break;
            }
        }

        if (providerSqlType != null)
            return true;

        // handle POCO type
        if (dotnetType.IsClass || dotnetType.IsInterface)
        {
            providerSqlType = ProviderSqlTypeLookup[PostgreSqlTypes.sql_jsonb];
        }

        return providerSqlType != null;
    }

    protected override bool TryGetProviderSqlTypeFromFullSqlTypeName(
        string fullSqlType,
        out DbProviderSqlType? providerSqlType
    )
    {
        if (base.TryGetProviderSqlTypeFromFullSqlTypeName(fullSqlType, out providerSqlType))
            return true;

        providerSqlType = null;

        // PostgreSql (unlike other providers) supports array types for (almost) all its types
        if (fullSqlType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
        {
            var elementTypeName = fullSqlType.Substring(0, fullSqlType.Length - 2);
            if (
                TryGetProviderSqlTypeFromFullSqlTypeName(
                    elementTypeName,
                    out var elementProviderSqlType
                )
                && elementProviderSqlType != null
            )
            {
                providerSqlType = new DbProviderSqlType(
                    DbProviderSqlTypeAffinity.Other,
                    $"{elementProviderSqlType.Name}[]"
                );
                return true;
            }
        }

        return false;
    }
}
