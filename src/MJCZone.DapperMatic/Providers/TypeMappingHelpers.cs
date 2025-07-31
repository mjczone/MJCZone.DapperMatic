using MJCZone.DapperMatic.Converters;

namespace MJCZone.DapperMatic.Providers;

/// <summary>
/// Provides common helper methods for type mapping operations across all database providers.
/// </summary>
public static class TypeMappingHelpers
{
    /// <summary>
    /// Gets the short form assembly qualified name for a type (Type, Assembly).
    /// This is commonly used for geometry type identification.
    /// </summary>
    /// <param name="type">The type to get the short assembly qualified name for.</param>
    /// <returns>The short assembly qualified name in format "FullTypeName, AssemblyName" or null if type is null.</returns>
    public static string? GetAssemblyQualifiedShortName(Type? type)
    {
        var assemblyQualifiedName = type?.AssemblyQualifiedName;
        if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
        {
            return null;
        }

        var assemblyQualifiedNameParts = assemblyQualifiedName.Split(',');
        if (assemblyQualifiedNameParts.Length < 2)
        {
            return assemblyQualifiedName;
        }

        return assemblyQualifiedNameParts[0] + ", " + assemblyQualifiedNameParts[1];
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for decimal/numeric types with consistent precision and scale handling.
    /// </summary>
    /// <param name="sqlType">The base SQL type name (e.g., "decimal", "numeric").</param>
    /// <param name="precision">The precision, or null to use default.</param>
    /// <param name="scale">The scale, or null to use default.</param>
    /// <returns>A SqlTypeDescriptor with properly formatted SQL type name and metadata.</returns>
    public static SqlTypeDescriptor CreateDecimalType(string sqlType, int? precision = null, int? scale = null)
    {
        var actualPrecision = precision ?? TypeMappingDefaults.DefaultDecimalPrecision;
        var actualScale = scale ?? TypeMappingDefaults.DefaultDecimalScale;

        return new SqlTypeDescriptor($"{sqlType}({actualPrecision},{actualScale})")
        {
            Precision = actualPrecision,
            Scale = actualScale,
        };
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for string/text types with consistent length and unicode handling.
    /// </summary>
    /// <param name="sqlType">The base SQL type name (e.g., "varchar", "nvarchar", "char").</param>
    /// <param name="length">The length, or null to use default.</param>
    /// <param name="isUnicode">Whether the type supports unicode characters.</param>
    /// <param name="isFixedLength">Whether the type is fixed-length.</param>
    /// <returns>A SqlTypeDescriptor with properly formatted SQL type name and metadata.</returns>
    public static SqlTypeDescriptor CreateStringType(
        string sqlType,
        int? length = null,
        bool isUnicode = false,
        bool isFixedLength = false)
    {
        var actualLength = length ?? TypeMappingDefaults.DefaultStringLength;

        string sqlTypeName;
        if (actualLength == TypeMappingDefaults.MaxLength)
        {
            sqlTypeName = $"{sqlType}(max)";
        }
        else
        {
            sqlTypeName = $"{sqlType}({actualLength})";
        }

        return new SqlTypeDescriptor(sqlTypeName)
        {
            Length = actualLength == TypeMappingDefaults.MaxLength ? null : actualLength,
            IsUnicode = isUnicode,
            IsFixedLength = isFixedLength,
        };
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for GUID types stored as strings.
    /// </summary>
    /// <param name="sqlType">The base SQL type name (e.g., "varchar", "char").</param>
    /// <param name="isUnicode">Whether the type supports unicode characters.</param>
    /// <param name="isFixedLength">Whether the type is fixed-length (typically true for GUIDs).</param>
    /// <returns>A SqlTypeDescriptor configured for GUID storage.</returns>
    public static SqlTypeDescriptor CreateGuidStringType(
        string sqlType,
        bool isUnicode = false,
        bool isFixedLength = true)
    {
        return CreateStringType(sqlType, TypeMappingDefaults.GuidStringLength, isUnicode, isFixedLength);
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for enum types stored as strings.
    /// </summary>
    /// <param name="sqlType">The base SQL type name (e.g., "varchar").</param>
    /// <param name="isUnicode">Whether the type supports unicode characters.</param>
    /// <returns>A SqlTypeDescriptor configured for enum storage.</returns>
    public static SqlTypeDescriptor CreateEnumStringType(string sqlType, bool isUnicode = false)
    {
        return CreateStringType(sqlType, TypeMappingDefaults.DefaultEnumLength, isUnicode, isFixedLength: false);
    }

    /// <summary>
    /// Determines if a type is a NetTopologySuite geometry type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a NetTopologySuite geometry type.</returns>
    public static bool IsNetTopologySuiteGeometryType(Type? type)
    {
        if (type == null)
        {
            return false;
        }

        var shortName = GetAssemblyQualifiedShortName(type);
        return !string.IsNullOrWhiteSpace(shortName) &&
               shortName.Contains("NetTopologySuite.Geometries.", StringComparison.OrdinalIgnoreCase) &&
               shortName.Contains(", NetTopologySuite", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the geometry type name from a NetTopologySuite type.
    /// </summary>
    /// <param name="type">The NetTopologySuite geometry type.</param>
    /// <returns>The geometry type name (e.g., "Point", "LineString", "Polygon") or null if not a geometry type.</returns>
    public static string? GetGeometryTypeName(Type? type)
    {
        if (!IsNetTopologySuiteGeometryType(type))
        {
            return null;
        }

        var shortName = GetAssemblyQualifiedShortName(type);
        if (string.IsNullOrWhiteSpace(shortName))
        {
            return null;
        }

        // Extract type name from "NetTopologySuite.Geometries.Point, NetTopologySuite"
        const string prefix = "NetTopologySuite.Geometries.";
        var startIndex = shortName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
        {
            return null;
        }

        var typeNameStart = startIndex + prefix.Length;
        var commaIndex = shortName.IndexOf(',', typeNameStart);
        if (commaIndex == -1)
        {
            return shortName[typeNameStart..];
        }

        return shortName[typeNameStart..commaIndex];
    }

    /// <summary>
    /// Creates a simple SqlTypeDescriptor for basic types without additional metadata.
    /// </summary>
    /// <param name="sqlType">The SQL type name.</param>
    /// <returns>A SqlTypeDescriptor for the basic type.</returns>
    public static SqlTypeDescriptor CreateSimpleType(string sqlType)
    {
        return new SqlTypeDescriptor(sqlType);
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for datetime types with optional precision.
    /// </summary>
    /// <param name="sqlType">The base SQL type name (e.g., "datetime", "timestamp").</param>
    /// <param name="precision">The precision for fractional seconds, or null for no precision.</param>
    /// <returns>A SqlTypeDescriptor with properly formatted SQL type name and metadata.</returns>
    public static SqlTypeDescriptor CreateDateTimeType(string sqlType, int? precision = null)
    {
        if (precision.HasValue)
        {
            return new SqlTypeDescriptor($"{sqlType}({precision})")
            {
                Precision = precision,
            };
        }

        return new SqlTypeDescriptor(sqlType);
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for binary types with length specification.
    /// </summary>
    /// <param name="sqlType">The base SQL type name (e.g., "binary", "varbinary").</param>
    /// <param name="length">The length, or null to use default.</param>
    /// <param name="isFixedLength">Whether the type is fixed-length.</param>
    /// <returns>A SqlTypeDescriptor configured for binary storage.</returns>
    public static SqlTypeDescriptor CreateBinaryType(
        string sqlType,
        int? length = null,
        bool isFixedLength = false)
    {
        if (length == TypeMappingDefaults.MaxLength)
        {
            return new SqlTypeDescriptor($"{sqlType}(max)")
            {
                Length = null,
                IsFixedLength = isFixedLength,
            };
        }

        if (length.HasValue)
        {
            return new SqlTypeDescriptor($"{sqlType}({length})")
            {
                Length = length,
                IsFixedLength = isFixedLength,
            };
        }

        // Default binary types don't typically have length specification
        return new SqlTypeDescriptor(sqlType)
        {
            IsFixedLength = isFixedLength,
        };
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for JSON types, handling provider-specific variations.
    /// </summary>
    /// <param name="sqlType">The SQL type for JSON storage (e.g., "json", "jsonb", "nvarchar(max)").</param>
    /// <param name="isText">Whether this is stored as text (true) or native JSON (false).</param>
    /// <returns>A SqlTypeDescriptor configured for JSON storage.</returns>
    public static SqlTypeDescriptor CreateJsonType(string sqlType, bool isText = false)
    {
        var descriptor = new SqlTypeDescriptor(sqlType);

        if (isText)
        {
            // JSON stored as text typically supports max length
            descriptor.Length = TypeMappingDefaults.MaxLength;
        }

        return descriptor;
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for geometry types based on geometry type name.
    /// </summary>
    /// <param name="sqlType">The SQL type for geometry storage (e.g., "geometry", "geography").</param>
    /// <param name="geometryTypeName">The specific geometry type name (e.g., "Point", "Polygon").</param>
    /// <returns>A SqlTypeDescriptor configured for geometry storage.</returns>
    public static SqlTypeDescriptor CreateGeometryType(string sqlType, string? geometryTypeName = null)
    {
        var descriptor = new SqlTypeDescriptor(sqlType);

        if (!string.IsNullOrWhiteSpace(geometryTypeName))
        {
            // Some providers might use type-specific SQL syntax
            descriptor.SqlTypeName = $"{sqlType}";
        }

        return descriptor;
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for large object (LOB) types like text/blob.
    /// </summary>
    /// <param name="sqlType">The SQL type for LOB storage (e.g., "text", "blob", "longtext").</param>
    /// <param name="isUnicode">Whether the LOB supports unicode characters (for text LOBs).</param>
    /// <returns>A SqlTypeDescriptor configured for LOB storage.</returns>
    public static SqlTypeDescriptor CreateLobType(string sqlType, bool isUnicode = false)
    {
        return new SqlTypeDescriptor(sqlType)
        {
            Length = TypeMappingDefaults.MaxLength,
            IsUnicode = isUnicode,
        };
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor for array types with element type information.
    /// </summary>
    /// <param name="sqlType">The SQL type for array storage (e.g., "integer[]", "text[]").</param>
    /// <param name="elementTypeName">The name of the element type (e.g., "integer", "text").</param>
    /// <returns>A SqlTypeDescriptor configured for array storage.</returns>
    public static SqlTypeDescriptor CreateArrayType(string sqlType, string? elementTypeName = null)
    {
        var descriptor = new SqlTypeDescriptor(sqlType);

        // Store element type information for providers that need it
        if (!string.IsNullOrWhiteSpace(elementTypeName))
        {
            // This could be used by providers to understand the array element type
            descriptor.SqlTypeName = sqlType;
        }

        return descriptor;
    }

    /// <summary>
    /// Creates a SqlTypeDescriptor with precision-based length (for types like TIME, TIMESTAMP).
    /// </summary>
    /// <param name="sqlType">The base SQL type name.</param>
    /// <param name="precision">The precision value.</param>
    /// <returns>A SqlTypeDescriptor with precision formatting.</returns>
    public static SqlTypeDescriptor CreatePrecisionType(string sqlType, int precision)
    {
        return new SqlTypeDescriptor($"{sqlType}({precision})")
        {
            Precision = precision,
        };
    }

    /// <summary>
    /// Determines if a .NET type represents an array type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is an array type.</returns>
    public static bool IsArrayType(Type? type)
    {
        return type?.IsArray == true;
    }

    /// <summary>
    /// Gets the element type of an array type.
    /// </summary>
    /// <param name="arrayType">The array type.</param>
    /// <returns>The element type, or null if not an array type.</returns>
    public static Type? GetArrayElementType(Type? arrayType)
    {
        return arrayType?.GetElementType();
    }

    /// <summary>
    /// Determines if a .NET type is a generic collection type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a generic collection.</returns>
    public static bool IsGenericCollectionType(Type? type)
    {
        if (type == null || !type.IsGenericType)
        {
            return false;
        }

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(List<>) ||
               genericTypeDefinition == typeof(IList<>) ||
               genericTypeDefinition == typeof(ICollection<>) ||
               genericTypeDefinition == typeof(IEnumerable<>) ||
               genericTypeDefinition == typeof(HashSet<>) ||
               genericTypeDefinition == typeof(ISet<>);
    }

    /// <summary>
    /// Gets the element type of a generic collection.
    /// </summary>
    /// <param name="collectionType">The collection type.</param>
    /// <returns>The element type, or null if not a supported collection type.</returns>
    public static Type? GetCollectionElementType(Type? collectionType)
    {
        if (!IsGenericCollectionType(collectionType))
        {
            return null;
        }

        return collectionType!.GetGenericArguments().FirstOrDefault();
    }

    /// <summary>
    /// Gets the standard NetTopologySuite geometry types for registration.
    /// This provides a consistent set of geometry types across all providers.
    /// </summary>
    /// <returns>An array of NetTopologySuite geometry types, filtering out null entries.</returns>
    public static Type[] GetStandardGeometryTypes()
    {
        var types = new[]
        {
            Type.GetType("NetTopologySuite.Geometries.Geometry, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.Point, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.LineString, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.Polygon, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.MultiPoint, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.MultiLineString, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.MultiPolygon, NetTopologySuite"),
            Type.GetType("NetTopologySuite.Geometries.GeometryCollection, NetTopologySuite"),
        };

        // Filter out null types (in case NetTopologySuite is not available)
        return types.Where(t => t != null).ToArray()!;
    }

    /// <summary>
    /// Gets additional provider-specific geometry types for SQL Server.
    /// </summary>
    /// <returns>An array of SQL Server specific geometry types, filtering out null entries.</returns>
    public static Type[] GetSqlServerGeometryTypes()
    {
        var types = new[]
        {
            Type.GetType("Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types"),
            Type.GetType("Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types"),
            Type.GetType("Microsoft.SqlServer.Types.SqlHierarchyId, Microsoft.SqlServer.Types"),
        };

        return types.Where(t => t != null).ToArray()!;
    }

    /// <summary>
    /// Gets additional provider-specific geometry types for MySQL.
    /// </summary>
    /// <returns>An array of MySQL specific geometry types, filtering out null entries.</returns>
    public static Type[] GetMySqlGeometryTypes()
    {
        var types = new[]
        {
            Type.GetType("MySql.Data.Types.MySqlGeometry, MySql.Data"),
            Type.GetType("MySqlConnector.MySqlGeometry, MySqlConnector"),
        };

        return types.Where(t => t != null).ToArray()!;
    }

    /// <summary>
    /// Gets additional provider-specific geometry and network types for PostgreSQL.
    /// </summary>
    /// <returns>An array of PostgreSQL specific types, filtering out null entries.</returns>
    public static Type[] GetPostgreSqlSpecialTypes()
    {
        var types = new[]
        {
            // Network types
            Type.GetType("System.Net.NetworkInformation.PhysicalAddress, System.Net.NetworkInformation"),
            Type.GetType("System.Net.IPAddress, System.Net.Primitives"),
            // PostgreSQL specific types
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
            Type.GetType("NpgsqlTypes.NpgsqlTsVector, Npgsql"),
        };

        return types.Where(t => t != null).ToArray()!;
    }

    /// <summary>
    /// Gets the combined geometry types for a specific provider.
    /// </summary>
    /// <param name="provider">The database provider name.</param>
    /// <returns>An array of all geometry types for the specified provider.</returns>
    public static Type[] GetGeometryTypesForProvider(string provider)
    {
        var standardTypes = GetStandardGeometryTypes();

        return provider.ToLowerInvariant() switch
        {
            "sqlserver" => standardTypes.Concat(GetSqlServerGeometryTypes()).ToArray(),
            "mysql" => standardTypes.Concat(GetMySqlGeometryTypes()).ToArray(),
            "postgresql" => standardTypes.Concat(GetPostgreSqlSpecialTypes()).ToArray(),
            "sqlite" => standardTypes, // SQLite only supports NetTopologySuite types
            _ => standardTypes,
        };
    }

    /// <summary>
    /// Gets the standard System.Text.Json types that should be registered for JSON handling.
    /// This provides a consistent set of JSON types across all providers.
    /// </summary>
    /// <returns>An array of System.Text.Json types.</returns>
    public static Type[] GetStandardJsonTypes()
    {
        return new[]
        {
            typeof(System.Text.Json.JsonDocument),
            typeof(System.Text.Json.JsonElement),
            typeof(System.Text.Json.Nodes.JsonArray),
            typeof(System.Text.Json.Nodes.JsonNode),
            typeof(System.Text.Json.Nodes.JsonObject),
            typeof(System.Text.Json.Nodes.JsonValue),
        };
    }

    /// <summary>
    /// Creates a standardized JSON type converter for a specific provider.
    /// This handles the different JSON storage strategies across providers.
    /// Note: This method should be used by individual providers who can provide their specific type constants.
    /// </summary>
    /// <param name="provider">The database provider name.</param>
    /// <returns>A DotnetTypeToSqlTypeConverter configured for the provider's JSON strategy.</returns>
    public static DotnetTypeToSqlTypeConverter CreateJsonConverter(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "mysql" => new DotnetTypeToSqlTypeConverter(d => CreateJsonType("json", isText: false)),
            "postgresql" => new DotnetTypeToSqlTypeConverter(d => CreateJsonType("jsonb", isText: false)),
            "sqlserver" => new DotnetTypeToSqlTypeConverter(d =>
            {
                var sqlType = d.IsUnicode == true ? "nvarchar(max)" : "varchar(max)";
                return CreateJsonType(sqlType, isText: true);
            }),
            "sqlite" => new DotnetTypeToSqlTypeConverter(d => CreateJsonType("text", isText: true)),
            _ => new DotnetTypeToSqlTypeConverter(d => CreateJsonType("text", isText: true)) // Default to text storage
        };
    }

    /// <summary>
    /// Creates an enhanced JSON type descriptor with provider-specific optimizations.
    /// </summary>
    /// <param name="provider">The database provider name.</param>
    /// <param name="isUnicode">Whether to use Unicode storage for text-based JSON (relevant for SQL Server/SQLite).</param>
    /// <returns>A SqlTypeDescriptor optimized for the provider's JSON capabilities.</returns>
    public static SqlTypeDescriptor CreateProviderOptimizedJsonType(string provider, bool isUnicode = false)
    {
        return provider.ToLowerInvariant() switch
        {
            "mysql" => CreateJsonType("json", isText: false),
            "postgresql" => CreateJsonType("jsonb", isText: false), // jsonb is preferred over json in PostgreSQL
            "sqlserver" => CreateJsonType(isUnicode ? "nvarchar(max)" : "varchar(max)", isText: true),
            "sqlite" => CreateJsonType("text", isText: true),
            _ => CreateJsonType("text", isText: true)
        };
    }

    /// <summary>
    /// Creates a native array type descriptor for PostgreSQL.
    /// </summary>
    /// <param name="elementSqlType">The SQL type of the array element (e.g., "integer", "text").</param>
    /// <returns>A SqlTypeDescriptor configured for PostgreSQL native array storage.</returns>
    public static SqlTypeDescriptor CreateNativeArrayType(string elementSqlType)
    {
        return new SqlTypeDescriptor($"{elementSqlType}[]");
    }

    /// <summary>
    /// Creates a standardized array converter that uses native arrays for PostgreSQL
    /// and falls back to JSON for other providers.
    /// </summary>
    /// <param name="provider">The database provider name.</param>
    /// <returns>A DotnetTypeToSqlTypeConverter configured for the provider's array strategy.</returns>
    public static DotnetTypeToSqlTypeConverter CreateArrayConverter(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "postgresql" => new DotnetTypeToSqlTypeConverter(d =>
            {
                if (d.DotnetType?.IsArray == true)
                {
                    var elementType = d.DotnetType.GetElementType();
                    var arrayTypeName = GetPostgreSqlArrayTypeName(elementType);
                    if (arrayTypeName != null)
                    {
                        return CreateNativeArrayType(arrayTypeName);
                    }
                    // Fall back to JSON for unsupported array types
                    return CreateJsonType("jsonb", isText: false);
                }
                return null;
            }),
            // Other providers fall back to JSON
            _ => CreateJsonConverter(provider)
        };
    }

    /// <summary>
    /// Maps .NET array element types to PostgreSQL array type names.
    /// </summary>
    /// <param name="elementType">The .NET element type.</param>
    /// <returns>The PostgreSQL array type name, or null if not supported.</returns>
    public static string? GetPostgreSqlArrayTypeName(Type? elementType)
    {
        if (elementType == null)
        {
            return null;
        }

        return elementType switch
        {
            Type t when t == typeof(bool) => "boolean",
            Type t when t == typeof(short) => "smallint",
            Type t when t == typeof(int) => "integer",
            Type t when t == typeof(long) => "bigint",
            Type t when t == typeof(float) => "real",
            Type t when t == typeof(double) => "double precision",
            Type t when t == typeof(decimal) => "numeric",
            Type t when t == typeof(string) => "text",
            Type t when t == typeof(char) => "char",
            Type t when t == typeof(byte[]) => "bytea",
            Type t when t == typeof(DateTime) => "timestamp",
            Type t when t == typeof(DateTimeOffset) => "timestamptz",
            Type t when t == typeof(TimeSpan) => "interval",
            Type t when t == typeof(DateOnly) => "date",
            Type t when t == typeof(TimeOnly) => "time",
            Type t when t == typeof(Guid) => "uuid",
            _ => null // Unsupported type
        };
    }

    /// <summary>
    /// Determines if a provider supports native array types.
    /// </summary>
    /// <param name="provider">The database provider name.</param>
    /// <returns>True if the provider supports native arrays.</returns>
    public static bool SupportsNativeArrays(string provider)
    {
        return string.Equals(provider, "postgresql", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the standard PostgreSQL array type names that should be registered for SQL-to-.NET type mapping.
    /// This provides array versions of common PostgreSQL types.
    /// </summary>
    /// <returns>An array of PostgreSQL array type names.</returns>
    public static string[] GetPostgreSqlStandardArrayTypes()
    {
        return new[]
        {
            // Standard array notation
            "boolean[]",
            "smallint[]",
            "integer[]",
            "bigint[]",
            "real[]",
            "double precision[]",
            "numeric[]",
            "text[]",
            "char[]",
            "varchar[]",
            "character varying[]",
            "character[]",
            "bytea[]",
            "timestamp[]",
            "timestamp without time zone[]",
            "timestamp with time zone[]",
            "timestamptz[]",
            "date[]",
            "time[]",
            "time without time zone[]",
            "time with time zone[]",
            "timetz[]",
            "interval[]",
            "uuid[]",
            "json[]",
            "jsonb[]",
            // PostgreSQL internal array notation (with underscore prefix)
            "_bool",
            "_int2",
            "_int4",
            "_int8",
            "_float4",
            "_float8",
            "_numeric",
            "_text",
            "_char",
            "_varchar",
            "_bpchar",
            "_bytea",
            "_timestamp",
            "_timestamptz",
            "_date",
            "_time",
            "_timetz",
            "_interval",
            "_uuid",
            "_json",
            "_jsonb",
        };
    }

    /// <summary>
    /// Creates a SQL-to-.NET array type converter for PostgreSQL that converts array types to their .NET array equivalents.
    /// </summary>
    /// <returns>A SqlTypeToDotnetTypeConverter configured for PostgreSQL array types.</returns>
    public static SqlTypeToDotnetTypeConverter CreatePostgreSqlArrayTypeConverter()
    {
        return new SqlTypeToDotnetTypeConverter(d =>
        {
            if (string.IsNullOrWhiteSpace(d.SqlTypeName))
            {
                return null;
            }

            string elementTypeName;

            // Check if this is an array type (ends with [] or starts with _)
            if (d.SqlTypeName.EndsWith("[]", StringComparison.Ordinal))
            {
                // Standard array notation: "text[]"
                elementTypeName = d.SqlTypeName[..^2]; // Remove the "[]" suffix
            }
            else if (d.SqlTypeName.StartsWith('_'))
            {
                // PostgreSQL internal array notation: "_text"
                elementTypeName = d.SqlTypeName[1..]; // Remove the "_" prefix
            }
            else
            {
                return null; // Not an array type
            }

            // Map element type to .NET array type
            return elementTypeName switch
            {
                "boolean" or "bool" => new DotnetTypeDescriptor(typeof(bool[])),
                "smallint" or "int2" => new DotnetTypeDescriptor(typeof(short[])),
                "integer" or "int4" => new DotnetTypeDescriptor(typeof(int[])),
                "bigint" or "int8" => new DotnetTypeDescriptor(typeof(long[])),
                "real" or "float4" => new DotnetTypeDescriptor(typeof(float[])),
                "double precision" or "float8" => new DotnetTypeDescriptor(typeof(double[])),
                "numeric" => new DotnetTypeDescriptor(typeof(decimal[])),
                "text" => new DotnetTypeDescriptor(typeof(string[])),
                "char" or "bpchar" => new DotnetTypeDescriptor(typeof(char[])),
                "varchar" => new DotnetTypeDescriptor(typeof(string[])),
                "character varying" => new DotnetTypeDescriptor(typeof(string[])),
                "character" => new DotnetTypeDescriptor(typeof(char[])),
                "bytea" => new DotnetTypeDescriptor(typeof(byte[][])),
                "timestamp" => new DotnetTypeDescriptor(typeof(DateTime[])),
                "timestamp without time zone" => new DotnetTypeDescriptor(typeof(DateTime[])),
                "timestamp with time zone" or "timestamptz" => new DotnetTypeDescriptor(typeof(DateTimeOffset[])),
                "date" => new DotnetTypeDescriptor(typeof(DateOnly[])),
                "time" => new DotnetTypeDescriptor(typeof(TimeOnly[])),
                "time without time zone" => new DotnetTypeDescriptor(typeof(TimeOnly[])),
                "time with time zone" or "timetz" => new DotnetTypeDescriptor(typeof(TimeOnly[])),
                "interval" => new DotnetTypeDescriptor(typeof(TimeSpan[])),
                "uuid" => new DotnetTypeDescriptor(typeof(Guid[])),
                "json" => new DotnetTypeDescriptor(typeof(System.Text.Json.JsonDocument[])),
                "jsonb" => new DotnetTypeDescriptor(typeof(System.Text.Json.JsonDocument[])),
                _ => null // Unsupported array element type
            };
        });
    }
}