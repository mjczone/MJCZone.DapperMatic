namespace DapperMatic.Providers.Sqlite;

public sealed class SqliteProviderTypeMap : ProviderTypeMapBase
{
    internal static readonly Lazy<SqliteProviderTypeMap> Instance =
        new(() => new SqliteProviderTypeMap());

    private SqliteProviderTypeMap() : base()
    {
    }

    protected override DbProviderType ProviderType => DbProviderType.Sqlite;

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override ProviderSqlType[] ProviderSqlTypes =>
    [
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_integer, formatWithPrecision: "integer({0})",
            defaultPrecision: 11, canUseToAutoIncrement: true, minValue: -2147483648, maxValue: 2147483647),
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_int, aliasOf: "integer", formatWithPrecision: "int({0})",
            defaultPrecision: 11, canUseToAutoIncrement: true, minValue: -2147483648, maxValue: 2147483647),
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_tinyint, formatWithPrecision: "tinyint({0})",
            defaultPrecision: 4, canUseToAutoIncrement: true, minValue: -128, maxValue: 128),
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_smallint, formatWithPrecision: "smallint({0})",
            defaultPrecision: 5, canUseToAutoIncrement: true, minValue: -32768, maxValue: 32767),
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_mediumint, formatWithPrecision: "mediumint({0})",
            defaultPrecision: 7, canUseToAutoIncrement: true, minValue: -8388608, maxValue: 8388607),
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_bigint, formatWithPrecision: "bigint({0})",
            defaultPrecision: 19, canUseToAutoIncrement: true, minValue: -9223372036854775808,
            maxValue: 9223372036854775807),
        new(ProviderSqlTypeAffinity.Integer, SqliteTypes.sql_unsigned_big_int, formatWithPrecision: "unsigned big int({0})",
            defaultPrecision: 20, canUseToAutoIncrement: true, minValue: 0, maxValue: 18446744073709551615),
        new(ProviderSqlTypeAffinity.Real, SqliteTypes.sql_real, formatWithPrecision: "real({0})",
            defaultPrecision: 12, defaultScale: 2),
        new(ProviderSqlTypeAffinity.Real, SqliteTypes.sql_double, formatWithPrecision: "double({0})",
            defaultPrecision: 12, defaultScale: 2),
        new(ProviderSqlTypeAffinity.Real, SqliteTypes.sql_float, formatWithPrecision: "float({0})",
            defaultPrecision: 12, defaultScale: 2),
        new(ProviderSqlTypeAffinity.Real, SqliteTypes.sql_numeric, formatWithPrecision: "numeric({0})",
            defaultPrecision: 12, defaultScale: 2),
        new(ProviderSqlTypeAffinity.Real, SqliteTypes.sql_decimal, formatWithPrecision: "decimal({0})",
            defaultPrecision: 12, defaultScale: 2),
        new(ProviderSqlTypeAffinity.Boolean, SqliteTypes.sql_bool, formatWithPrecision: "bool({0})",
            defaultPrecision: 1),
        new(ProviderSqlTypeAffinity.Boolean, SqliteTypes.sql_boolean, formatWithPrecision: "boolean({0})",
            defaultPrecision: 1),
        new(ProviderSqlTypeAffinity.DateTime, SqliteTypes.sql_date, formatWithPrecision: "date({0})",
            defaultPrecision: 10, isDateOnly: true),
        new(ProviderSqlTypeAffinity.DateTime, SqliteTypes.sql_datetime, formatWithPrecision: "datetime({0})",
            defaultPrecision: 19),
        new(ProviderSqlTypeAffinity.DateTime, SqliteTypes.sql_timestamp, formatWithPrecision: "timestamp({0})",
            defaultPrecision: 19),
        new(ProviderSqlTypeAffinity.DateTime, SqliteTypes.sql_time, formatWithPrecision: "time({0})",
            defaultPrecision: 8, isTimeOnly: true),
        new(ProviderSqlTypeAffinity.DateTime, SqliteTypes.sql_year, formatWithPrecision: "year({0})",
            defaultPrecision: 4),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_char, formatWithPrecision: "char({0})",
            defaultPrecision: 1),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_nchar, formatWithPrecision: "nchar({0})",
            defaultPrecision: 1),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_varchar, formatWithPrecision: "varchar({0})",
            defaultPrecision: 255),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_nvarchar, formatWithPrecision: "nvarchar({0})",
            defaultPrecision: 255),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_varying_character, formatWithPrecision: "varying character({0})",
            defaultPrecision: 255),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_native_character, formatWithPrecision: "native character({0})",
            defaultPrecision: 255),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_text, formatWithPrecision: "text({0})",
            defaultPrecision: 65535),
        new(ProviderSqlTypeAffinity.Text, SqliteTypes.sql_clob, formatWithPrecision: "clob({0})",
            defaultPrecision: 65535),
        new(ProviderSqlTypeAffinity.Binary, SqliteTypes.sql_blob, formatWithPrecision: "blob({0})",
            defaultPrecision: 65535),        
    ];
}