// Purpose: Provides a type map for PostgreSql data types.
namespace DapperMatic.Providers.PostgreSql;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class PostgreSqlProviderTypeMap : ProviderTypeMapBase<PostgreSqlProviderTypeMap>
{
    public PostgreSqlProviderTypeMap()
    {
        foreach (var providerDataType in GetDefaultProviderDataTypes())
        {
            RegisterProviderDataType(providerDataType);
        }
    }

    // see: https://www.postgresql.org/docs/15/datatype.html
    // covers the following PostgreSql data types:
    //
    // - INTEGER affinity types
    // - bigint, int8
    // - bigserial, serial8
    // - integer, int, int4
    // - smallint, int2
    // - smallserial, serial2
    // - serial, serial4
    // - bit(n)
    // - bit varying(n), varbit(n)
    // - boolean, bool

    // - REAL affinity types
    // - double precision, float8
    // - money
    // - numeric(p,s), decimal(p,s)
    // - real, float4
    //
    // - DATE/TIME affinity types
    // - date
    // - interval
    // - time (p) without time zone, time(p)
    // - time (p) with time zone, timetz(p)
    // - timestamp (p) without time zone, timestamp(p)
    // - timestamp (p) with time zone, timestampz(p)
    //
    // - TEXT affinity types
    // - character varying(n), varchar(n)
    // - character(n), char(n)
    // - text
    // - json
    // - jsonb
    // - xml
    // - uuid
    //
    // - BINARY affinity types
    // - bytea
    //
    // - GEOMETRY affinity types
    // - box
    // - circle
    // - lseg
    // - line
    // - path
    // - point
    // - polygon
    // - geometry
    // - geography
    //
    // - OTHER affinity types
    // - cidr
    // - inet
    // - macaddr
    // - macaddr8
    // - pg_lsn
    // - pg_snapshot
    // - tsquery
    // - tsvector
    // - txid_snapshot
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
        Type[] allGeometryAffinityType = [typeof(string), typeof(object)];
        ProviderDataType[] providerDataTypes =
        [
            // TEXT AFFINITY TYPES
            new(
                "character",
                typeof(string),
                allTextAffinityTypes,
                "character({0})"
            ),
            new("char", typeof(string), allTextAffinityTypes, "char({0})"),
            new(
                "character varying",
                typeof(string),
                allTextAffinityTypes,
                "character varying({0})"
            ),
            new("varchar", typeof(string), allTextAffinityTypes, "varchar({0})"),
            new("text", typeof(string), allTextAffinityTypes),
            new("json", typeof(string), [typeof(string)]),
            new("jsonb", typeof(string), [typeof(string)]),
            new("xml", typeof(string), [typeof(string)]),
            new("uuid", typeof(Guid), [typeof(Guid), typeof(string)]),
            // OTHER AFFINITY TYPES
            new("cidr", typeof(object), allGeometryAffinityType),
            new("inet", typeof(object), allGeometryAffinityType),
            new("macaddr", typeof(object), allGeometryAffinityType),
            new("macaddr8", typeof(object), allGeometryAffinityType),
            new("pg_lsn", typeof(object), allGeometryAffinityType),
            new("pg_snapshot", typeof(object), allGeometryAffinityType),
            new("tsquery", typeof(object), allGeometryAffinityType),
            new("tsvector", typeof(object), allGeometryAffinityType),
            new("txid_snapshot", typeof(object), allGeometryAffinityType),
            // GEOMETRY SUPPORTED YET
            new("box", typeof(object), allGeometryAffinityType),
            new("circle", typeof(object), allGeometryAffinityType),
            new("lseg", typeof(object), allGeometryAffinityType),
            new("line", typeof(object), allGeometryAffinityType),
            new("path", typeof(object), allGeometryAffinityType),
            new("point", typeof(object), allGeometryAffinityType),
            new("polygon", typeof(object), allGeometryAffinityType),
            new("geometry", typeof(object), allGeometryAffinityType),
            new("geography", typeof(object), allGeometryAffinityType),
            // INTEGER AFFINITY TYPES
            new("smallint", typeof(short), allIntegerAffinityTypes),
            new("int2", typeof(short), allIntegerAffinityTypes),
            new("smallserial", typeof(short), allIntegerAffinityTypes),
            new("serial2", typeof(short), allIntegerAffinityTypes),
            new("integer", typeof(int), allIntegerAffinityTypes),
            new("int", typeof(int), allIntegerAffinityTypes),
            new("int4", typeof(int), allIntegerAffinityTypes),
            new("serial", typeof(int), allIntegerAffinityTypes),
            new("serial4", typeof(int), allIntegerAffinityTypes),
            new("bigint", typeof(long), allIntegerAffinityTypes),
            new("int8", typeof(long), allIntegerAffinityTypes),
            new("bigserial", typeof(long), allIntegerAffinityTypes),
            new("serial8", typeof(long), allIntegerAffinityTypes),
            new("bit", typeof(int), allIntegerAffinityTypes, "bit({0})"),
            new(
                "bit varying",
                typeof(int),
                allIntegerAffinityTypes,
                "bit varying({0})"
            ),
            new("varbit", typeof(int), allIntegerAffinityTypes, "varbit({0})"),
            new("boolean", typeof(bool), allIntegerAffinityTypes),
            new("bool", typeof(bool), allIntegerAffinityTypes),
            // REAL AFFINITY TYPES
            new(
                "decimal",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "decimal({0})",
                "decimal({0},{1})"
            ),
            new(
                "numeric",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "numeric({0})",
                "numeric({0},{1})"
            ),
            new("money", typeof(decimal), allRealAffinityTypes),
            new("double precision", typeof(double), allRealAffinityTypes),
            new("float8", typeof(double), allRealAffinityTypes),
            new("real", typeof(float), allRealAffinityTypes),
            new("float4", typeof(float), allRealAffinityTypes),
            // DATE/TIME AFFINITY TYPES
            new("date", typeof(DateTime), allDateTimeAffinityTypes),
            new("interval", typeof(TimeSpan), allDateTimeAffinityTypes),
            new(
                "time without time zone",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "time({0}) without time zone"
            ),
            new(
                "time",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "time({0})"
            ),
            new(
                "time with time zone",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "time({0}) with time zone"
            ),
            new(
                "timetz",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "timetz({0})"
            ),
            new(
                "timestamp without time zone",
                typeof(DateTime),
                allDateTimeAffinityTypes,
                null,
                "timestamp({0}) without time zone"
            ),
            new(
                "timestamp",
                typeof(DateTime),
                allDateTimeAffinityTypes,
                null,
                "timestamp({0})"
            ),
            new(
                "timestamp with time zone",
                typeof(DateTimeOffset),
                allDateTimeAffinityTypes,
                null,
                "timestamp({0}) with time zone"
            ),
            new(
                "timestamptz",
                typeof(DateTimeOffset),
                allDateTimeAffinityTypes,
                null,
                "timestamptz({0})"
            ),
            // BINARY AFFINITY TYPES
            new("bytea", typeof(byte[]), allBlobAffinityTypes)
        ];

        // add array versions of data types
        providerDataTypes =
        [
            .. providerDataTypes,
            .. providerDataTypes
                .Where(x =>
                    !x.SqlTypeFormat.Equals("bytea", StringComparison.OrdinalIgnoreCase)
                    && !x.SqlTypeFormat.Equals("json", StringComparison.OrdinalIgnoreCase)
                    && !x.SqlTypeFormat.Equals("jsonb", StringComparison.OrdinalIgnoreCase)
                    && !x.SqlTypeFormat.Equals("xml", StringComparison.OrdinalIgnoreCase)
                )
                .Select(x =>
                {
                    return new ProviderDataType(
                        $"{x.SqlTypeFormat}[]",
                        x.PrimaryDotnetType.MakeArrayType(),
                        x.SupportedDotnetTypes.Select(t => t.MakeArrayType())
                            .Concat(allGeometryAffinityType)
                            .Distinct()
                            .ToArray()
                    );
                })
        ];

        return providerDataTypes;
    }
}
