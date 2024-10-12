// Purpose: Provides a type map for MySql data types.
namespace DapperMatic.Providers.MySql;

public class MySqlProviderTypeMap : ProviderTypeMapBase<MySqlProviderTypeMap>
{
    public MySqlProviderTypeMap()
    {
        foreach (var providerDataType in GetDefaultProviderDataTypes())
        {
            RegisterProviderDataType(providerDataType);
        }
    }

    // see: https://dev.mysql.com/doc/refman/8.0/en/data-types.html
    // covers the following MySql data types:
    //
    // - INTEGER affinity types
    // - integer
    // - tinyint
    // - smallint
    // - int
    // - mediumint
    // - bigint
    // - bit
    //
    // - REAL affinity types
    // - decimal(m,d)
    // - numeric(m,d)
    // - double(m,d)
    // - double precision(m,d)
    // - float (alias for double, being deprecated)
    // - real (alias for double, being deprecated)
    //
    // - DATE/TIME affinity types
    // - datetime
    // - timestamp
    // - time
    // - date
    // - year
    //
    // - TEXT affinity types
    // - char
    // - varchar(l)
    // - text
    // - enum('value1', 'value2', ...) -> not supported yet
    // - set('value1', 'value2', ...) -> not supported yet
    //
    // - BINARY affinity types
    // - binary
    // - varbinary
    // - blob
    //
    // - GEOMETRY affinity types
    // - geometry
    // - point
    // - linestring
    // - polygon
    // - multipoint
    // - multilinestring
    // - multipolygon
    // - geometrycollection
    //
    // - OTHER affinity types
    // - json


    /// <summary>
    /// The order is important if you don't use the isRecommendedDotNetTypeMatch predicate.
    /// </summary>
    public override ProviderDataType[] GetDefaultProviderDataTypes()
    {
        Type[] allTextAffinityTypes =
        [
            .. CommonTypes,
            .. CommonDictionaryTypes,
            .. CommonEnumerableTypes,
            typeof(object),
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
        Type[] allGeometryAffinityType = [typeof(byte[]), typeof(object)];
        return
        [
            // TEXT AFFINITY TYPES
            new ProviderDataType(
                "varchar",
                typeof(string),
                allTextAffinityTypes,
                "varchar({0})",
                sqlTypeFormatWithMaxLength: "text",
                isRecommendedDotNetTypeMatch: (x) => x == typeof(string)
            ),
            new ProviderDataType("text", typeof(string), allTextAffinityTypes),
            new ProviderDataType("char", typeof(string), allTextAffinityTypes, "char({0})"),
            // OTHER AFFINITY TYPES
            // new ProviderDataType("json", typeof(string), [typeof(string)]),
            // GEOMETRY SUPPORTED YET
            new ProviderDataType("geometry", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("point", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("linestring", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("polygon", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType(
                "geometrycollection",
                typeof(object),
                [typeof(string), typeof(object)]
            ),
            new ProviderDataType(
                "geomcollection",
                typeof(object),
                [typeof(string), typeof(object)]
            ),
            new ProviderDataType("multipoint", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType(
                "multilinestring",
                typeof(object),
                [typeof(string), typeof(object)]
            ),
            new ProviderDataType("multipolygon", typeof(object), [typeof(string), typeof(object)]),
            // non-instantiable types
            // new ProviderDataType("curve", typeof(object), [typeof(string), typeof(object)]),
            // new ProviderDataType("surface", typeof(object), [typeof(string), typeof(object)]),
            // new ProviderDataType("multicurve", typeof(object), [typeof(string), typeof(object)]),
            // new ProviderDataType("multisurface", typeof(object), [typeof(string), typeof(object)]),
            // INTEGER AFFINITY TYPES
            new ProviderDataType("bit", typeof(bool), allIntegerAffinityTypes),
            new ProviderDataType(
                "integer",
                typeof(int),
                allIntegerAffinityTypes,
                isRecommendedDotNetTypeMatch: (x) => x == typeof(int)
            ),
            new ProviderDataType("int", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("tinyint", typeof(byte), allIntegerAffinityTypes),
            new ProviderDataType(
                "smallint",
                typeof(short),
                allIntegerAffinityTypes,
                isRecommendedDotNetTypeMatch: (x) => x == typeof(short)
            ),
            new ProviderDataType("mediumint", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType(
                "bigint",
                typeof(long),
                allIntegerAffinityTypes,
                isRecommendedDotNetTypeMatch: (x) => x == typeof(long)
            ),
            // REAL AFFINITY TYPES
            new ProviderDataType(
                "decimal",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "decimal({0})",
                "decimal({0},{1})",
                isRecommendedDotNetTypeMatch: (x) => x == typeof(decimal)
            ),
            new ProviderDataType(
                "numeric",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "numeric({0})",
                "numeric({0},{1})"
            ),
            new ProviderDataType(
                "double",
                typeof(double),
                allRealAffinityTypes,
                null,
                "double({0})",
                "double({0},{1})",
                isRecommendedDotNetTypeMatch: (x) => x == typeof(double)
            ),
            new ProviderDataType(
                "double precision",
                typeof(double),
                allRealAffinityTypes,
                null,
                "double precision({0})",
                "double precision({0},{1})"
            ),
            new ProviderDataType(
                "float",
                typeof(double),
                allRealAffinityTypes,
                isRecommendedDotNetTypeMatch: (x) => x == typeof(float)
            ),
            new ProviderDataType("real", typeof(float), allRealAffinityTypes),
            // DATE/TIME AFFINITY TYPES
            new ProviderDataType(
                "datetime",
                typeof(DateTime),
                allDateTimeAffinityTypes,
                isRecommendedDotNetTypeMatch: (x) =>
                    x == typeof(DateTime) || x == typeof(DateTimeOffset)
            ),
            new ProviderDataType("timestamp", typeof(DateTime), allDateTimeAffinityTypes),
            new ProviderDataType("time", typeof(TimeSpan), allDateTimeAffinityTypes),
            new ProviderDataType("date", typeof(DateTime), allDateTimeAffinityTypes),
            new ProviderDataType(
                "year",
                typeof(int),
                [typeof(int), typeof(DateTime), typeof(DateTimeOffset)]
            ),
            // BINARY AFFINITY TYPES
            new ProviderDataType("blob", typeof(byte[]), [typeof(byte[]), typeof(object)]),
            new ProviderDataType(
                "varbinary(255)",
                typeof(byte[]),
                [typeof(byte[]), typeof(object)],
                "varbinary({0})",
                defaultLength: 255
            ),
            new ProviderDataType("binary", typeof(byte[]), [typeof(byte[]), typeof(object)]),
        ];
    }
}
