using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DapperMatic.Providers.Sqlite;

public sealed class SqliteProviderTypeMap : DbProviderTypeMapBase
{
    internal static readonly Lazy<SqliteProviderTypeMap> Instance =
        new(() => new SqliteProviderTypeMap());

    private SqliteProviderTypeMap()
        : base() { }

    protected override DbProviderType ProviderType => DbProviderType.Sqlite;

    public override string SqTypeForStringLengthMax => "text";

    public override string SqTypeForBinaryLengthMax => "blob";

    public override string SqlTypeForJson => "text";

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override DbProviderSqlType[] ProviderSqlTypes =>
        [
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_integer,
                formatWithPrecision: "integer({0})",
                defaultPrecision: 11,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_int,
                aliasOf: "integer",
                formatWithPrecision: "int({0})",
                defaultPrecision: 11,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_tinyint,
                formatWithPrecision: "tinyint({0})",
                defaultPrecision: 4,
                canUseToAutoIncrement: true,
                minValue: -128,
                maxValue: 128
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_smallint,
                formatWithPrecision: "smallint({0})",
                defaultPrecision: 5,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_mediumint,
                formatWithPrecision: "mediumint({0})",
                defaultPrecision: 7,
                canUseToAutoIncrement: true,
                minValue: -8388608,
                maxValue: 8388607
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_bigint,
                formatWithPrecision: "bigint({0})",
                defaultPrecision: 19,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                DbProviderSqlTypeAffinity.Integer,
                SqliteTypes.sql_unsigned_big_int,
                formatWithPrecision: "unsigned big int({0})",
                defaultPrecision: 20,
                canUseToAutoIncrement: true,
                minValue: 0,
                maxValue: 18446744073709551615
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqliteTypes.sql_real,
                formatWithPrecision: "real({0})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqliteTypes.sql_double,
                formatWithPrecision: "double({0})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqliteTypes.sql_float,
                formatWithPrecision: "float({0})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqliteTypes.sql_numeric,
                formatWithPrecision: "numeric({0})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Real,
                SqliteTypes.sql_decimal,
                formatWithPrecision: "decimal({0})",
                defaultPrecision: 12,
                defaultScale: 2
            ),
            new(
                DbProviderSqlTypeAffinity.Boolean,
                SqliteTypes.sql_bool,
                formatWithPrecision: "bool({0})",
                defaultPrecision: 1
            ),
            new(
                DbProviderSqlTypeAffinity.Boolean,
                SqliteTypes.sql_boolean,
                formatWithPrecision: "boolean({0})",
                defaultPrecision: 1
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                SqliteTypes.sql_date,
                formatWithPrecision: "date({0})",
                defaultPrecision: 10,
                isDateOnly: true
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                SqliteTypes.sql_datetime,
                formatWithPrecision: "datetime({0})",
                defaultPrecision: 19
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                SqliteTypes.sql_timestamp,
                formatWithPrecision: "timestamp({0})",
                defaultPrecision: 19
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                SqliteTypes.sql_time,
                formatWithPrecision: "time({0})",
                defaultPrecision: 8,
                isTimeOnly: true
            ),
            new(
                DbProviderSqlTypeAffinity.DateTime,
                SqliteTypes.sql_year,
                formatWithPrecision: "year({0})",
                defaultPrecision: 4
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqliteTypes.sql_nvarchar,
                formatWithLength: "nvarchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqliteTypes.sql_varchar,
                formatWithLength: "varchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqliteTypes.sql_nchar,
                formatWithLength: "nchar({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqliteTypes.sql_char,
                formatWithLength: "char({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqliteTypes.sql_varying_character,
                formatWithLength: "varying character({0})",
                defaultLength: DefaultLength
            ),
            new(
                DbProviderSqlTypeAffinity.Text,
                SqliteTypes.sql_native_character,
                formatWithLength: "native character({0})",
                defaultLength: DefaultLength
            ),
            new(DbProviderSqlTypeAffinity.Text, SqliteTypes.sql_text),
            new(DbProviderSqlTypeAffinity.Text, SqliteTypes.sql_clob),
            new(DbProviderSqlTypeAffinity.Binary, SqliteTypes.sql_blob),
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
                Type t when t == typeof(bool) => ProviderSqlTypeLookup[SqliteTypes.sql_boolean],
                Type t when t == typeof(byte) => ProviderSqlTypeLookup[SqliteTypes.sql_smallint],
                Type t when t == typeof(ReadOnlyMemory<byte>)
                    => ProviderSqlTypeLookup[SqliteTypes.sql_smallint],
                Type t when t == typeof(sbyte) => ProviderSqlTypeLookup[SqliteTypes.sql_smallint],
                Type t when t == typeof(short) => ProviderSqlTypeLookup[SqliteTypes.sql_smallint],
                Type t when t == typeof(ushort) => ProviderSqlTypeLookup[SqliteTypes.sql_smallint],
                Type t when t == typeof(int) => ProviderSqlTypeLookup[SqliteTypes.sql_int],
                Type t when t == typeof(uint) => ProviderSqlTypeLookup[SqliteTypes.sql_int],
                Type t when t == typeof(long) => ProviderSqlTypeLookup[SqliteTypes.sql_bigint],
                Type t when t == typeof(ulong) => ProviderSqlTypeLookup[SqliteTypes.sql_bigint],
                Type t when t == typeof(float) => ProviderSqlTypeLookup[SqliteTypes.sql_float],
                Type t when t == typeof(double) => ProviderSqlTypeLookup[SqliteTypes.sql_double],
                Type t when t == typeof(decimal) => ProviderSqlTypeLookup[SqliteTypes.sql_decimal],
                Type t when t == typeof(char) => ProviderSqlTypeLookup[SqliteTypes.sql_varchar],
                Type t when t == typeof(string)
                    => descriptor.Length.GetValueOrDefault(255) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(char[])
                    => descriptor.Length.GetValueOrDefault(255) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(ReadOnlyMemory<byte>[])
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(Stream)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(TextReader)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(byte[]) => ProviderSqlTypeLookup[SqliteTypes.sql_blob],
                Type t when t == typeof(object)
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(object[])
                    => descriptor.Length.GetValueOrDefault(int.MaxValue) < 8000
                        ? ProviderSqlTypeLookup[SqliteTypes.sql_varchar]
                        : ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t when t == typeof(Guid) => ProviderSqlTypeLookup[SqliteTypes.sql_varchar],
                Type t when t == typeof(DateTime)
                    => ProviderSqlTypeLookup[SqliteTypes.sql_datetime],
                Type t when t == typeof(DateTimeOffset)
                    => ProviderSqlTypeLookup[SqliteTypes.sql_timestamp],
                Type t when t == typeof(TimeSpan) => ProviderSqlTypeLookup[SqliteTypes.sql_bigint],
                Type t when t == typeof(DateOnly) => ProviderSqlTypeLookup[SqliteTypes.sql_date],
                Type t when t == typeof(TimeOnly) => ProviderSqlTypeLookup[SqliteTypes.sql_time],
                Type t when t == typeof(BitArray) || t == typeof(BitVector32)
                    => ProviderSqlTypeLookup[SqliteTypes.sql_blob],
                Type t
                    when t == typeof(ImmutableDictionary<string, string>)
                        || t == typeof(Dictionary<string, string>)
                        || t == typeof(IDictionary<string, string>)
                    => ProviderSqlTypeLookup[SqliteTypes.sql_text],
                Type t
                    when t == typeof(JsonNode)
                        || t == typeof(JsonObject)
                        || t == typeof(JsonArray)
                        || t == typeof(JsonValue)
                        || t == typeof(JsonDocument)
                        || t == typeof(JsonElement)
                    => ProviderSqlTypeLookup[SqliteTypes.sql_text],
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
                    => ProviderSqlTypeLookup[SqliteTypes.sql_text],
                _ => null
            };

        if (providerSqlType != null)
            return true;

        // handle POCO type
        if (dotnetType.IsClass || dotnetType.IsInterface)
        {
            providerSqlType = ProviderSqlTypeLookup[SqliteTypes.sql_text];
        }

        return providerSqlType != null;
    }
}
