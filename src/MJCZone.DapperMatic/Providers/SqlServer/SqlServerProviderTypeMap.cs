using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using MJCZone.DapperMatic.Converters;

namespace MJCZone.DapperMatic.Providers.SqlServer;

// See:
// https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
// https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/media/sql-clr-type-mapping.png

/// <summary>
/// Provides a type map for SQL Server, mapping .NET types to SQL Server types and vice versa.
/// </summary>
public sealed class SqlServerProviderTypeMap : DbProviderTypeMapBase<SqlServerProviderTypeMap>
{
    /// <inheritdoc/>
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

        // Geometry types (support NetTopologySuite and SQL Server specific types)
        RegisterConverterForTypes(
            geometricConverter,
            TypeMappingHelpers.GetGeometryTypesForProvider("sqlserver")
        );
    }

    /// <inheritdoc/>
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
        var objectConverter = GetObjectToDotnetTypeConverter();
        var geometricConverter = GetGeometricToDotnetTypeConverter();

        // Boolean affinity (in SQL Server, the bit type is used for boolean values, it consists of 0 or 1)
        RegisterConverter(SqlServerTypes.sql_bit, booleanConverter);

        // Numeric affinity
        RegisterConverterForTypes(
            numericConverter,
            SqlServerTypes.sql_tinyint,
            SqlServerTypes.sql_smallint,
            SqlServerTypes.sql_int,
            SqlServerTypes.sql_bigint,
            SqlServerTypes.sql_real,
            SqlServerTypes.sql_float,
            SqlServerTypes.sql_decimal,
            SqlServerTypes.sql_numeric,
            SqlServerTypes.sql_money,
            SqlServerTypes.sql_smallmoney
        );

        // Guid affinity
        RegisterConverter(SqlServerTypes.sql_uniqueidentifier, guidConverter);

        // Text affinity
        RegisterConverterForTypes(
            textConverter,
            SqlServerTypes.sql_nvarchar,
            SqlServerTypes.sql_varchar,
            SqlServerTypes.sql_ntext,
            SqlServerTypes.sql_text,
            SqlServerTypes.sql_nchar,
            SqlServerTypes.sql_char
        );

        // Xml affinity
        RegisterConverter(SqlServerTypes.sql_xml, xmlConverter);

        // Json affinity (only for very latest versions of SQL Server)
        RegisterConverter(SqlServerTypes.sql_json, jsonConverter);

        // DateTime affinity
        RegisterConverterForTypes(
            dateTimeConverter,
            SqlServerTypes.sql_smalldatetime,
            SqlServerTypes.sql_datetime,
            SqlServerTypes.sql_datetime2,
            SqlServerTypes.sql_datetimeoffset,
            SqlServerTypes.sql_time,
            SqlServerTypes.sql_date,
            SqlServerTypes.sql_timestamp,
            SqlServerTypes.sql_rowversion
        );

        // Binary affinity
        RegisterConverterForTypes(
            byteArrayConverter,
            SqlServerTypes.sql_varbinary,
            SqlServerTypes.sql_binary,
            SqlServerTypes.sql_image
        );

        // Object affinity
        RegisterConverter(SqlServerTypes.sql_variant, objectConverter);

        // Geometry affinity
        RegisterConverterForTypes(
            geometricConverter,
            SqlServerTypes.sql_geometry,
            SqlServerTypes.sql_geography,
            SqlServerTypes.sql_hierarchyid
        );
    }

    #region DotnetTypeToSqlTypeConverters

    private static DotnetTypeToSqlTypeConverter GetBooleanToSqlTypeConverter()
    {
        return new(d =>
        {
            return new(SqlServerTypes.sql_bit) { Length = 1 };
        });
    }

    private static DotnetTypeToSqlTypeConverter GetGuidToSqlTypeConverter()
    {
        return new(d =>
        {
            return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_uniqueidentifier);
        });
    }

    private static DotnetTypeToSqlTypeConverter GetNumbericToSqlTypeConverter()
    {
        return new(d =>
        {
            switch (d.DotnetType)
            {
                case Type t when t == typeof(byte):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_tinyint);
                case Type t when t == typeof(sbyte):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_tinyint);
                case Type t when t == typeof(short):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_smallint);
                case Type t when t == typeof(ushort):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_smallint);
                case Type t when t == typeof(int):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_int);
                case Type t when t == typeof(uint):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_int);
                case Type t when t == typeof(BigInteger) || t == typeof(long):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_bigint);
                case Type t when t == typeof(ulong):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_bigint);
                case Type t when t == typeof(float):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_real);
                case Type t when t == typeof(double):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_float);
                case Type t when t == typeof(decimal):
                    return TypeMappingHelpers.CreateDecimalType(SqlServerTypes.sql_decimal, d.Precision, d.Scale);
                default:
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_int);
            }
        });
    }

    private static DotnetTypeToSqlTypeConverter GetTextToSqlTypeConverter()
    {
        return new(d =>
        {
            var sqlType = d.IsFixedLength == true
                ? (d.IsUnicode == true ? SqlServerTypes.sql_nchar : SqlServerTypes.sql_char)
                : (d.IsUnicode == true ? SqlServerTypes.sql_nvarchar : SqlServerTypes.sql_varchar);

            return TypeMappingHelpers.CreateStringType(
                sqlType,
                d.Length,
                d.IsUnicode.GetValueOrDefault(false),
                d.IsFixedLength.GetValueOrDefault(false));
        });
    }

    private static DotnetTypeToSqlTypeConverter GetXmlToSqlTypeConverter()
    {
        return new(d =>
        {
            return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_xml);
        });
    }

    private static DotnetTypeToSqlTypeConverter GetJsonToSqlTypeConverter()
    {
        return new(d =>
        {
            var sqlType = d.IsUnicode == true ? "nvarchar(max)" : "varchar(max)";
            return TypeMappingHelpers.CreateJsonType(sqlType, isText: true);
        });
    }

    private DotnetTypeToSqlTypeConverter GetDateTimeToSqlTypeConverter()
    {
        return new(d =>
        {
            switch (d.DotnetType)
            {
                case Type t when t == typeof(DateTime):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_datetime);
                case Type t when t == typeof(DateTimeOffset):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_datetimeoffset);
                case Type t when t == typeof(TimeSpan):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_time);
                case Type t when t == typeof(DateOnly):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_date);
                case Type t when t == typeof(TimeOnly):
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_time);
                default:
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_datetime);
            }
        });
    }

    private DotnetTypeToSqlTypeConverter GetByteArrayToSqlTypeConverter()
    {
        return new(d =>
        {
            var sqlType = d.IsFixedLength == true ? SqlServerTypes.sql_binary : SqlServerTypes.sql_varbinary;
            return TypeMappingHelpers.CreateBinaryType(
                sqlType,
                d.Length,
                d.IsFixedLength.GetValueOrDefault(false));
        });
    }

    private DotnetTypeToSqlTypeConverter GetObjectToSqlTypeConverter()
    {
        return new(d =>
        {
            return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_variant);
        });
    }

    private DotnetTypeToSqlTypeConverter GetEnumerableToSqlTypeConverter() =>
        GetJsonToSqlTypeConverter();

    private DotnetTypeToSqlTypeConverter GetEnumToSqlTypeConverter()
    {
        return new(d =>
        {
            return TypeMappingHelpers.CreateEnumStringType(SqlServerTypes.sql_varchar);
        });
    }

    private DotnetTypeToSqlTypeConverter GetArrayToSqlTypeConverter() =>
        GetJsonToSqlTypeConverter();

    private DotnetTypeToSqlTypeConverter GetPocoToSqlTypeConverter() => GetJsonToSqlTypeConverter();

    private DotnetTypeToSqlTypeConverter GetGeometricToSqlTypeConverter()
    {
        return new(d =>
        {
            var shortName = TypeMappingHelpers.GetAssemblyQualifiedShortName(d.DotnetType);
            if (string.IsNullOrWhiteSpace(shortName))
            {
                return null;
            }

            switch (shortName)
            {
                // NetTopologySuite types
                case "NetTopologySuite.Geometries.Geometry, NetTopologySuite":
                    return TypeMappingHelpers.CreateGeometryType(SqlServerTypes.sql_geometry);
                case "NetTopologySuite.Geometries.Point, NetTopologySuite":
                case "NetTopologySuite.Geometries.LineString, NetTopologySuite":
                case "NetTopologySuite.Geometries.Polygon, NetTopologySuite":
                case "NetTopologySuite.Geometries.MultiPoint, NetTopologySuite":
                case "NetTopologySuite.Geometries.MultiLineString, NetTopologySuite":
                case "NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite":
                case "NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite":
                    return TypeMappingHelpers.CreateLobType("nvarchar(max)", isUnicode: true);
                // SQL Server types
                case "Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types":
                    return TypeMappingHelpers.CreateGeometryType(SqlServerTypes.sql_geometry);
                case "Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types":
                    return TypeMappingHelpers.CreateGeometryType(SqlServerTypes.sql_geography);
                case "Microsoft.SqlServer.Types.SqlHierarchyId, Microsoft.SqlServer.Types":
                    return TypeMappingHelpers.CreateSimpleType(SqlServerTypes.sql_hierarchyid);
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
                case SqlServerTypes.sql_tinyint:
                    return new DotnetTypeDescriptor(typeof(byte));
                case SqlServerTypes.sql_smallint:
                    return new DotnetTypeDescriptor(typeof(short));
                case SqlServerTypes.sql_int:
                    return new DotnetTypeDescriptor(typeof(int));
                case SqlServerTypes.sql_bigint:
                    return new DotnetTypeDescriptor(typeof(long));
                case SqlServerTypes.sql_real:
                    return new DotnetTypeDescriptor(typeof(float));
                case SqlServerTypes.sql_float:
                    return new DotnetTypeDescriptor(typeof(double));
                case SqlServerTypes.sql_decimal:
                    return new DotnetTypeDescriptor(typeof(decimal))
                    {
                        Precision = d.Precision ?? 16,
                        Scale = d.Scale ?? 4,
                    };
                case SqlServerTypes.sql_numeric:
                    return new DotnetTypeDescriptor(typeof(decimal))
                    {
                        Precision = d.Precision ?? 16,
                        Scale = d.Scale ?? 4,
                    };
                case SqlServerTypes.sql_money:
                    return new DotnetTypeDescriptor(typeof(decimal))
                    {
                        Precision = d.Precision ?? 19,
                        Scale = d.Scale ?? 4,
                    };
                case SqlServerTypes.sql_smallmoney:
                    return new DotnetTypeDescriptor(typeof(decimal))
                    {
                        Precision = d.Precision ?? 10,
                        Scale = d.Scale ?? 4,
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
            return new DotnetTypeDescriptor(typeof(Guid));
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

    private static SqlTypeToDotnetTypeConverter GetXmlToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(typeof(XDocument));
        });
    }

    private static SqlTypeToDotnetTypeConverter GetJsonToDotnetTypeConverter()
    {
        return new(d =>
        {
            return new DotnetTypeDescriptor(typeof(JsonDocument));
        });
    }

    private static SqlTypeToDotnetTypeConverter GetDateTimeToDotnetTypeConverter()
    {
        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case SqlServerTypes.sql_smalldatetime:
                case SqlServerTypes.sql_datetime:
                case SqlServerTypes.sql_datetime2:
                case SqlServerTypes.sql_timestamp:
                    return new DotnetTypeDescriptor(typeof(DateTime));
                case SqlServerTypes.sql_datetimeoffset:
                    return new DotnetTypeDescriptor(typeof(DateTimeOffset));
                case SqlServerTypes.sql_time:
                    return new DotnetTypeDescriptor(typeof(TimeOnly));
                case SqlServerTypes.sql_date:
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

    private static SqlTypeToDotnetTypeConverter GetGeometricToDotnetTypeConverter()
    {
        // NetTopologySuite types
        var sqlNetTopologyGeometryType = Type.GetType(
            "NetTopologySuite.Geometries.Geometry, NetTopologySuite"
        );

        // var sqlNetTopologyPointType = Type.GetType(
        //     "NetTopologySuite.Geometries.Point, NetTopologySuite"
        // );
        // var sqlNetTopologyLineStringType = Type.GetType(
        //     "NetTopologySuite.Geometries.LineString, NetTopologySuite"
        // );
        // var sqlNetTopologyPolygonType = Type.GetType(
        //     "NetTopologySuite.Geometries.Polygon, NetTopologySuite"
        // );
        // var sqlNetTopologyMultiPointType = Type.GetType(
        //     "NetTopologySuite.Geometries.MultiPoint, NetTopologySuite"
        // );
        // var sqlNetTopologyMultLineStringType = Type.GetType(
        //     "NetTopologySuite.Geometries.MultiLineString, NetTopologySuite"
        // );
        // var sqlNetTopologyMultiPolygonType = Type.GetType(
        //     "NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite"
        // );
        // var sqlNetTopologyGeometryCollectionType = Type.GetType(
        //     "NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite"
        // );

        // Geometry affinity
        var sqlGeometryType = Type.GetType(
            "Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types",
            false,
            false
        );
        var sqlGeographyType = Type.GetType(
            "Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types",
            false,
            false
        );
        var sqlHierarchyIdType = Type.GetType(
            "Microsoft.SqlServer.Types.SqlHierarchyId, Microsoft.SqlServer.Types",
            false,
            false
        );

        return new(d =>
        {
            switch (d.BaseTypeName)
            {
                case SqlServerTypes.sql_geometry:
                    if (sqlNetTopologyGeometryType != null)
                    {
                        return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                    }

                    if (sqlGeometryType != null)
                    {
                        return new DotnetTypeDescriptor(sqlGeometryType);
                    }

                    return new DotnetTypeDescriptor(typeof(object));
                case SqlServerTypes.sql_geography:
                    if (sqlGeographyType != null)
                    {
                        return new DotnetTypeDescriptor(sqlGeographyType);
                    }

                    return new DotnetTypeDescriptor(typeof(object));
                case SqlServerTypes.sql_hierarchyid:
                    if (sqlHierarchyIdType != null)
                    {
                        return new DotnetTypeDescriptor(sqlHierarchyIdType);
                    }

                    return new DotnetTypeDescriptor(typeof(object));
            }

            return null;
        });
    }

    #endregion // SqlTypeToDotnetTypeConverters
}
