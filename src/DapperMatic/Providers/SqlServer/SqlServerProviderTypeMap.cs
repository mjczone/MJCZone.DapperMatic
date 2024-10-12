// Purpose: Provides a type map for SqlServer data types.
namespace DapperMatic.Providers.SqlServer;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class SqlServerProviderTypeMap : ProviderTypeMapBase<SqlServerProviderTypeMap>
{
    public SqlServerProviderTypeMap()
    {
        foreach (var providerDataType in GetDefaultProviderDataTypes())
        {
            RegisterProviderDataType(providerDataType);
        }
    }

    // see: https://docs.microsoft.com/en-us/sql/t-sql/data-types/data-types-transact-sql
    // covers the following SqlServer data types:
    //
    // - INTEGER affinity types
    // - tinyint
    // - smallint
    // - int
    // - bigint
    // - bit
    //
    // - REAL affinity types
    // - decimal
    // - numeric
    // - money
    // - smallmoney
    // - float
    // - real
    //
    // - DATE/TIME affinity types
    // - date
    // - time
    // - datetime2
    // - datetimeoffset
    // - datetime
    // - smalldatetime
    //
    // - TEXT affinity types
    // - char
    // - varchar
    // - text
    // - nchar
    // - nvarchar
    // - ntext
    //
    // - BINARY affinity types
    // - binary
    // - varbinary
    // - image
    //
    // - OTHER affinity types
    // - uniqueidentifier
    // - xml
    // - timestamp
    // - hierarchyid
    // - sql_variant
    // - geometry
    // - geography
    // - cursor (n/a)
    // - table (n/a)
    // - json (n/a, currently in preview for Azure SQL Database and Azure SQL Managed Instance)
    public override ProviderDataType[] GetDefaultProviderDataTypes()
    {
        Type[] allTextAffinityTypes =
        [
            .. CommonTypes,
            .. CommonDictionaryTypes,
            .. CommonEnumerableTypes,
            typeof(object)
        ];
        Type[] allDateTimeAffinityTypes =
        [
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan)
        ];
        Type[] allIntegerAffinityTypes =
        [
            typeof(bool),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(byte)
        ];
        Type[] allRealAffinityTypes =
        [
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(int),
            typeof(long),
            typeof(short)
        ];
        Type[] allBlobAffinityTypes = [typeof(byte[]), typeof(object)];
        return
        [
            // TEXT AFFINITY TYPES
            new ProviderDataType("char", typeof(string), allTextAffinityTypes, "char({0})"),
            new ProviderDataType("varchar", typeof(string), allTextAffinityTypes, "varchar({0})"),
            new ProviderDataType("text", typeof(string), allTextAffinityTypes),
            new ProviderDataType("nchar", typeof(string), allTextAffinityTypes, "nchar({0})"),
            new ProviderDataType("nvarchar", typeof(string), allTextAffinityTypes, "nvarchar({0})"),
            new ProviderDataType("ntext", typeof(string), allTextAffinityTypes),
            // OTHER AFFINITY TYPES
            new ProviderDataType("uniqueidentifier", typeof(Guid), [typeof(Guid), typeof(string)]),
            new ProviderDataType("xml", typeof(string), [typeof(string)]),
            new ProviderDataType("timestamp", typeof(DateTime), [typeof(DateTime)]),
            new ProviderDataType("sql_variant", typeof(object), [typeof(object)]),
            // NOT SUPPORTED YET
            new ProviderDataType("hierarchyid", typeof(object), [typeof(object)]),
            new ProviderDataType("geometry", typeof(object), [typeof(object)]),
            new ProviderDataType("geography", typeof(object), [typeof(object)]),
            // new ProviderDataType("cursor", typeof(object), [typeof(object)]),
            // new ProviderDataType("table", typeof(object), [typeof(object)]),
            // new ProviderDataType("json", typeof(string), [typeof(string)]),
            // INTEGER AFFINITY TYPES
            new ProviderDataType("tinyint", typeof(byte), allIntegerAffinityTypes),
            new ProviderDataType("smallint", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("int", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("bigint", typeof(long), allIntegerAffinityTypes),
            new ProviderDataType("bit", typeof(bool), allIntegerAffinityTypes),
            // REAL AFFINITY TYPES
            new ProviderDataType(
                "decimal",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "decimal({0})",
                "decimal({0},{1})"
            ),
            new ProviderDataType(
                "numeric",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "numeric({0})",
                "numeric({0},{1})"
            ),
            new ProviderDataType("money", typeof(decimal), allRealAffinityTypes),
            new ProviderDataType("smallmoney", typeof(decimal), allRealAffinityTypes),
            new ProviderDataType("float", typeof(double), allRealAffinityTypes),
            new ProviderDataType("real", typeof(float), allRealAffinityTypes),
            // DATE/TIME AFFINITY TYPES
            new ProviderDataType("datetime", typeof(DateTime), allDateTimeAffinityTypes),
            new ProviderDataType("datetime2", typeof(DateTimeOffset), allDateTimeAffinityTypes),
            new ProviderDataType(
                "datetimeoffset",
                typeof(DateTimeOffset),
                allDateTimeAffinityTypes
            ),
            new ProviderDataType("date", typeof(DateTime), allDateTimeAffinityTypes),
            new ProviderDataType("time", typeof(DateTime), allDateTimeAffinityTypes),
            new ProviderDataType("smalldatetime", typeof(DateTime), allDateTimeAffinityTypes),
            // BINARY AFFINITY TYPES
            new ProviderDataType("varbinary", typeof(byte[]), allBlobAffinityTypes),
            new ProviderDataType("binary", typeof(byte[]), allBlobAffinityTypes),
            new ProviderDataType("image", typeof(byte[]), allBlobAffinityTypes)
        ];
    }
}
