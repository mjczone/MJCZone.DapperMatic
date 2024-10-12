// Purpose: Provides a type map for PostgreSql data types.
namespace DapperMatic.Providers.PostgreSql;

public class PostgreSqlProviderTypeMap : ProviderTypeMapBase<PostgreSqlProviderTypeMap>
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
        ProviderDataType[] providerDataTypes =
        [
            // TEXT AFFINITY TYPES
            new ProviderDataType(
                "character",
                typeof(string),
                allTextAffinityTypes,
                "character({0})"
            ),
            new ProviderDataType("char", typeof(string), allTextAffinityTypes, "char({0})"),
            new ProviderDataType(
                "character varying",
                typeof(string),
                allTextAffinityTypes,
                "character varying({0})"
            ),
            new ProviderDataType("varchar", typeof(string), allTextAffinityTypes, "varchar({0})"),
            new ProviderDataType("text", typeof(string), allTextAffinityTypes),
            new ProviderDataType("json", typeof(string), [typeof(string)]),
            new ProviderDataType("jsonb", typeof(string), [typeof(string)]),
            new ProviderDataType("xml", typeof(string), [typeof(string)]),
            new ProviderDataType("uuid", typeof(Guid), [typeof(Guid), typeof(string)]),
            // OTHER AFFINITY TYPES
            new ProviderDataType("cidr", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("inet", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("macaddr", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("macaddr8", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("pg_lsn", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("pg_snapshot", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("tsquery", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("tsvector", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("txid_snapshot", typeof(object), [typeof(string), typeof(object)]),
            // GEOMETRY SUPPORTED YET
            new ProviderDataType("box", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("circle", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("lseg", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("line", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("path", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("point", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("polygon", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("geometry", typeof(object), [typeof(string), typeof(object)]),
            new ProviderDataType("geography", typeof(object), [typeof(string), typeof(object)]),
            // INTEGER AFFINITY TYPES
            new ProviderDataType("smallint", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("int2", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("smallserial", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("serial2", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("integer", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("int", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("int4", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("serial", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("serial4", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("bigint", typeof(long), allIntegerAffinityTypes),
            new ProviderDataType("int8", typeof(long), allIntegerAffinityTypes),
            new ProviderDataType("bigserial", typeof(long), allIntegerAffinityTypes),
            new ProviderDataType("serial8", typeof(long), allIntegerAffinityTypes),
            new ProviderDataType("bit", typeof(int), allIntegerAffinityTypes, "bit({0})"),
            new ProviderDataType(
                "bit varying",
                typeof(int),
                allIntegerAffinityTypes,
                "bit varying({0})"
            ),
            new ProviderDataType("varbit", typeof(int), allIntegerAffinityTypes, "varbit({0})"),
            new ProviderDataType("boolean", typeof(bool), allIntegerAffinityTypes),
            new ProviderDataType("bool", typeof(bool), allIntegerAffinityTypes),
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
            new ProviderDataType("money", typeof(decimal), allRealAffinityTypes, null),
            new ProviderDataType("double precision", typeof(double), allRealAffinityTypes, null),
            new ProviderDataType("float8", typeof(double), allRealAffinityTypes),
            new ProviderDataType("real", typeof(float), allRealAffinityTypes),
            new ProviderDataType("float4", typeof(float), allRealAffinityTypes),
            // DATE/TIME AFFINITY TYPES
            new ProviderDataType("date", typeof(DateTime), allDateTimeAffinityTypes),
            new ProviderDataType("interval", typeof(TimeSpan), allDateTimeAffinityTypes),
            new ProviderDataType(
                "time without time zone",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "time({0}) without time zone"
            ),
            new ProviderDataType(
                "time",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "time({0})"
            ),
            new ProviderDataType(
                "time with time zone",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "time({0}) with time zone"
            ),
            new ProviderDataType(
                "timetz",
                typeof(TimeSpan),
                allDateTimeAffinityTypes,
                null,
                "timetz({0})"
            ),
            new ProviderDataType(
                "timestamp without time zone",
                typeof(DateTime),
                allDateTimeAffinityTypes,
                null,
                "timestamp({0}) without time zone"
            ),
            new ProviderDataType(
                "timestamp",
                typeof(DateTime),
                allDateTimeAffinityTypes,
                null,
                "timestamp({0})"
            ),
            new ProviderDataType(
                "timestamp with time zone",
                typeof(DateTimeOffset),
                allDateTimeAffinityTypes,
                null,
                "timestamp({0}) with time zone"
            ),
            new ProviderDataType(
                "timestamptz",
                typeof(DateTimeOffset),
                allDateTimeAffinityTypes,
                null,
                "timestamptz({0})"
            ),
            // BINARY AFFINITY TYPES
            new ProviderDataType("bytea", typeof(byte[]), [typeof(byte[]), typeof(object)]),
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
                            .Concat([typeof(string), typeof(object)])
                            .Distinct()
                            .ToArray()
                    );
                }),
        ];

        return providerDataTypes;
    }
}
