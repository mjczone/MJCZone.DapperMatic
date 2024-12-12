using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using MJCZone.DapperMatic.Converters;

namespace MJCZone.DapperMatic.Providers.MySql
{
    /// <summary>
    /// Provides type mapping for MySQL database provider.
    /// </summary>
    /// <remarks>
    /// See:
    /// https://dev.mysql.com/doc/connector-net/en/
    /// https://stackoverflow.com/questions/67101765/c-sharp-mysql-dapper-mysqlgeometry
    /// ...
    /// </remarks>
    public sealed class MySqlProviderTypeMap : DbProviderTypeMapBase<MySqlProviderTypeMap>
    {
        /// <summary>
        /// Registers .NET types to SQL types converters.
        /// </summary>
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
                Type.GetType("NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite"),
                // also register the MySQL types
                Type.GetType("MySql.Data.Types.MySqlGeometry, MySql.Data"),
                Type.GetType("MySqlConnector.MySqlGeometry, MySqlConnector")
            );
        }

        /// <summary>
        /// Registers SQL types to .NET types converters.
        /// </summary>
        protected override void RegisterSqlTypeToDotnetTypeConverters()
        {
            var booleanConverter = GetBooleanToDotnetTypeConverter();
            var numericConverter = GetNumericToDotnetTypeConverter();
            var guidConverter = GetGuidToDotnetTypeConverter();
            var textConverter = GetTextToDotnetTypeConverter();
            var jsonConverter = GetJsonToDotnetTypeConverter();
            var dateTimeConverter = GetDateTimeToDotnetTypeConverter();
            var byteArrayConverter = GetByteArrayToDotnetTypeConverter();
            var geometricConverter = GetGeometricToDotnetTypeConverter();

            // Boolean affinity (in MySQL, bool and boolean are the same, they are synonyms of tinyint(1))
            RegisterConverterForTypes(
                booleanConverter,
                MySqlTypes.sql_bool,
                MySqlTypes.sql_boolean
            );

            // Numeric affinity
            RegisterConverterForTypes(
                numericConverter,
                MySqlTypes.sql_bit,
                MySqlTypes.sql_tinyint,
                MySqlTypes.sql_tinyint_unsigned,
                MySqlTypes.sql_smallint,
                MySqlTypes.sql_smallint_unsigned,
                MySqlTypes.sql_mediumint,
                MySqlTypes.sql_mediumint_unsigned,
                MySqlTypes.sql_int,
                MySqlTypes.sql_int_unsigned,
                MySqlTypes.sql_integer,
                MySqlTypes.sql_integer_unsigned,
                MySqlTypes.sql_bigint,
                MySqlTypes.sql_bigint_unsigned,
                MySqlTypes.sql_serial,
                MySqlTypes.sql_fixed,
                MySqlTypes.sql_real,
                MySqlTypes.sql_float,
                MySqlTypes.sql_dec,
                MySqlTypes.sql_decimal,
                MySqlTypes.sql_numeric,
                MySqlTypes.sql_double,
                MySqlTypes.sql_double_unsigned,
                MySqlTypes.sql_double_precision,
                MySqlTypes.sql_double_precision_unsigned
            );

            // DateTime affinity
            RegisterConverterForTypes(
                dateTimeConverter,
                MySqlTypes.sql_datetime,
                MySqlTypes.sql_timestamp,
                MySqlTypes.sql_time,
                MySqlTypes.sql_date,
                MySqlTypes.sql_year
            );

            // Guid affinity
            RegisterConverterForTypes(guidConverter, MySqlTypes.sql_char, MySqlTypes.sql_varchar);

            // Text affinity
            RegisterConverterForTypes(
                textConverter,
                MySqlTypes.sql_char,
                MySqlTypes.sql_varchar,
                MySqlTypes.sql_long_varchar,
                MySqlTypes.sql_tinytext,
                MySqlTypes.sql_mediumtext,
                MySqlTypes.sql_text,
                MySqlTypes.sql_longtext,
                MySqlTypes.sql_enum,
                MySqlTypes.sql_set
            );

            // Json affinity
            RegisterConverterForTypes(jsonConverter, MySqlTypes.sql_json);

            // Binary affinity
            RegisterConverterForTypes(
                byteArrayConverter,
                MySqlTypes.sql_binary,
                MySqlTypes.sql_varbinary,
                MySqlTypes.sql_long_varbinary,
                MySqlTypes.sql_tinyblob,
                MySqlTypes.sql_blob,
                MySqlTypes.sql_mediumblob,
                MySqlTypes.sql_longblob
            );

            // Geometry affinity
            RegisterConverterForTypes(
                geometricConverter,
                MySqlTypes.sql_geometry,
                MySqlTypes.sql_point,
                MySqlTypes.sql_linestring,
                MySqlTypes.sql_polygon,
                MySqlTypes.sql_multipoint,
                MySqlTypes.sql_multilinestring,
                MySqlTypes.sql_multipolygon,
                MySqlTypes.sql_geomcollection,
                MySqlTypes.sql_geometrycollection
            );
        }

        #region DotnetTypeToSqlTypeConverters

        /// <summary>
        /// Gets the boolean to SQL type converter.
        /// </summary>
        /// <returns>The boolean to SQL type converter.</returns>
        private static DotnetTypeToSqlTypeConverter GetBooleanToSqlTypeConverter()
        {
            return new(d =>
            {
                return new(MySqlTypes.sql_bit) { Length = 1 };
            });
        }

        /// <summary>
        /// Gets the numeric to SQL type converter.
        /// </summary>
        /// <returns>The numeric to SQL type converter.</returns>
        private static DotnetTypeToSqlTypeConverter GetNumbericToSqlTypeConverter()
        {
            return new(d =>
            {
                switch (d.DotnetType)
                {
                    case Type t when t == typeof(byte):
                        return new(MySqlTypes.sql_tinyint);
                    case Type t when t == typeof(sbyte):
                        return new(MySqlTypes.sql_tinyint);
                    case Type t when t == typeof(short):
                        return new(MySqlTypes.sql_smallint);
                    case Type t when t == typeof(ushort):
                        return new(MySqlTypes.sql_smallint);
                    case Type t when t == typeof(int):
                        return new(MySqlTypes.sql_int);
                    case Type t when t == typeof(uint):
                        return new(MySqlTypes.sql_int);
                    case Type t when t == typeof(BigInteger) || t == typeof(long):
                        return new(MySqlTypes.sql_bigint);
                    case Type t when t == typeof(ulong):
                        return new(MySqlTypes.sql_bigint);
                    case Type t when t == typeof(float):
                        return new(MySqlTypes.sql_real);
                    case Type t when t == typeof(double):
                        return new(MySqlTypes.sql_float);
                    case Type t when t == typeof(decimal):
                        var precision = d.Precision ?? 16;
                        var scale = d.Scale ?? 4;
                        return new(MySqlTypes.sql_decimal)
                        {
                            SqlTypeName = $"decimal({precision},{scale})",
                            Precision = precision,
                            Scale = scale,
                        };
                    default:
                        return new(MySqlTypes.sql_int);
                }
            });
        }

        /// <summary>
        /// Gets the GUID to SQL type converter.
        /// </summary>
        /// <returns>The GUID to SQL type converter.</returns>
        private static DotnetTypeToSqlTypeConverter GetGuidToSqlTypeConverter()
        {
            return new(d =>
            {
                return new(MySqlTypes.sql_char) { Length = 36 };
            });
        }

        /// <summary>
        /// Gets the text to SQL type converter.
        /// </summary>
        /// <returns>The text to SQL type converter.</returns>
        private static DotnetTypeToSqlTypeConverter GetTextToSqlTypeConverter()
        {
            return new(d =>
            {
                var length = d.Length.GetValueOrDefault(255);
                if (length == int.MaxValue)
                {
                    return new(MySqlTypes.sql_text);
                }
                if (d.IsFixedLength == true)
                {
                    return new(MySqlTypes.sql_char)
                    {
                        SqlTypeName = $"char({length})",
                        Length = length,
                    };
                }
                return new(MySqlTypes.sql_varchar)
                {
                    SqlTypeName = $"varchar({length})",
                    Length = length,
                };
            });
        }

        /// <summary>
        /// Gets the XML to SQL type converter.
        /// </summary>
        /// <returns>The XML to SQL type converter.</returns>
        private static DotnetTypeToSqlTypeConverter GetXmlToSqlTypeConverter()
        {
            return new(d => new(MySqlTypes.sql_text));
        }

        /// <summary>
        /// Gets the JSON to SQL type converter.
        /// </summary>
        /// <returns>The JSON to SQL type converter.</returns>
        private static DotnetTypeToSqlTypeConverter GetJsonToSqlTypeConverter()
        {
            return new(d => new(MySqlTypes.sql_json));
        }

        /// <summary>
        /// Gets the DateTime to SQL type converter.
        /// </summary>
        /// <returns>The DateTime to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetDateTimeToSqlTypeConverter()
        {
            return new(d =>
            {
                switch (d.DotnetType)
                {
                    case Type t when t == typeof(TimeSpan):
                        return new(MySqlTypes.sql_time);
                    case Type t when t == typeof(DateOnly):
                        return new(MySqlTypes.sql_date);
                    case Type t when t == typeof(TimeOnly):
                        return new(MySqlTypes.sql_time);
                    case Type t when t == typeof(DateTime) || t == typeof(DateTimeOffset):
                    default:
                        var precision = d.Length ?? d.Precision ?? 6;
                        return new(MySqlTypes.sql_datetime)
                        {
                            SqlTypeName = $"datetime({precision})",
                            Precision = precision,
                        };
                }
            });
        }

        /// <summary>
        /// Gets the byte array to SQL type converter.
        /// </summary>
        /// <returns>The byte array to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetByteArrayToSqlTypeConverter()
        {
            return new(d =>
                d.IsFixedLength == true && d.Length.HasValue
                    ? new(MySqlTypes.sql_binary)
                    {
                        SqlTypeName = $"binary({d.Length})",
                        Length = d.Length,
                    }
                    : new(MySqlTypes.sql_blob)
            );
        }

        /// <summary>
        /// Gets the object to SQL type converter.
        /// </summary>
        /// <returns>The object to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetObjectToSqlTypeConverter()
        {
            return new(d => new(MySqlTypes.sql_json));
        }

        /// <summary>
        /// Gets the enumerable to SQL type converter.
        /// </summary>
        /// <returns>The enumerable to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetEnumerableToSqlTypeConverter() =>
            GetJsonToSqlTypeConverter();

        /// <summary>
        /// Gets the enum to SQL type converter.
        /// </summary>
        /// <returns>The enum to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetEnumToSqlTypeConverter()
        {
            return new(d =>
            {
                return new(MySqlTypes.sql_varchar) { SqlTypeName = "varchar(128)", Length = 128 };
            });
        }

        /// <summary>
        /// Gets the array to SQL type converter.
        /// </summary>
        /// <returns>The array to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetArrayToSqlTypeConverter() =>
            GetJsonToSqlTypeConverter();

        /// <summary>
        /// Gets the POCO to SQL type converter.
        /// </summary>
        /// <returns>The POCO to SQL type converter.</returns>
        private DotnetTypeToSqlTypeConverter GetPocoToSqlTypeConverter() =>
            GetJsonToSqlTypeConverter();

        /// <summary>
        /// Gets the geometric to SQL type converter.
        /// </summary>
        /// <returns>The geometric to SQL type converter.</returns>
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
                        return new(MySqlTypes.sql_geometry);
                    case "NetTopologySuite.Geometries.Point, NetTopologySuite":
                        return new(MySqlTypes.sql_point);
                    case "NetTopologySuite.Geometries.LineString, NetTopologySuite":
                        return new(MySqlTypes.sql_linestring);
                    case "NetTopologySuite.Geometries.Polygon, NetTopologySuite":
                        return new(MySqlTypes.sql_polygon);
                    case "NetTopologySuite.Geometries.MultiPoint, NetTopologySuite":
                        return new(MySqlTypes.sql_multipoint);
                    case "NetTopologySuite.Geometries.MultiLineString, NetTopologySuite":
                        return new(MySqlTypes.sql_multilinestring);
                    case "NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite":
                        return new(MySqlTypes.sql_multipolygon);
                    case "NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite":
                        return new(MySqlTypes.sql_geometrycollection);
                    // MySQL types
                    case "MySql.Data.Types.MySqlGeometry, MySql.Data":
                        return new(MySqlTypes.sql_geometry);
                    case "MySqlConnector.MySqlGeometry, MySqlConnector":
                        return new(MySqlTypes.sql_geometry);
                }

                return null;
            });
        }

        #endregion // DotnetTypeToSqlTypeConverters

        #region SqlTypeToDotnetTypeConverters

        /// <summary>
        /// Gets the boolean to .NET type converter.
        /// </summary>
        /// <returns>The boolean to .NET type converter.</returns>
        private static SqlTypeToDotnetTypeConverter GetBooleanToDotnetTypeConverter()
        {
            return new(d =>
            {
                if (
                    d.BaseTypeName == MySqlTypes.sql_bool
                    || d.BaseTypeName == MySqlTypes.sql_boolean
                )
                {
                    return new DotnetTypeDescriptor(typeof(bool));
                }

                return null;
            });
        }

        /// <summary>
        /// Gets the numeric to .NET type converter.
        /// </summary>
        /// <returns>The numeric to .NET type converter.</returns>
        private static SqlTypeToDotnetTypeConverter GetNumericToDotnetTypeConverter()
        {
            return new(d =>
            {
                switch (d.BaseTypeName)
                {
                    case MySqlTypes.sql_bit:
                        if (!d.Length.HasValue || d.Length == 1)
                        {
                            return new DotnetTypeDescriptor(typeof(bool));
                        }

                        if (d.Length == 8)
                        {
                            return new DotnetTypeDescriptor(typeof(byte));
                        }

                        if (d.Length == 16)
                        {
                            return new DotnetTypeDescriptor(typeof(short));
                        }

                        if (d.Length == 32)
                        {
                            return new DotnetTypeDescriptor(typeof(int));
                        }

                        if (d.Length == 64)
                        {
                            return new DotnetTypeDescriptor(typeof(long));
                        }

                        // make it a long if no recognizable length is specified
                        return new DotnetTypeDescriptor(typeof(long));
                    case MySqlTypes.sql_tinyint:
                        return new DotnetTypeDescriptor(typeof(sbyte));
                    case MySqlTypes.sql_tinyint_unsigned:
                        return new DotnetTypeDescriptor(typeof(byte));
                    case MySqlTypes.sql_smallint:
                        return new DotnetTypeDescriptor(typeof(short));
                    case MySqlTypes.sql_smallint_unsigned:
                        return new DotnetTypeDescriptor(typeof(ushort));
                    case MySqlTypes.sql_mediumint:
                    case MySqlTypes.sql_int:
                    case MySqlTypes.sql_integer:
                        return new DotnetTypeDescriptor(typeof(int));
                    case MySqlTypes.sql_serial:
                        return new DotnetTypeDescriptor(typeof(int), isAutoIncrementing: true);
                    case MySqlTypes.sql_mediumint_unsigned:
                    case MySqlTypes.sql_int_unsigned:
                    case MySqlTypes.sql_integer_unsigned:
                        return new DotnetTypeDescriptor(typeof(uint));
                    case MySqlTypes.sql_bigint:
                        return new DotnetTypeDescriptor(typeof(long));
                    case MySqlTypes.sql_bigint_unsigned:
                        return new DotnetTypeDescriptor(typeof(ulong));
                    case MySqlTypes.sql_decimal:
                    case MySqlTypes.sql_dec:
                    case MySqlTypes.sql_fixed:
                    case MySqlTypes.sql_numeric:
                        return new DotnetTypeDescriptor(typeof(decimal))
                        {
                            Precision = d.Precision ?? 16,
                            Scale = d.Scale ?? 4,
                        };
                    case MySqlTypes.sql_float:
                        return new DotnetTypeDescriptor(typeof(float));
                    case MySqlTypes.sql_real:
                    case MySqlTypes.sql_double_precision:
                    case MySqlTypes.sql_double:
                        return new DotnetTypeDescriptor(typeof(double));
                    case MySqlTypes.sql_double_precision_unsigned:
                    case MySqlTypes.sql_double_unsigned:
                        // there is no unsigned double in C#
                        return new DotnetTypeDescriptor(typeof(double));
                }

                return null;
            });
        }

        /// <summary>
        /// Gets the DateTime to .NET type converter.
        /// </summary>
        /// <returns>The DateTime to .NET type converter.</returns>
        private static SqlTypeToDotnetTypeConverter GetDateTimeToDotnetTypeConverter()
        {
            return new(d =>
            {
                switch (d.BaseTypeName)
                {
                    case MySqlTypes.sql_datetime:
                        return new DotnetTypeDescriptor(typeof(DateTime));
                    case MySqlTypes.sql_timestamp:
                        return new DotnetTypeDescriptor(typeof(DateTimeOffset));
                    case MySqlTypes.sql_time:
                        return new DotnetTypeDescriptor(typeof(TimeOnly));
                    case MySqlTypes.sql_date:
                        return new DotnetTypeDescriptor(typeof(DateOnly));
                    case MySqlTypes.sql_year:
                        return new DotnetTypeDescriptor(typeof(int));
                }

                return null;
            });
        }

        /// <summary>
        /// Gets the GUID to .NET type converter.
        /// </summary>
        /// <returns>The GUID to .NET type converter.</returns>
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

        /// <summary>
        /// Gets the text to .NET type converter.
        /// </summary>
        /// <returns>The text to .NET type converter.</returns>
        private static SqlTypeToDotnetTypeConverter GetTextToDotnetTypeConverter()
        {
            /*
            MySQL Type   Maximum Length/Count
            -------------------------------------------------------
            CHAR(M)      255 characters
            VARCHAR(M)   65,535 characters (or 16,383 if M > 16,383)
            LONGTEXT     (2^{32} - 1) bytes (approximately 4 GB)
            TINYTEXT     255 bytes
            MEDIUMTEXT   (2^{24} - 1) bytes (approximately 16 MB)
            TEXT         (2^{16} - 1) bytes (65,535 bytes)
            ENUM         65,535 enumeration values, each up to 255 characters
            SET          64 set members, each up to 255 characters
            */
            return new(d =>
            {
                if (
                    (
                        d.BaseTypeName == MySqlTypes.sql_char
                        || d.BaseTypeName == MySqlTypes.sql_varchar
                    )
                    && d.Length == 36
                )
                {
                    return new DotnetTypeDescriptor(typeof(Guid));
                }

                switch (d.BaseTypeName)
                {
                    case MySqlTypes.sql_char:
                        return new DotnetTypeDescriptor(
                            typeof(string),
                            d.Length,
                            isUnicode: d.IsUnicode.GetValueOrDefault(true),
                            isFixedLength: true
                        );
                    case MySqlTypes.sql_varchar:
                    case MySqlTypes.sql_tinytext:
                    case MySqlTypes.sql_mediumtext:
                    case MySqlTypes.sql_text:
                    case MySqlTypes.sql_long_varchar:
                    case MySqlTypes.sql_longtext:
                    case MySqlTypes.sql_enum:
                    case MySqlTypes.sql_set:
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

        /// <summary>
        /// Gets the JSON to .NET type converter.
        /// </summary>
        /// <returns>The JSON to .NET type converter.</returns>
        private static SqlTypeToDotnetTypeConverter GetJsonToDotnetTypeConverter()
        {
            return new(d => new DotnetTypeDescriptor(typeof(JsonDocument)));
        }

        /// <summary>
        /// Gets the byte array to .NET type converter.
        /// </summary>
        /// <returns>The byte array to .NET type converter.</returns>
        private static SqlTypeToDotnetTypeConverter GetByteArrayToDotnetTypeConverter()
        {
            return new(d =>
            {
                if (d.BaseTypeName == MySqlTypes.sql_binary && d.Length.HasValue)
                {
                    return new DotnetTypeDescriptor(typeof(byte[]), d.Length, isFixedLength: true);
                }

                switch (d.BaseTypeName)
                {
                    case MySqlTypes.sql_binary:
                    case MySqlTypes.sql_varbinary:
                    case MySqlTypes.sql_long_varbinary:
                    case MySqlTypes.sql_tinyblob:
                    case MySqlTypes.sql_blob:
                    case MySqlTypes.sql_mediumblob:
                    case MySqlTypes.sql_longblob:
                        return new DotnetTypeDescriptor(
                            typeof(byte[]),
                            d.Length,
                            isUnicode: true,
                            isFixedLength: d.IsFixedLength.GetValueOrDefault(false)
                        );
                }

                return null;
            });
        }

        /// <summary>
        /// Gets the geometric to .NET type converter.
        /// </summary>
        /// <returns>The geometric to .NET type converter.</returns>
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
            var sqlMySqlDataGeometryType = Type.GetType(
                "MySql.Data.Types.MySqlGeometry, MySql.Data",
                false,
                false
            );
            var sqlMySqlConnectorGeometryType = Type.GetType(
                "MySqlConnector.MySqlGeometry, MySqlConnector",
                false,
                false
            );

            return new(d =>
            {
                switch (d.BaseTypeName)
                {
                    case MySqlTypes.sql_geometry:
                        if (sqlNetTopologyGeometryType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyGeometryType);
                        }

                        if (sqlMySqlDataGeometryType != null)
                        {
                            return new DotnetTypeDescriptor(sqlMySqlDataGeometryType);
                        }

                        if (sqlMySqlConnectorGeometryType != null)
                        {
                            return new DotnetTypeDescriptor(sqlMySqlConnectorGeometryType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_point:
                        if (sqlNetTopologyPointType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyPointType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_linestring:
                        if (sqlNetTopologyLineStringType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyLineStringType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_polygon:
                        if (sqlNetTopologyPolygonType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyPolygonType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_multipoint:
                        if (sqlNetTopologyMultiPointType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyMultiPointType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_multilinestring:
                        if (sqlNetTopologyMultLineStringType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyMultLineStringType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_multipolygon:
                        if (sqlNetTopologyMultiPolygonType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyMultiPolygonType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                    case MySqlTypes.sql_geomcollection:
                    case MySqlTypes.sql_geometrycollection:
                        if (sqlNetTopologyGeometryCollectionType != null)
                        {
                            return new DotnetTypeDescriptor(sqlNetTopologyGeometryCollectionType);
                        }

                        return new DotnetTypeDescriptor(typeof(object));
                }

                return null;
            });
        }

        #endregion // SqlTypeToDotnetTypeConverters
    }
}
