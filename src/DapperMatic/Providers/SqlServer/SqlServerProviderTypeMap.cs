namespace DapperMatic.Providers.SqlServer;

public sealed class SqlServerProviderTypeMap : ProviderTypeMapBase
{
    internal static readonly Lazy<SqlServerProviderTypeMap> Instance =
        new(() => new SqlServerProviderTypeMap());

    private SqlServerProviderTypeMap()
        : base() { }

    protected override DbProviderType ProviderType => DbProviderType.Sqlite;

    /// <summary>
    /// IMPORTANT!! The order within an affinity group matters, as the first possible match will be used as the recommended sql type for a dotnet type
    /// </summary>
    protected override ProviderSqlType[] ProviderSqlTypes =>
        [
            new(
                ProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_tinyint,
                canUseToAutoIncrement: true,
                minValue: -128,
                maxValue: 128
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_smallint,
                canUseToAutoIncrement: true,
                minValue: -32768,
                maxValue: 32767
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_int,
                canUseToAutoIncrement: true,
                minValue: -2147483648,
                maxValue: 2147483647
            ),
            new(
                ProviderSqlTypeAffinity.Integer,
                SqlServerTypes.sql_bigint,
                canUseToAutoIncrement: true,
                minValue: -9223372036854775808,
                maxValue: 9223372036854775807
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_decimal,
                formatWithPrecision: "decimal({0})",
                formatWithPrecisionAndScale: "decimal({0},{1})",
                defaultPrecision: 18,
                defaultScale: 0
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_numeric,
                formatWithPrecision: "numeric({0})",
                formatWithPrecisionAndScale: "numeric({0},{1})",
                defaultPrecision: 18,
                defaultScale: 0
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_float,
                formatWithPrecision: "float({0})",
                defaultPrecision: 53,
                defaultScale: 0
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_real,
                formatWithPrecision: "real({0})",
                defaultPrecision: 24,
                defaultScale: 0
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_money,
                formatWithPrecision: "money({0})",
                defaultPrecision: 19,
                defaultScale: 4
            ),
            new(
                ProviderSqlTypeAffinity.Real,
                SqlServerTypes.sql_smallmoney,
                formatWithPrecision: "smallmoney({0})",
                defaultPrecision: 10,
                defaultScale: 4
            ),
            new(ProviderSqlTypeAffinity.Boolean, SqlServerTypes.sql_bit),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_date, isDateOnly: true),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_datetime),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_smalldatetime),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_datetime2),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_datetimeoffset),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_time, isTimeOnly: true),
            new(ProviderSqlTypeAffinity.DateTime, SqlServerTypes.sql_timestamp),
            new(
                ProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_char,
                formatWithLength: "char({0})",
                defaultLength: 255
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_varchar,
                formatWithLength: "varchar({0})",
                defaultLength: 255
            ),
            new(ProviderSqlTypeAffinity.Text, SqlServerTypes.sql_text),
            new(
                ProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_nchar,
                formatWithLength: "nchar({0})",
                defaultLength: 255
            ),
            new(
                ProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_nvarchar,
                formatWithLength: "nvarchar({0})",
                defaultLength: 255
            ),
            new(ProviderSqlTypeAffinity.Text, SqlServerTypes.sql_ntext),
            new(
                ProviderSqlTypeAffinity.Text,
                SqlServerTypes.sql_uniqueidentifier,
                isGuidOnly: true
            ),
            new(
                ProviderSqlTypeAffinity.Binary,
                SqlServerTypes.sql_binary,
                formatWithLength: "binary({0})",
                defaultLength: 1024
            ),
            new(
                ProviderSqlTypeAffinity.Binary,
                SqlServerTypes.sql_varbinary,
                formatWithLength: "varbinary({0})",
                defaultLength: 1024
            ),
            new(ProviderSqlTypeAffinity.Binary, SqlServerTypes.sql_image),
            new(ProviderSqlTypeAffinity.Geometry, SqlServerTypes.sql_geometry),
            new(ProviderSqlTypeAffinity.Geometry, SqlServerTypes.sql_geography),
            new(ProviderSqlTypeAffinity.Geometry, SqlServerTypes.sql_hierarchyid),
            new(ProviderSqlTypeAffinity.Other, SqlServerTypes.sql_variant),
            new(ProviderSqlTypeAffinity.Other, SqlServerTypes.sql_xml),
            new(ProviderSqlTypeAffinity.Other, SqlServerTypes.sql_cursor),
            new(ProviderSqlTypeAffinity.Other, SqlServerTypes.sql_table),
            new(ProviderSqlTypeAffinity.Other, SqlServerTypes.sql_json)
        ];
}
