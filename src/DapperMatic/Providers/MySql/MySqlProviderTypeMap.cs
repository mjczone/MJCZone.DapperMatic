using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DapperMatic.Providers.MySql;

public sealed class MySqlProviderTypeMap : DbProviderTypeMapBase
{
    internal static readonly Lazy<MySqlProviderTypeMap> Instance =
        new(() => new MySqlProviderTypeMap());

    private MySqlProviderTypeMap()
        : base() { }

    protected override DbProviderType ProviderType => DbProviderType.MySql;

    public override string SqTypeForStringLengthMax => "text(65535)";

    public override string SqTypeForBinaryLengthMax => "blob(65535)";

    public override string SqlTypeForJson => "text(65535)";

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override DbProviderSqlType[] ProviderSqlTypes =>
        [
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_tinyint,
                formatWithPrecision: "tinyint({0})",
                defaultPrecision: 4,
                canUseToAutoIncrement: true,
                minValue: -128,
                maxValue: 128
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_tinyint_unsigned,
                formatWithPrecision: "tinyint({0}) unsigned",
                defaultPrecision: 4,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: 255
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_smallint,
                formatWithPrecision: "smallint({0})",
                defaultPrecision: 5,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_smallint_unsigned,
                formatWithPrecision: "smallint({0}) unsigned",
                defaultPrecision: 5,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: 65535
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_mediumint,
                formatWithPrecision: "mediumint({0})",
                defaultPrecision: 7,
                canUseToAutoIncrement: true,
                minValue: -8388608,
                maxValue: 8388607
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_mediumint_unsigned,
                formatWithPrecision: "mediumint({0}) unsigned",
                defaultPrecision: 7,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: 16777215
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_integer,
                formatWithPrecision: "integer({0})",
                defaultPrecision: 11,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_integer_unsigned,
                formatWithPrecision: "integer({0}) unsigned",
                defaultPrecision: 11,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: 4294967295
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_int,
                aliasOf: "integer",
                formatWithPrecision: "int({0})",
                defaultPrecision: 11,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_int_unsigned,
                formatWithPrecision: "int({0}) unsigned",
                defaultPrecision: 11,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: 4294967295
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_bigint,
                formatWithPrecision: "bigint({0})",
                defaultPrecision: 19,
                canUseToAutoIncrement: true,
                minValue: -Math.Pow(2, 63),
                maxValue: Math.Pow(2, 63) - 1
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_bigint_unsigned,
                formatWithPrecision: "bigint({0}) unsigned",
                defaultPrecision: 19,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: Math.Pow(2, 64) - 1
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_serial,
                aliasOf: "bigint unsigned",
                canUseToAutoIncrement: true,
                autoIncrementsAutomatically: true,
                minValue: 0,
                maxValue: Math.Pow(2, 64) - 1
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                MySqlTypes.sql_bit,
                formatWithPrecision: "bit({0})",
                defaultPrecision: 1,
                minValue: 0,
                maxValue: long.MaxValue
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_decimal,
                formatWithPrecision: "decimal({0})",
                formatWithPrecisionAndScale: "decimal({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_dec,
                aliasOf: "decimal",
                formatWithPrecision: "dec({0})",
                formatWithPrecisionAndScale: "dec({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_numeric,
                formatWithPrecision: "numeric({0})",
                formatWithPrecisionAndScale: "numeric({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_fixed,
                aliasOf: "decimal",
                formatWithPrecision: "fixed({0})",
                formatWithPrecisionAndScale: "fixed({0},{1})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_float
            // formatWithPrecision: "float({0})",
            // formatWithPrecisionAndScale: "float({0},{1})",
            // defaultPrecision: 12,
            // defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_real
            // aliasOf: "double",
            // formatWithPrecisionAndScale: "real({0},{1})",
            // defaultPrecision: 12,
            // defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_double_precision
            // aliasOf: "double",
            // formatWithPrecisionAndScale: "double precision({0},{1})",
            // defaultPrecision: 12,
            // defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_double_precision_unsigned,
                aliasOf: "double unsigned"
            // formatWithPrecisionAndScale: "double precision({0},{1}) unsigned",
            // defaultPrecision: 12,
            // defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_double
            // formatWithPrecisionAndScale: "double({0},{1})",
            // defaultPrecision: 12,
            // defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                MySqlTypes.sql_double_unsigned
            // formatWithPrecisionAndScale: "double({0},{1}) unsigned",
            // defaultPrecision: 12,
            // defaultScale: 2
            ),
            new(DbProviderSqlTypeAffinity.Boolean, MySqlTypes.sql_bool, aliasOf: "tinyint(1)"),
            new(DbProviderSqlTypeAffinity.Boolean, MySqlTypes.sql_boolean, aliasOf: "tinyint(1)"),
            new(DbProviderSqlTypeAffinity.DateTime, MySqlTypes.sql_datetime),
            new(DbProviderSqlTypeAffinity.DateTime, MySqlTypes.sql_timestamp),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                MySqlTypes.sql_time,
                formatWithPrecision: "time({0})",
                defaultPrecision: 6,
                isTimeOnly: true
            ),
            new(DbProviderSqlTypeAffinity.DateTime, MySqlTypes.sql_date, isDateOnly: true),
            new(DbProviderSqlTypeAffinity.DateTime, MySqlTypes.sql_year, isYearOnly: true),
            new(
                DbProviderSqlTypeAffinity.Text,
                MySqlTypes.sql_char,
                formatWithLength: "char({0})",
                defaultLength: DefaultLength,
                isFixedLength: true
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                MySqlTypes.sql_varchar,
                formatWithLength: "varchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                MySqlTypes.sql_text,
                formatWithLength: "text({0})",
                defaultLength: 65535
            ),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_long_varchar, aliasOf: "mediumtext"),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_tinytext),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_mediumtext),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_longtext),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_enum),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_set),
            new(DbProviderSqlTypeAffinity.Text, MySqlTypes.sql_json),
            new(
                DbProviderSqlTypeAffinity.Binary,
                MySqlTypes.sql_blob,
                formatWithLength: "blob({0})",
                defaultLength: 65535
            ),
            new(DbProviderSqlTypeAffinity.Binary, MySqlTypes.sql_tinyblob),
            new(DbProviderSqlTypeAffinity.Binary, MySqlTypes.sql_mediumblob),
            new(DbProviderSqlTypeAffinity.Binary, MySqlTypes.sql_longblob),
            new(
                DbProviderSqlTypeAffinity.Binary,
                MySqlTypes.sql_varbinary,
                formatWithLength: "varbinary({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Binary,
                MySqlTypes.sql_binary,
                formatWithLength: "binary({0})",
                defaultLength: DefaultLength,
                isFixedLength: true
            ),
            new(
                DbProviderSqlTypeAffinity.Binary,
                MySqlTypes.sql_long_varbinary,
                aliasOf: "mediumblob"
            ),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_geometry),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_point),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_linestring),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_polygon),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_multipoint),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_multilinestring),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_multipolygon),
            new(DbProviderSqlTypeAffinity.Geometry, MySqlTypes.sql_geomcollection),
            new(
                DbProviderSqlTypeAffinity.Geometry,
                MySqlTypes.sql_geometrycollection,
                aliasOf: "geomcollection"
            )
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
                Type t when t == typeof(bool) => ProviderSqlTypeLookup[MySqlTypes.sql_boolean],
                Type t when t == typeof(byte)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_smallint_unsigned],
                Type t when t == typeof(ReadOnlyMemory<byte>)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_smallint],
                Type t when t == typeof(sbyte) => ProviderSqlTypeLookup[MySqlTypes.sql_smallint],
                Type t when t == typeof(short) => ProviderSqlTypeLookup[MySqlTypes.sql_smallint],
                Type t when t == typeof(ushort)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_smallint_unsigned],
                Type t when t == typeof(int) => ProviderSqlTypeLookup[MySqlTypes.sql_int],
                Type t when t == typeof(uint) => ProviderSqlTypeLookup[MySqlTypes.sql_int_unsigned],
                Type t when t == typeof(long) => ProviderSqlTypeLookup[MySqlTypes.sql_bigint],
                Type t when t == typeof(ulong)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_bigint_unsigned],
                Type t when t == typeof(float) => ProviderSqlTypeLookup[MySqlTypes.sql_float],
                Type t when t == typeof(double) => ProviderSqlTypeLookup[MySqlTypes.sql_double],
                Type t when t == typeof(decimal) => ProviderSqlTypeLookup[MySqlTypes.sql_decimal],
                Type t when t == typeof(char) => ProviderSqlTypeLookup[MySqlTypes.sql_varchar],
                Type t when t == typeof(string)
                    => descriptor.Length.GetValueOrDefault(255) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(char[])
                    => descriptor.Length.GetValueOrDefault(255) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(ReadOnlyMemory<byte>[])
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(Stream)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(TextReader)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(byte[]) => ProviderSqlTypeLookup[MySqlTypes.sql_blob],
                Type t when t == typeof(object)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(object[])
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[MySqlTypes.sql_varchar]
                        : ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t when t == typeof(Guid) => ProviderSqlTypeLookup[MySqlTypes.sql_varchar],
                Type t when t == typeof(DateTime) => ProviderSqlTypeLookup[MySqlTypes.sql_datetime],
                Type t when t == typeof(DateTimeOffset)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_timestamp],
                Type t when t == typeof(TimeSpan) => ProviderSqlTypeLookup[MySqlTypes.sql_bigint],
                Type t when t == typeof(DateOnly) => ProviderSqlTypeLookup[MySqlTypes.sql_date],
                Type t when t == typeof(TimeOnly) => ProviderSqlTypeLookup[MySqlTypes.sql_time],
                Type t when t == typeof(BitArray) || t == typeof(BitVector32)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_varbinary],
                Type t
                    when t == typeof(ImmutableDictionary<string, string>)
                        || t == typeof(Dictionary<string, string>)
                        || t == typeof(IDictionary<string, string>)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_text],
                Type t
                    when t == typeof(JsonNode)
                        || t == typeof(JsonObject)
                        || t == typeof(JsonArray)
                        || t == typeof(JsonValue)
                        || t == typeof(JsonDocument)
                        || t == typeof(JsonElement)
                    => ProviderSqlTypeLookup[MySqlTypes.sql_text],
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
                    => ProviderSqlTypeLookup[MySqlTypes.sql_text],
                _ => null
            };

        if (providerSqlType != null)
            return true;

        // handle POCO type
        if (dotnetType.IsClass || dotnetType.IsInterface)
        {
            providerSqlType = ProviderSqlTypeLookup[MySqlTypes.sql_text];
        }

        return providerSqlType != null;
    }
}
