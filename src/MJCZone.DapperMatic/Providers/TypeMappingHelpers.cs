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
}