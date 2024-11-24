using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace DapperMatic.Providers.PostgreSql;

// https://www.npgsql.org/doc/types/basic.html#read-mappings
// https://www.npgsql.org/doc/types/basic.html#write-mappings
public sealed class PostgreSqlProviderTypeMap : DbProviderTypeMapBase<PostgreSqlProviderTypeMap>
{
    protected override void RegisterDotnetTypeToSqlTypeConverters()
    {
        var booleanConverter = GetBooleanToSqlTypeConverter();
        var numericConverter = GetNumbericToSqlTypeConverter();
        var guidConverter = GetGuidToSqlTypeConverter();
        var textConverter = GetTextToSqlTypeConverter();
        var xmlConverter = GetXmlToSqlTypeConverter();
        var jsonConverter = GetJsonToSqlTypeConverter();
        var dateTimeConverter = GetDateTimeToSqlTypeConverter();
        var byteArrayConverter = GetByteArrayToSqlTypeConverter();
        var objectConverter = GetObjectToSqlTypeConverter();
        var enumerableConverter = GetEnumerableToSqlTypeConverter();
        var enumConverter = GetEnumToSqlTypeConverter();
        var arrayConverter = GetArrayToSqlTypeConverter();
        var pocoConverter = GetPocoToSqlTypeConverter();
        var geometricConverter = GetGeometricToSqlTypeConverter();
        var rangeConverter = GetRangeToSqlTypeConverter();

        // Boolean affinity
        RegisterConverter<bool>(booleanConverter);

        // Numeric affinity
        RegisterConverterForTypes(
            numericConverter,
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(BigInteger),
            typeof(long),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(decimal),
            typeof(float),
            typeof(double)
        );

        // Guid affinity
        RegisterConverter<Guid>(guidConverter);

        // Text affinity
        RegisterConverterForTypes(
            textConverter,
            typeof(string),
            typeof(char),
            typeof(char[]),
            typeof(MemoryStream),
            typeof(ReadOnlyMemory<byte>[]),
            typeof(Stream),
            typeof(TextReader)
        );

        // Xml affinity
        RegisterConverterForTypes(xmlConverter, typeof(XDocument), typeof(XElement));

        // Json affinity
        RegisterConverterForTypes(
            jsonConverter,
            typeof(JsonDocument),
            typeof(JsonElement),
            typeof(JsonArray),
            typeof(JsonNode),
            typeof(JsonObject),
            typeof(JsonValue)
        );

        // DateTime affinity
        RegisterConverterForTypes(
            dateTimeConverter,
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateOnly),
            typeof(TimeOnly)
        );

        // Binary affinity
        RegisterConverterForTypes(
            byteArrayConverter,
            typeof(byte[]),
            typeof(ReadOnlyMemory<byte>),
            typeof(Memory<byte>),
            typeof(Stream),
            typeof(BinaryReader),
            typeof(BitArray),
            typeof(BitVector32)
        );

        // Object affinity
        RegisterConverter<object>(objectConverter);

        // Enumerable affinity
        RegisterConverterForTypes(
            enumerableConverter,
            typeof(ImmutableDictionary<string, string>),
            typeof(Dictionary<string, string>),
            typeof(IDictionary<string, string>),
            typeof(Dictionary<string, object>),
            typeof(IDictionary<string, object>),
            typeof(HashSet<string>),
            typeof(List<string>),
            typeof(IList<string>),
            typeof(HashSet<>),
            typeof(ISet<>),
            typeof(Dictionary<,>),
            typeof(IDictionary<,>),
            typeof(List<>),
            typeof(IList<>),
            typeof(Collection<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlySet<>),
            typeof(ICollection<>),
            typeof(IEnumerable<>)
        );

        // Enums (uses a placeholder to easily locate it)
        RegisterConverter<InternalEnumTypePlaceholder>(enumConverter);

        // Arrays (uses a placeholder to easily locate it)
        RegisterConverter<InternalArrayTypePlaceholder>(arrayConverter);

        // Poco (uses a placeholder to easily locate it)
        RegisterConverter<InternalPocoTypePlaceholder>(pocoConverter);

        // Geometry types (support the NetTopologySuite types)
        RegisterConverterForTypes(
            geometricConverter,
            // always register the NetTopologySuite types, as they provide
            // a good way to handle geometry types across database providers
            Type.GetType("NetTopologySuite.Geometries.Geometry, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.Point, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.LineString, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.Polygon, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.MultiPoint, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.MultiLineString, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite"),
            // also register the PostgreSQL compatible types
            Type.GetType(
                "System.Net.NetworkInformation.PhysicalAddress, System.Net.NetworkInformation"
            ),
            Type.GetType("System.Net.IPAddress, System.Net.Primitives"),
            Type.GetType("NpgsqlTypes.NpgsqlInet, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlCidr, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlPoint, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlLSeg, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlPath, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlPolygon, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlLine, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlCircle, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlBox, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlInterval, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlTid, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlTsQuery, Npgsql"),
            Type.GetType("NpgsqlTypes.NpgsqlTsVector, Npgsql")
        );

