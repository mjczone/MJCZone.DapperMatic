using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace DapperMatic.Providers.Sqlite;

// See:
// https://www.sqlite.org/datatype3.html
public sealed class SqliteProviderTypeMap : DbProviderTypeMapBase<SqliteProviderTypeMap>
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
            Type.GetType("NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite")
        );
    }

    protected override void RegisterSqlTypeToDotnetTypeConverters()
    {
        var booleanConverter = GetBooleanToDotnetTypeConverter();
        var numericConverter = GetNumbericToDotnetTypeConverter();
        var guidConverter = GetGuidToDotnetTypeConverter();
        var textConverter = GetTextToDotnetTypeConverter();
        var dateTimeConverter = GetDateTimeToDotnetTypeConverter();
        var byteArrayConverter = GetByteArrayToDotnetTypeConverter();
        var objectConverter = GetObjectToDotnetTypeConverter();

        // Boolean affinity
        RegisterConverterForTypes(booleanConverter, SqliteTypes.sql_bool, SqliteTypes.sql_boolean);

        // Numeric affinity
        RegisterConverterForTypes(
            numericConverter,
            SqliteTypes.sql_tinyint,
            SqliteTypes.sql_smallint,
            SqliteTypes.sql_int,
            SqliteTypes.sql_integer,
            SqliteTypes.sql_mediumint,
            SqliteTypes.sql_unsigned_big_int,
            SqliteTypes.sql_bigint,
            SqliteTypes.sql_real,
            SqliteTypes.sql_float,
            SqliteTypes.sql_decimal,
            SqliteTypes.sql_numeric,
            SqliteTypes.sql_double,
            SqliteTypes.sql_double_precision,
            SqliteTypes.sql_int2,
            SqliteTypes.sql_int4,
            SqliteTypes.sql_int8
        );

        // Guid affinity
        RegisterConverterForTypes(guidConverter, SqliteTypes.sql_char, SqliteTypes.sql_varchar);

        // Text affinity
        RegisterConverterForTypes(
            textConverter,
            SqliteTypes.sql_nvarchar,
            SqliteTypes.sql_varchar,
            SqliteTypes.sql_varying_character,
            SqliteTypes.sql_native_character,
            SqliteTypes.sql_text,
            SqliteTypes.sql_nchar,
            SqliteTypes.sql_char,
            SqliteTypes.sql_character
        );

        // DateTime affinity
        RegisterConverterForTypes(
            dateTimeConverter,
            SqliteTypes.sql_datetime,
            SqliteTypes.sql_time,
            SqliteTypes.sql_date,
            SqliteTypes.sql_timestamp,
            SqliteTypes.sql_year
        );

        // Binary affinity
        RegisterConverterForTypes(byteArrayConverter, SqliteTypes.sql_blob);

        // Object affinity
        RegisterConverterForTypes(objectConverter, SqliteTypes.sql_clob);
    }

    #region DotnetTypeToSqlTypeConverters

    private static DotnetTypeToSqlTypeConverter GetBooleanToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_boolean);
        });
    }

    private static DotnetTypeToSqlTypeConverter GetGuidToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_varchar) { Length = 36 };
        });
    }

    private static DotnetTypeToSqlTypeConverter GetNumbericToSqlTypeConverter()
    {
        return new(d =>
        {
            switch (d.DotnetType)
            {
                case Type t when t == typeof(byte):
                    return new(SqliteTypes.sql_tinyint);
                case Type t when t == typeof(sbyte):
                    return new(SqliteTypes.sql_tinyint);
                case Type t when t == typeof(short):
                    return new(SqliteTypes.sql_smallint);
                case Type t when t == typeof(ushort):
                    return new(SqliteTypes.sql_smallint);
                case Type t when t == typeof(int):
                    return new(SqliteTypes.sql_int);
                case Type t when t == typeof(uint):
                    return new(SqliteTypes.sql_int);
                case Type t when t == typeof(BigInteger) || t == typeof(long):
                    return new(SqliteTypes.sql_bigint);
                case Type t when t == typeof(ulong):
                    return new(SqliteTypes.sql_bigint);
                case Type t when t == typeof(float):
                    return new(SqliteTypes.sql_real);
                case Type t when t == typeof(double):
                    return new(SqliteTypes.sql_float);
                case Type t when t == typeof(decimal):
                    var precision = d.Precision ?? 16;
                    var scale = d.Scale ?? 4;
                    return new(SqliteTypes.sql_decimal)
                    {
                        SqlTypeName = $"decimal({precision},{scale})",
                        Precision = precision,
                        Scale = scale
                    };
                default:
                    return new(SqliteTypes.sql_int);
            }
        });
    }

    private static DotnetTypeToSqlTypeConverter GetTextToSqlTypeConverter()
    {
        return new(d =>
        {
            var length = d.Length.GetValueOrDefault(255);
            if (length == int.MaxValue)
            {
                return d.IsUnicode == true
                    ? new(SqliteTypes.sql_nvarchar)
                    {
                        SqlTypeName = "nvarchar(max)",
                        Length = int.MaxValue
                    }
                    : new(SqliteTypes.sql_varchar)
                    {
                        SqlTypeName = "varchar(max)",
                        Length = int.MaxValue
                    };
            }

            if (d.IsFixedLength == true)
            {
                return d.IsUnicode == true
                    ? new(SqliteTypes.sql_nchar)
                    {
                        SqlTypeName = $"nchar({length})",
                        Length = length
                    }
                    : new(SqliteTypes.sql_char)
                    {
                        SqlTypeName = $"char({length})",
                        Length = length
                    };
            }

            return d.IsUnicode == true
                ? new(SqliteTypes.sql_nvarchar)
                {
                    SqlTypeName = $"nvarchar({length})",
                    Length = length
                }
                : new(SqliteTypes.sql_varchar)
                {
                    SqlTypeName = $"varchar({length})",
                    Length = length
                };
        });
    }

    private static DotnetTypeToSqlTypeConverter GetXmlToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_text);
        });
    }

    private static DotnetTypeToSqlTypeConverter GetJsonToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_text);
        });
    }

    private DotnetTypeToSqlTypeConverter GetDateTimeToSqlTypeConverter()
    {
        return new(d =>
        {
            switch (d.DotnetType)
            {
                case Type t when t == typeof(DateTime):
                    return new(SqliteTypes.sql_datetime);
                case Type t when t == typeof(DateTimeOffset):
                    return new(SqliteTypes.sql_datetime);
                case Type t when t == typeof(TimeSpan):
                    return new(SqliteTypes.sql_time);
                case Type t when t == typeof(DateOnly):
                    return new(SqliteTypes.sql_date);
                case Type t when t == typeof(TimeOnly):
                    return new(SqliteTypes.sql_time);
                default:
                    return new(SqliteTypes.sql_datetime);
            }
        });
    }

    private DotnetTypeToSqlTypeConverter GetByteArrayToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_blob);
        });
    }

    private DotnetTypeToSqlTypeConverter GetObjectToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_clob);
        });
    }

    private DotnetTypeToSqlTypeConverter GetEnumerableToSqlTypeConverter() =>
        GetJsonToSqlTypeConverter();

    private DotnetTypeToSqlTypeConverter GetEnumToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqliteTypes.sql_varchar) { SqlTypeName = "varchar(128)", Length = 128 };
        });
    }

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
                case "NetTopologySuite.Geometries.Point, NetTopologySuite":
                case "NetTopologySuite.Geometries.LineString, NetTopologySuite":
                case "NetTopologySuite.Geometries.Polygon, NetTopologySuite":
                case "NetTopologySuite.Geometries.MultiPoint, NetTopologySuite":
                case "NetTopologySuite.Geometries.MultiLineString, NetTopologySuite":
                case "NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite":
                case "NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite":
                    return new(SqliteTypes.sql_text);
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
                case SqliteTypes.sql_tinyint:
                    return new DotnetTypeDescriptor(typeof(byte));
                case SqliteTypes.sql_smallint:
                case SqliteTypes.sql_int2:
                    return new DotnetTypeDescriptor(typeof(short));
                case SqliteTypes.sql_int:
                case SqliteTypes.sql_int4:
                case SqliteTypes.sql_integer:
                case SqliteTypes.sql_mediumint:
                    return new DotnetTypeDescriptor(typeof(int));
                case SqliteTypes.sql_unsigned_big_int:
                case SqliteTypes.sql_bigint:
                case SqliteTypes.sql_int8:
                    return new DotnetTypeDescriptor(typeof(long));
                case SqliteTypes.sql_real:
                    return new DotnetTypeDescriptor(typeof(float));
                case SqliteTypes.sql_float:
                case SqliteTypes.sql_double:
                case SqliteTypes.sql_double_precision:
                    return new DotnetTypeDescriptor(typeof(double));
                case SqliteTypes.sql_decimal:
                case SqliteTypes.sql_numeric:
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

    private static SqlTypeToDotnetTypeConverter GetGuidToDotnetTypeConverter()
    {
        return new(d =>
        {
            if (d.Length == 36)
            {
                return new DotnetTypeDescriptor(typeof(Guid));
            }

            // move on to the next type converter
            return null;
        });
    }

    private static SqlTypeToDotnetTypeConverter GetTextToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(
                typeof(string),
                d.Length ?? 255,
                isUnicode: d.IsUnicode.GetValueOrDefault(true),
                isFixedLength: d.IsFixedLength.GetValueOrDefault(false)
            );
        });
    }

    private static SqlTypeToDotnetTypeConverter GetDateTimeToDotnetTypeConverter()
    {
        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case SqliteTypes.sql_datetime:
                case SqliteTypes.sql_timestamp:
                    return new DotnetTypeDescriptor(typeof(DateTime));
                case SqliteTypes.sql_time:
                    return new DotnetTypeDescriptor(typeof(TimeOnly));
                case SqliteTypes.sql_date:
                    return new DotnetTypeDescriptor(typeof(DateOnly));
                default:
                    return new DotnetTypeDescriptor(typeof(DateTime));
            }
        });
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

    private static SqlTypeToDotnetTypeConverter GetObjectToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(typeof(object));
        });
    }

    #endregion // SqlTypeToDotnetTypeConverters
}
