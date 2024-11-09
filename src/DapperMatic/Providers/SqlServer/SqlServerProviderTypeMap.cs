using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DapperMatic.Providers.SqlServer;

public sealed class SqlServerProviderTypeMap : DbProviderTypeMapBase
{
    internal static readonly Lazy<SqlServerProviderTypeMap> Instance =
        new(() => new SqlServerProviderTypeMap());

    private SqlServerProviderTypeMap()
        : base() { }

    protected override DbProviderType ProviderType => DbProviderType.SqlServer;

    public override string SqTypeForStringLengthMax => "nvarchar(max)";

    public override string SqTypeForBinaryLengthMax => "varbinary(max)";

    public override string SqlTypeForJson => "nvarchar(max)";

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override DbProviderSqlType[] ProviderSqlTypes =>
        [
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_tinyint,
                canUseToAutoIncrement: true,
                minValue: -128,
                maxValue: 128
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_smallint,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_int,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_bigint,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_decimal,
                formatWithPrecision: "decimal({0})",
                formatWithPrecisionAndScale: "decimal({0},{1})",
                defaultPrecision: 18,
                defaultScale: 0
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_numeric,
                formatWithPrecision: "numeric({0})",
                formatWithPrecisionAndScale: "numeric({0},{1})",
                defaultPrecision: 18,
                defaultScale: 0
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_float,
                formatWithPrecision: "float({0})",
                defaultPrecision: 53,
                defaultScale: 0
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_real,
                formatWithPrecision: "real({0})",
                defaultPrecision: 24,
                defaultScale: 0
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_money,
                formatWithPrecision: "money({0})",
                defaultPrecision: 19,
                defaultScale: 4
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_smallmoney,
                formatWithPrecision: "smallmoney({0})",
                defaultPrecision: 10,
                defaultScale: 4
            ),
            new(DbProviderSqlTypeAffinity.Boolean, SqlServerTypes.sql_bit),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_date, isDateOnly: true),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_datetime),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_smalldatetime),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_datetime2),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_datetimeoffset),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_time, isTimeOnly: true),
            new(DbProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_timestamp),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_nvarchar,
                formatWithLength: "nvarchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_varchar,
                formatWithLength: "varchar({0})",
                defaultLength: DefaultLength
            ),
            new(DbProviderSqlTypeAffinity.Text, SqlServerTypes.sql_ntext),
            new(DbProviderSqlTypeAffinity.Text, SqlServerTypes.sql_text),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_nchar,
                formatWithLength: "nchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_char,
                formatWithLength: "char({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_uniqueidentifier,
                isGuidOnly: true
            ),
            new(
                DbProviderSqlTypeAffinity.Binary,
                SqlServerTypes.sql_varbinary,
                formatWithLength: "varbinary({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Binary,
                SqlServerTypes.sql_binary,
                formatWithLength: "binary({0})",
                defaultLength: DefaultLength
            ),
            new(DbProviderSqlTypeAffinity.Binary, SqlServerTypes.sql_image),
            new(DbProviderSqlTypeAffinity.Geometry, SqlServerTypes.sql_geometry),
            new(DbProviderSqlTypeAffinity.Geometry, SqlServerTypes.sql_geography),
            new(DbProviderSqlTypeAffinity.Geometry, SqlServerTypes.sql_hierarchyid),
            new(DbProviderSqlTypeAffinity.Other, SqlServerTypes.sql_variant),
            new(DbProviderSqlTypeAffinity.Other, SqlServerTypes.sql_xml),
            new(DbProviderSqlTypeAffinity.Other, SqlServerTypes.sql_cursor),
            new(DbProviderSqlTypeAffinity.Other, SqlServerTypes.sql_table),
            new(DbProviderSqlTypeAffinity.Other, SqlServerTypes.sql_json)
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
                Type t when t == typeof(bool) => ProviderSqlTypeLookup[SqlServerTypes.sql_bit],
                Type t when t == typeof(byte) => ProviderSqlTypeLookup[SqlServerTypes.sql_smallint],
                Type t when t == typeof(ReadOnlyMemory<byte>)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_smallint],
                Type t when t == typeof(sbyte)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_smallint],
                Type t when t == typeof(short)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_smallint],
                Type t when t == typeof(ushort)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_smallint],
                Type t when t == typeof(int) => ProviderSqlTypeLookup[SqlServerTypes.sql_int],
                Type t when t == typeof(uint) => ProviderSqlTypeLookup[SqlServerTypes.sql_int],
                Type t when t == typeof(long) => ProviderSqlTypeLookup[SqlServerTypes.sql_bigint],
                Type t when t == typeof(ulong) => ProviderSqlTypeLookup[SqlServerTypes.sql_bigint],
                Type t when t == typeof(float) => ProviderSqlTypeLookup[SqlServerTypes.sql_float],
                Type t when t == typeof(double) => ProviderSqlTypeLookup[SqlServerTypes.sql_float],
                Type t when t == typeof(decimal)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_decimal],
                Type t when t == typeof(char)
                    => descriptor.Unicode.GetValueOrDefault(true)
                        ? ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar]
                        : ProviderSqlTypeLookup[SqlServerTypes.sql_varchar],
                Type t when t == typeof(string)
                    => descriptor.Unicode.GetValueOrDefault(true)
                        ? ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar]
                        : ProviderSqlTypeLookup[SqlServerTypes.sql_varchar],
                Type t when t == typeof(char[])
                    => descriptor.Unicode.GetValueOrDefault(true)
                        ? ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar]
                        : ProviderSqlTypeLookup[SqlServerTypes.sql_varchar],
                Type t when t == typeof(ReadOnlyMemory<byte>[])
                    => descriptor.Unicode.GetValueOrDefault(true)
                        ? ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar]
                        : ProviderSqlTypeLookup[SqlServerTypes.sql_varchar],
                Type t when t == typeof(Stream)
                    => descriptor.Unicode.GetValueOrDefault(true)
                        ? ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar]
                        : ProviderSqlTypeLookup[SqlServerTypes.sql_varchar],
                Type t when t == typeof(TextReader)
                    => descriptor.Unicode.GetValueOrDefault(true)
                        ? ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar]
                        : ProviderSqlTypeLookup[SqlServerTypes.sql_varchar],
                Type t when t == typeof(byte[])
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_varbinary],
                Type t when t == typeof(object)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar],
                Type t when t == typeof(object[])
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar],
                Type t when t == typeof(Guid)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_uniqueidentifier],
                Type t when t == typeof(DateTime)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_datetime],
                Type t when t == typeof(DateTimeOffset)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_datetimeoffset],
                Type t when t == typeof(TimeSpan)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_bigint],
                Type t when t == typeof(DateOnly) => ProviderSqlTypeLookup[SqlServerTypes.sql_date],
                Type t when t == typeof(TimeOnly) => ProviderSqlTypeLookup[SqlServerTypes.sql_time],
                Type t when t == typeof(BitArray) || t == typeof(BitVector32)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_varbinary],
                Type t
                    when t == typeof(ImmutableDictionary<string, string>)
                        || t == typeof(Dictionary<string, string>)
                        || t == typeof(IDictionary<string, string>)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar],
                Type t
                    when t == typeof(JsonNode)
                        || t == typeof(JsonObject)
                        || t == typeof(JsonArray)
                        || t == typeof(JsonValue)
                        || t == typeof(JsonDocument)
                        || t == typeof(JsonElement)
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar],
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
                    => ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar],
                _ => null
            };

        if (providerSqlType != null)
            return true;

        // handle POCO type
        if (dotnetType.IsClass || dotnetType.IsInterface)
        {
            providerSqlType = ProviderSqlTypeLookup[SqlServerTypes.sql_nvarchar];
        }

        return providerSqlType != null;
    }
}