        // Range types (PostgreSQL is jacked up with range types)
        var rangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1, Npgsql");
        if (rangeType != null)
        {
            RegisterConverterForTypes(
                rangeConverter,
                new[]
                {
                    typeof(DateOnly),
                    typeof(int),
                    typeof(long),
                    typeof(double),
                    typeof(float),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset)
                }
                    .SelectMany(t =>
                    {
                        var rangeType = t.MakeGenericType(t);
                        return new[] { rangeType, rangeType.MakeArrayType() };
                    })
                    .ToArray()
            );
        }
    }

    protected override void RegisterSqlTypeToDotnetTypeConverters()
    {
        var booleanConverter = GetBooleanToDotnetTypeConverter();
        var numericConverter = GetNumbericToDotnetTypeConverter();
        var guidConverter = GetGuidToDotnetTypeConverter();
        var textConverter = GetTextToDotnetTypeConverter();
        var xmlConverter = GetXmlToDotnetTypeConverter();
        var jsonConverter = GetJsonToDotnetTypeConverter();
        var dateTimeConverter = GetDateTimeToDotnetTypeConverter();
        var byteArrayConverter = GetByteArrayToDotnetTypeConverter();
        var geometricConverter = GetGeometricToDotnetTypeConverter();
        var rangeConverter = GetRangeToDotnetTypeConverter();
        var miscConverter = GetMiscellaneousToDotnetTypeConverter();

        // Boolean affinity
        RegisterConverterForTypes(
            booleanConverter,
            PostgreSqlTypes.sql_bool,
            PostgreSqlTypes.sql_boolean
        );

        // Numeric affinity
        RegisterConverterForTypes(
            numericConverter,
            PostgreSqlTypes.sql_smallint,
            PostgreSqlTypes.sql_int2,
            PostgreSqlTypes.sql_smallserial,
            PostgreSqlTypes.sql_serial2,
            PostgreSqlTypes.sql_integer,
            PostgreSqlTypes.sql_int,
            PostgreSqlTypes.sql_int4,
            PostgreSqlTypes.sql_serial,
            PostgreSqlTypes.sql_serial4,
            PostgreSqlTypes.sql_bigint,
            PostgreSqlTypes.sql_int8,
            PostgreSqlTypes.sql_bigserial,
            PostgreSqlTypes.sql_serial8,
            PostgreSqlTypes.sql_float4,
            PostgreSqlTypes.sql_real,
            PostgreSqlTypes.sql_double_precision,
            PostgreSqlTypes.sql_float8,
            PostgreSqlTypes.sql_money,
            PostgreSqlTypes.sql_numeric,
            PostgreSqlTypes.sql_decimal
        );

        // DateTime affinity
        RegisterConverterForTypes(
            dateTimeConverter,
            PostgreSqlTypes.sql_date,
            PostgreSqlTypes.sql_interval,
            PostgreSqlTypes.sql_time_without_timezone,
            PostgreSqlTypes.sql_time,
            PostgreSqlTypes.sql_time_with_time_zone,
            PostgreSqlTypes.sql_timetz,
            PostgreSqlTypes.sql_timestamp_without_time_zone,
            PostgreSqlTypes.sql_timestamp,
            PostgreSqlTypes.sql_timestamp_with_time_zone,
            PostgreSqlTypes.sql_timestamptz
        );

        // Guid affinity
        RegisterConverter(PostgreSqlTypes.sql_uuid, guidConverter);

        // Text affinity
        RegisterConverterForTypes(
            textConverter,
            PostgreSqlTypes.sql_bit,
            PostgreSqlTypes.sql_bit_varying,
            PostgreSqlTypes.sql_varbit,
            PostgreSqlTypes.sql_character_varying,
            PostgreSqlTypes.sql_varchar,
            PostgreSqlTypes.sql_character,
            PostgreSqlTypes.sql_char,
            PostgreSqlTypes.sql_bpchar,
            PostgreSqlTypes.sql_text,
            PostgreSqlTypes.sql_name
        );

        // Xml affinity
        RegisterConverter(PostgreSqlTypes.sql_xml, xmlConverter);

        // Json affinity (only for very latest versions of SQL Server)
        RegisterConverterForTypes(
            jsonConverter,
            PostgreSqlTypes.sql_json,
            PostgreSqlTypes.sql_jsonb,
            PostgreSqlTypes.sql_jsonpath
        );

        // Binary affinity
        RegisterConverter(PostgreSqlTypes.sql_bytea, byteArrayConverter);

        // Geometry affinity
        RegisterConverterForTypes(
            geometricConverter,
            // the NetopologySuite types are often mapped to the geometry type
            PostgreSqlTypes.sql_box,
            PostgreSqlTypes.sql_circle,
            PostgreSqlTypes.sql_geography,
            PostgreSqlTypes.sql_geometry,
            PostgreSqlTypes.sql_line,
            PostgreSqlTypes.sql_lseg,
            PostgreSqlTypes.sql_path,
            PostgreSqlTypes.sql_point,
            PostgreSqlTypes.sql_polygon
        );

        // Miscellaneous affinity
        RegisterConverterForTypes(
            miscConverter,
            PostgreSqlTypes.sql_cidr,
            PostgreSqlTypes.sql_citext,
            PostgreSqlTypes.sql_hstore,
            PostgreSqlTypes.sql_inet,
            PostgreSqlTypes.sql_int2vector,
            PostgreSqlTypes.sql_lquery,
            PostgreSqlTypes.sql_ltree,
            PostgreSqlTypes.sql_ltxtquery,
            PostgreSqlTypes.sql_macaddr,
            PostgreSqlTypes.sql_macaddr8,
            PostgreSqlTypes.sql_oid,
            PostgreSqlTypes.sql_oidvector,
            PostgreSqlTypes.sql_pg_lsn,
            PostgreSqlTypes.sql_pg_snapshot,
            PostgreSqlTypes.sql_refcursor,
            PostgreSqlTypes.sql_regclass,
            PostgreSqlTypes.sql_regcollation,
            PostgreSqlTypes.sql_regconfig,
            PostgreSqlTypes.sql_regdictionary,
            PostgreSqlTypes.sql_regnamespace,
            PostgreSqlTypes.sql_regrole,
            PostgreSqlTypes.sql_regtype,
            PostgreSqlTypes.sql_tid,
            PostgreSqlTypes.sql_tsquery,
            PostgreSqlTypes.sql_tsvector,
            PostgreSqlTypes.sql_txid_snapshot,
            PostgreSqlTypes.sql_xid,
            PostgreSqlTypes.sql_xid8
        );

        // Range types (PostgreSQL is jacked up with range types)
        RegisterConverterForTypes(
            rangeConverter,
            PostgreSqlTypes.sql_datemultirange,
            PostgreSqlTypes.sql_daterange,
            PostgreSqlTypes.sql_int4multirange,
            PostgreSqlTypes.sql_int4range,
            PostgreSqlTypes.sql_int8multirange,
            PostgreSqlTypes.sql_int8range,
            PostgreSqlTypes.sql_nummultirange,
            PostgreSqlTypes.sql_numrange,
            PostgreSqlTypes.sql_tsrange,
            PostgreSqlTypes.sql_tsmultirange,
            PostgreSqlTypes.sql_tstzrange,
            PostgreSqlTypes.sql_tstzmultirange
        );
    }

    #region DotnetTypeToSqlTypeConverters

    private static DotnetTypeToSqlTypeConverter GetBooleanToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(PostgreSqlTypes.sql_boolean) { Length = 1 };
        });
    }

    private static DotnetTypeToSqlTypeConverter GetNumbericToSqlTypeConverter()
    {
        return new(d =>
        {
            switch (d.DotnetType)
            {
                case Type t when t == typeof(byte):
                    return new(PostgreSqlTypes.sql_smallint);
                case Type t when t == typeof(short):
                    return new(PostgreSqlTypes.sql_smallint);
                case Type t when t == typeof(int):
                    return new(PostgreSqlTypes.sql_int);
                case Type t when t == typeof(BigInteger) || t == typeof(long):
                    return new(PostgreSqlTypes.sql_bigint);
                case Type t when t == typeof(sbyte):
                    return new(PostgreSqlTypes.sql_smallint);
                case Type t when t == typeof(ushort):
                    return new(PostgreSqlTypes.sql_int);
                case Type t when t == typeof(uint):
                    return new(PostgreSqlTypes.sql_bigint);
                case Type t when t == typeof(ulong):
                    return new(PostgreSqlTypes.sql_bigint);
                case Type t when t == typeof(float):
                    return new(PostgreSqlTypes.sql_real);
                case Type t when t == typeof(double):
                    return new(PostgreSqlTypes.sql_double_precision);
                case Type t when t == typeof(decimal):
                    var precision = d.Precision ?? 16;
                    var scale = d.Scale ?? 4;
                    return new(PostgreSqlTypes.sql_decimal)
                    {
                        SqlTypeName = $"decimal({precision},{scale})",
                        Precision = precision,
                        Scale = scale
                    };
                default:
                    return new(PostgreSqlTypes.sql_int);
            }
        });
    }

    private static DotnetTypeToSqlTypeConverter GetGuidToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(PostgreSqlTypes.sql_uuid);
        });
    }

    private static DotnetTypeToSqlTypeConverter GetTextToSqlTypeConverter()
    {
        return new(d =>
        {
            if (d.IsFixedLength == true && d.Length.HasValue)
                return new(PostgreSqlTypes.sql_char)
                {
                    SqlTypeName = $"char({d.Length})",
                    Length = d.Length
                };
            if (d.Length.HasValue)
                return new(PostgreSqlTypes.sql_varchar)
                {
                    SqlTypeName = $"varchar({d.Length})",
                    Length = d.Length
                };
            return new(PostgreSqlTypes.sql_text);
        });
    }

    private static DotnetTypeToSqlTypeConverter GetXmlToSqlTypeConverter()
    {
        return new(d => new(PostgreSqlTypes.sql_xml));
    }

    private static DotnetTypeToSqlTypeConverter GetJsonToSqlTypeConverter()
    {
        return new(d => new(PostgreSqlTypes.sql_jsonb));
    }

    private DotnetTypeToSqlTypeConverter GetDateTimeToSqlTypeConverter()
    {
        return new(d =>
        {
            switch (d.DotnetType)
            {
                case Type t when t == typeof(DateTime):
                    return new(PostgreSqlTypes.sql_timestamp);
                case Type t when t == typeof(DateTimeOffset):
                    return new(PostgreSqlTypes.sql_timestamptz);
                case Type t when t == typeof(TimeSpan):
                    return new(PostgreSqlTypes.sql_time);
                case Type t when t == typeof(DateOnly):
                    return new(PostgreSqlTypes.sql_date);
                case Type t when t == typeof(TimeOnly):
                    return new(PostgreSqlTypes.sql_timetz);
                default:
                    return new(PostgreSqlTypes.sql_timestamp);
            }
        });
    }

    private DotnetTypeToSqlTypeConverter GetByteArrayToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(PostgreSqlTypes.sql_bytea) { SqlTypeName = "bytea", Length = int.MaxValue };
        });
    }

    private DotnetTypeToSqlTypeConverter GetObjectToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(PostgreSqlTypes.sql_jsonb);
        });
    }

    private DotnetTypeToSqlTypeConverter GetEnumerableToSqlTypeConverter()
    {
        return new(d =>
        {
            if (
                d.DotnetType == typeof(Dictionary<string, string>)
                || d.DotnetType == typeof(IDictionary<string, string>)
                || d.DotnetType == typeof(ImmutableDictionary<string, string>)
            )
                return new(PostgreSqlTypes.sql_hstore);

            return new(PostgreSqlTypes.sql_jsonb);
        });
    }

    private DotnetTypeToSqlTypeConverter GetEnumToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(PostgreSqlTypes.sql_varchar) { SqlTypeName = "varchar(128)", Length = 128 };
        });
    }

    // TODO: use native array types
    private DotnetTypeToSqlTypeConverter GetArrayToSqlTypeConverter() =>
        GetJsonToSqlTypeConverter();

    private DotnetTypeToSqlTypeConverter GetPocoToSqlTypeConverter() => GetJsonToSqlTypeConverter();

    private DotnetTypeToSqlTypeConverter GetGeometricToSqlTypeConverter()
    {
        return new(d =>
        {
            var assemblyQualifiedName = d.DotnetType?.AssemblyQualifiedName;
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
            {
                return null;
            }

            var assemblyQualifiedNameParts = assemblyQualifiedName.Split(',');
            var fullNameWithAssemblyName =
                assemblyQualifiedNameParts[0] + ", " + assemblyQualifiedNameParts[1];

            switch (fullNameWithAssemblyName)
            {
                // NetTopologySuite types
                case "NetTopologySuite.Geometries.Geometry, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_geometry);
                case "NetTopologySuite.Geometries.Point, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_point);
                case "NetTopologySuite.Geometries.LineString, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_geometry);
                case "NetTopologySuite.Geometries.Polygon, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_polygon);
                case "NetTopologySuite.Geometries.MultiPoint, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_geometry);
                case "NetTopologySuite.Geometries.MultiLineString, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_geometry);
                case "NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_geometry);
                case "NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite":
                    return new(PostgreSqlTypes.sql_geometry);
                // PostgreSQL types
                case "System.Net.NetworkInformation.PhysicalAddress, System.Net.NetworkInformation":
                    return new(PostgreSqlTypes.sql_macaddr8);
                case "System.Net.IPAddress, System.Net.Primitives":
                    return new(PostgreSqlTypes.sql_inet);
                case "NpgsqlTypes.NpgsqlInet, Npgsql":
                    return new(PostgreSqlTypes.sql_inet);
                case "NpgsqlTypes.NpgsqlCidr, Npgsql":
                    return new(PostgreSqlTypes.sql_cidr);
                case "NpgsqlTypes.NpgsqlPoint, Npgsql":
                    return new(PostgreSqlTypes.sql_point);
                case "NpgsqlTypes.NpgsqlLSeg, Npgsql":
                    return new(PostgreSqlTypes.sql_lseg);
                case "NpgsqlTypes.NpgsqlPath, Npgsql":
                    return new(PostgreSqlTypes.sql_path);
                case "NpgsqlTypes.NpgsqlPolygon, Npgsql":
                    return new(PostgreSqlTypes.sql_polygon);
                case "NpgsqlTypes.NpgsqlLine, Npgsql":
                    return new(PostgreSqlTypes.sql_line);
                case "NpgsqlTypes.NpgsqlCircle, Npgsql":
                    return new(PostgreSqlTypes.sql_circle);
                case "NpgsqlTypes.NpgsqlBox, Npgsql":
                    return new(PostgreSqlTypes.sql_box);
                case "NpgsqlTypes.NpgsqlInterval, Npgsql":
                    return new(PostgreSqlTypes.sql_interval);
                case "NpgsqlTypes.NpgsqlTid, Npgsql":
                    return new(PostgreSqlTypes.sql_tid);
                case "NpgsqlTypes.NpgsqlTsQuery, Npgsql":
                    return new(PostgreSqlTypes.sql_tsquery);
                case "NpgsqlTypes.NpgsqlTsVector, Npgsql":
                    return new(PostgreSqlTypes.sql_tsvector);
            }

            return null;
        });
    }

    private DotnetTypeToSqlTypeConverter GetRangeToSqlTypeConverter()
    {
        return new(d =>
        {
            var isArray = d.DotnetType != null && d.DotnetType.IsArray;

            var dotnetType = isArray ? d.DotnetType!.GetElementType() : d.DotnetType;

            if (dotnetType == null)
                return null;

            var assemblyQualifiedName = dotnetType.AssemblyQualifiedName!;

            var assemblyQualifiedNameParts = assemblyQualifiedName.Split(',');
            var fullNameWithAssemblyName =
                assemblyQualifiedNameParts[0] + ", " + assemblyQualifiedNameParts[1];

            switch (fullNameWithAssemblyName)
            {
                case "NpgsqlTypes.NpgsqlRange`1, Npgsql":
                    var genericType = dotnetType.GetGenericArguments()[0];
                    if (genericType == typeof(DateOnly))
                        return isArray
                            ? new(PostgreSqlTypes.sql_datemultirange)
                            : new(PostgreSqlTypes.sql_daterange);
                    if (genericType == typeof(int))
                        return isArray
                            ? new(PostgreSqlTypes.sql_int4multirange)
                            : new(PostgreSqlTypes.sql_int4range);
                    if (genericType == typeof(long))
                        return isArray
                            ? new(PostgreSqlTypes.sql_int8multirange)
                            : new(PostgreSqlTypes.sql_int8range);
                    if (genericType == typeof(double))
                        return isArray
                            ? new(PostgreSqlTypes.sql_nummultirange)
                            : new(PostgreSqlTypes.sql_numrange);
                    if (genericType == typeof(float))
                        return isArray
                            ? new(PostgreSqlTypes.sql_nummultirange)
                            : new(PostgreSqlTypes.sql_numrange);
                    if (genericType == typeof(decimal))
                        return isArray
                            ? new(PostgreSqlTypes.sql_nummultirange)
                            : new(PostgreSqlTypes.sql_numrange);
                    if (genericType == typeof(DateTime))
                        return isArray
                            ? new(PostgreSqlTypes.sql_tsmultirange)
                            : new(PostgreSqlTypes.sql_tsrange);
                    if (genericType == typeof(DateTimeOffset))
                        return isArray
                            ? new(PostgreSqlTypes.sql_tstzmultirange)
                            : new(PostgreSqlTypes.sql_tstzrange);
                    break;
            }

            return null;
        });
    }

    #endregion // DotnetTypeToSqlTypeConverters

    #region SqlTypeToDotnetTypeConverters

    private static SqlTypeToDotnetTypeConverter GetBooleanToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(typeof(bool));
        });
    }

    private static SqlTypeToDotnetTypeConverter GetNumbericToDotnetTypeConverter()
    {
        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case PostgreSqlTypes.sql_smallint:
                case PostgreSqlTypes.sql_int2:
                    return new DotnetTypeDescriptor(typeof(short));
                case PostgreSqlTypes.sql_smallserial:
                case PostgreSqlTypes.sql_serial2:
                    return new DotnetTypeDescriptor(typeof(short), isAutoIncrementing: true);
                case PostgreSqlTypes.sql_int:
                case PostgreSqlTypes.sql_integer:
                case PostgreSqlTypes.sql_int4:
                    return new DotnetTypeDescriptor(typeof(int));
                case PostgreSqlTypes.sql_serial:
                case PostgreSqlTypes.sql_serial4:
                    return new DotnetTypeDescriptor(typeof(int), isAutoIncrementing: true);
                case PostgreSqlTypes.sql_bigint:
                case PostgreSqlTypes.sql_int8:
                    return new DotnetTypeDescriptor(typeof(long));
                case PostgreSqlTypes.sql_bigserial:
                case PostgreSqlTypes.sql_serial8:
                    return new DotnetTypeDescriptor(typeof(long), isAutoIncrementing: true);
                case PostgreSqlTypes.sql_real:
                    return new DotnetTypeDescriptor(typeof(float));
                case PostgreSqlTypes.sql_float4:
                case PostgreSqlTypes.sql_double_precision:
                    return new DotnetTypeDescriptor(typeof(double));
                case PostgreSqlTypes.sql_decimal:
                case PostgreSqlTypes.sql_float8:
                case PostgreSqlTypes.sql_money:
                case PostgreSqlTypes.sql_numeric:
                    return new DotnetTypeDescriptor(typeof(decimal))
                    {
                        Precision = d.Precision ?? 16,
                        Scale = d.Scale ?? 4
                    };
                default:
                    return new DotnetTypeDescriptor(typeof(int));
            }
        });
    }

    private static SqlTypeToDotnetTypeConverter GetDateTimeToDotnetTypeConverter()
    {
        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case PostgreSqlTypes.sql_timestamp_without_time_zone:
                case PostgreSqlTypes.sql_timestamp:
                    return new DotnetTypeDescriptor(typeof(DateTime));
                case PostgreSqlTypes.sql_timestamp_with_time_zone:
                case PostgreSqlTypes.sql_timestamptz:
                    return new DotnetTypeDescriptor(typeof(DateTimeOffset));
                case PostgreSqlTypes.sql_interval:
                    return new DotnetTypeDescriptor(typeof(TimeSpan));
                case PostgreSqlTypes.sql_time:
                case PostgreSqlTypes.sql_time_without_timezone:
                case PostgreSqlTypes.sql_timetz:
                case PostgreSqlTypes.sql_time_with_time_zone:
                    return new DotnetTypeDescriptor(typeof(TimeOnly));
                case PostgreSqlTypes.sql_date:
                    return new DotnetTypeDescriptor(typeof(DateOnly));
                default:
                    return new DotnetTypeDescriptor(typeof(DateTime));
            }
        });
    }

    private static SqlTypeToDotnetTypeConverter GetGuidToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(typeof(Guid));
        });
    }

    private static SqlTypeToDotnetTypeConverter GetTextToDotnetTypeConverter()
    {
        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case PostgreSqlTypes.sql_char:
                case PostgreSqlTypes.sql_character:
                    return new DotnetTypeDescriptor(
                        typeof(string),
                        d.Length,
                        isUnicode: d.IsUnicode.GetValueOrDefault(true),
                        isFixedLength: true
                    );
                case PostgreSqlTypes.sql_bit:
                case PostgreSqlTypes.sql_bit_varying:
                case PostgreSqlTypes.sql_varbit:
                case PostgreSqlTypes.sql_varchar:
                case PostgreSqlTypes.sql_character_varying:
                case PostgreSqlTypes.sql_text:
                case PostgreSqlTypes.sql_bpchar:
                    return new DotnetTypeDescriptor(
                        typeof(string),
                        d.Length,
                        isUnicode: d.IsUnicode.GetValueOrDefault(true),
                        isFixedLength: false
                    );
            }

            return null;
        });
    }

    private static SqlTypeToDotnetTypeConverter GetXmlToDotnetTypeConverter()
    {
        return new(d => new DotnetTypeDescriptor(typeof(XDocument)));
    }

    private static SqlTypeToDotnetTypeConverter GetJsonToDotnetTypeConverter()
    {
        return new(d => new DotnetTypeDescriptor(typeof(JsonDocument)));
    }

    private static SqlTypeToDotnetTypeConverter GetByteArrayToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(
                typeof(byte[]),
                d.Length ?? int.MaxValue,
                isFixedLength: d.IsFixedLength.GetValueOrDefault(false)
            );
        });
    }

    private static SqlTypeToDotnetTypeConverter GetGeometricToDotnetTypeConverter()
    {
        // NetTopologySuite types
        var sqlNetTopologyGeometryType = Type.GetType(
            "NetTopologySuite.Geometries.Geometry, NetTopologySuite"
        );
        var sqlNetTopologyPointType = Type.GetType(
            "NetTopologySuite.Geometries.Point, NetTopologySuite"
        );
        var sqlNetTopologyLineStringType = Type.GetType(
            "NetTopologySuite.Geometries.LineString, NetTopologySuite"
        );
        var sqlNetTopologyPolygonType = Type.GetType(
            "NetTopologySuite.Geometries.Polygon, NetTopologySuite"
        );
        var sqlNetTopologyMultiPointType = Type.GetType(
            "NetTopologySuite.Geometries.MultiPoint, NetTopologySuite"
        );
        var sqlNetTopologyMultLineStringType = Type.GetType(
            "NetTopologySuite.Geometries.MultiLineString, NetTopologySuite"
        );
        var sqlNetTopologyMultiPolygonType = Type.GetType(
            "NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite"
        );
        var sqlNetTopologyGeometryCollectionType = Type.GetType(
            "NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite"
        );

        // Geometry affinity
        var sqlNpgsqlPoint = Type.GetType("NpgsqlTypes.NpgsqlPoint, Npgsql");
        var sqlNpgsqlLSeg = Type.GetType("NpgsqlTypes.NpgsqlLSeg, Npgsql");
        var sqlNpgsqlPath = Type.GetType("NpgsqlTypes.NpgsqlPath, Npgsql");
        var sqlNpgsqlPolygon = Type.GetType("NpgsqlTypes.NpgsqlPolygon, Npgsql");
        var sqlNpgsqlLine = Type.GetType("NpgsqlTypes.NpgsqlLine, Npgsql");
        var sqlNpgsqlCircle = Type.GetType("NpgsqlTypes.NpgsqlCircle, Npgsql");
        var sqlNpgsqlBox = Type.GetType("NpgsqlTypes.NpgsqlBox, Npgsql");
        var sqlGeometry = Type.GetType(
            "NetTopologySuite.Geometries.Geometry, NetTopologySuite",
            false,
            false
        );

        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case PostgreSqlTypes.sql_box:
                    if (sqlNetTopologyGeometryType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                    if (sqlNpgsqlBox != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlBox);
                    if (sqlNetTopologyGeometryType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_circle:
                    if (sqlNetTopologyGeometryType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                    if (sqlNpgsqlCircle != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlCircle);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_geography:
                    if (sqlNetTopologyGeometryType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                    if (sqlGeometry != null)
                        return new DotnetTypeDescriptor(sqlGeometry);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_geometry:
                    if (sqlNetTopologyGeometryType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                    if (sqlGeometry != null)
                        return new DotnetTypeDescriptor(sqlGeometry);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_line:
                    if (sqlNetTopologyLineStringType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyLineStringType);
                    if (sqlNpgsqlLine != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlLine);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_lseg:
                    if (sqlNpgsqlLSeg != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlLSeg);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_path:
                    if (sqlNpgsqlPath != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlPath);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_point:
                    if (sqlNetTopologyPointType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyPointType);
                    if (sqlNpgsqlPoint != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlPoint);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_polygon:
                    if (sqlNetTopologyPolygonType != null)
                        return new DotnetTypeDescriptor(sqlNetTopologyPolygonType);
                    if (sqlNpgsqlPolygon != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlPolygon);
                    return new DotnetTypeDescriptor(typeof(object));
            }

            return null;
        });
    }

    private static SqlTypeToDotnetTypeConverter GetMiscellaneousToDotnetTypeConverter()
    {
        var sqlNpgsqlInet = Type.GetType("NpgsqlTypes.NpgsqlInet, Npgsql");
        var sqlNpgsqlCidr = Type.GetType("NpgsqlTypes.NpgsqlCidr, Npgsql");
        var sqlPhysicalAddress = Type.GetType(
            "System.Net.NetworkInformation.PhysicalAddress, System.Net.NetworkInformation",
            false,
            false
        );
        var sqlNpgsqlInterval = Type.GetType("NpgsqlTypes.NpgsqlInterval, Npgsql");
        var sqlNpgsqlTid = Type.GetType("NpgsqlTypes.NpgsqlTid, Npgsql");
        var sqlNpgsqlTsQuery = Type.GetType("NpgsqlTypes.NpgsqlTsQuery, Npgsql");
        var sqlNpgsqlTsVector = Type.GetType("NpgsqlTypes.NpgsqlTsVector, Npgsql");

        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case PostgreSqlTypes.sql_cidr:
                    if (sqlNpgsqlCidr != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlCidr);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_citext:
                    return new DotnetTypeDescriptor(typeof(string));
                case PostgreSqlTypes.sql_hstore:
                    return new DotnetTypeDescriptor(typeof(Dictionary<string, string>));
                case PostgreSqlTypes.sql_inet:
                    return new DotnetTypeDescriptor(typeof(IPAddress));
                case PostgreSqlTypes.sql_lquery:
                case PostgreSqlTypes.sql_ltree:
                case PostgreSqlTypes.sql_ltxtquery:
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_macaddr:
                case PostgreSqlTypes.sql_macaddr8:
                    return new DotnetTypeDescriptor(typeof(PhysicalAddress));
                case PostgreSqlTypes.sql_interval:
                    if (sqlNpgsqlInterval != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlInterval);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_int2vector:
                    return new DotnetTypeDescriptor(typeof(int[]));
                case PostgreSqlTypes.sql_oid:
                    return new DotnetTypeDescriptor(typeof(uint));
                case PostgreSqlTypes.sql_oidvector:
                    return new DotnetTypeDescriptor(typeof(uint[]));
                case PostgreSqlTypes.sql_pg_lsn:
                case PostgreSqlTypes.sql_pg_snapshot:
                case PostgreSqlTypes.sql_refcursor:
                case PostgreSqlTypes.sql_regclass:
                case PostgreSqlTypes.sql_regcollation:
                case PostgreSqlTypes.sql_regconfig:
                case PostgreSqlTypes.sql_regdictionary:
                case PostgreSqlTypes.sql_regnamespace:
                case PostgreSqlTypes.sql_regrole:
                case PostgreSqlTypes.sql_regtype:
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_tid:
                    if (sqlNpgsqlTid != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlTid);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_tsquery:
                    if (sqlNpgsqlTsQuery != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlTsQuery);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_tsvector:
                    if (sqlNpgsqlTsVector != null)
                        return new DotnetTypeDescriptor(sqlNpgsqlTsVector);
                    return new DotnetTypeDescriptor(typeof(object));
                case PostgreSqlTypes.sql_txid_snapshot:
                case PostgreSqlTypes.sql_xid:
                case PostgreSqlTypes.sql_xid8:
                    return new DotnetTypeDescriptor(typeof(object));
            }

            return null;
        });
    }

    private static SqlTypeToDotnetTypeConverter GetRangeToDotnetTypeConverter()
    {
        var rangeType = Type.GetType("NpgsqlTypes.NpgsqlRange`1, Npgsql");
        if (rangeType == null)
            return new(d => new DotnetTypeDescriptor(typeof(object)));

        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case PostgreSqlTypes.sql_datemultirange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(DateOnly)).MakeArrayType()
                    );
                case PostgreSqlTypes.sql_daterange:
                    return new DotnetTypeDescriptor(rangeType.MakeGenericType(typeof(DateOnly)));
                case PostgreSqlTypes.sql_int4multirange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(int)).MakeArrayType()
                    );
                case PostgreSqlTypes.sql_int4range:
                    return new DotnetTypeDescriptor(rangeType.MakeGenericType(typeof(int)));
                case PostgreSqlTypes.sql_int8multirange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(long)).MakeArrayType()
                    );
                case PostgreSqlTypes.sql_int8range:
                    return new DotnetTypeDescriptor(rangeType.MakeGenericType(typeof(long)));
                case PostgreSqlTypes.sql_nummultirange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(double)).MakeArrayType()
                    );
                case PostgreSqlTypes.sql_numrange:
                    return new DotnetTypeDescriptor(rangeType.MakeGenericType(typeof(double)));
                case PostgreSqlTypes.sql_tsrange:
                    return new DotnetTypeDescriptor(rangeType.MakeGenericType(typeof(DateTime)));
                case PostgreSqlTypes.sql_tsmultirange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(DateTime)).MakeArrayType()
                    );
                case PostgreSqlTypes.sql_tstzrange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(DateTimeOffset))
                    );
                case PostgreSqlTypes.sql_tstzmultirange:
                    return new DotnetTypeDescriptor(
                        rangeType.MakeGenericType(typeof(DateTimeOffset)).MakeArrayType()
                    );
            }

            return null;
        });
    }

    #endregion // SqlTypeToDotnetTypeConverters
}
