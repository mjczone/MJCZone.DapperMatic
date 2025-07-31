using MJCZone.DapperMatic.Converters;

namespace MJCZone.DapperMatic.Providers.Base;

/// <summary>
/// Base class providing standard type mapping implementations shared across database providers.
/// This class extracts common conversion logic to reduce code duplication.
/// </summary>
/// <typeparam name="TImpl">The concrete provider type map implementation.</typeparam>
public abstract class StandardTypeMapBase<TImpl> : DbProviderTypeMapBase<TImpl>
    where TImpl : IDbProviderTypeMap
{
    /// <summary>
    /// Gets the provider-specific type mapping configuration.
    /// </summary>
    /// <returns>Provider-specific type mapping configuration.</returns>
    protected abstract IProviderTypeMapping GetProviderTypeMapping();

    /// <summary>
    /// Gets the provider name for use with helper methods.
    /// </summary>
    /// <returns>The provider name (e.g., "sqlserver", "postgresql").</returns>
    protected abstract string GetProviderName();

    #region Standard Converter Implementations

    /// <summary>
    /// Gets the boolean to SQL type converter using provider-specific boolean type.
    /// </summary>
    /// <returns>Boolean to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetBooleanToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return TypeMappingHelpers.CreateSimpleType(mapping.BooleanType);
        });
    }

    /// <summary>
    /// Gets the GUID to SQL type converter using provider-specific GUID handling.
    /// </summary>
    /// <returns>GUID to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetGuidToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return mapping.CreateGuidType();
        });
    }

    /// <summary>
    /// Gets the enum to SQL type converter using provider-specific string type.
    /// </summary>
    /// <returns>Enum to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetEnumToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return TypeMappingHelpers.CreateEnumStringType(mapping.EnumStringType, mapping.IsUnicodeProvider);
        });
    }

    /// <summary>
    /// Gets the numeric to SQL type converter using provider-specific numeric type mappings.
    /// </summary>
    /// <returns>Numeric to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetNumericToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            if (mapping.NumericTypeMap.TryGetValue(d.DotnetType, out var sqlType))
            {
                if (d.DotnetType == typeof(decimal))
                {
                    return TypeMappingHelpers.CreateDecimalType(sqlType, d.Precision, d.Scale);
                }
                return TypeMappingHelpers.CreateSimpleType(sqlType);
            }
            // Default fallback to int type
            return TypeMappingHelpers.CreateSimpleType(mapping.NumericTypeMap[typeof(int)]);
        });
    }

    /// <summary>
    /// Gets the JSON to SQL type converter using standardized JSON handling.
    /// </summary>
    /// <returns>JSON to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetJsonToSqlTypeConverter()
    {
        return TypeMappingHelpers.CreateJsonConverter(GetProviderName());
    }

    /// <summary>
    /// Gets the array to SQL type converter with native array support where available.
    /// </summary>
    /// <returns>Array to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetArrayToSqlTypeConverter()
    {
        return TypeMappingHelpers.CreateArrayConverter(GetProviderName());
    }

    /// <summary>
    /// Gets the enumerable to SQL type converter, typically using JSON serialization.
    /// </summary>
    /// <returns>Enumerable to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetEnumerableToSqlTypeConverter()
    {
        return GetJsonToSqlTypeConverter();
    }

    /// <summary>
    /// Gets the POCO to SQL type converter, typically using JSON serialization.
    /// </summary>
    /// <returns>POCO to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetPocoToSqlTypeConverter()
    {
        return GetJsonToSqlTypeConverter();
    }

    /// <summary>
    /// Gets the object to SQL type converter using provider-specific object handling.
    /// </summary>
    /// <returns>Object to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetObjectToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return mapping.CreateObjectType();
        });
    }

    /// <summary>
    /// Gets the text to SQL type converter using provider-specific text handling.
    /// </summary>
    /// <returns>Text to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetTextToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return mapping.CreateTextType(d);
        });
    }

    /// <summary>
    /// Gets the DateTime to SQL type converter using provider-specific DateTime handling.
    /// </summary>
    /// <returns>DateTime to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetDateTimeToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return mapping.CreateDateTimeType(d);
        });
    }

    /// <summary>
    /// Gets the byte array to SQL type converter using provider-specific binary handling.
    /// </summary>
    /// <returns>Byte array to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetByteArrayToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return mapping.CreateBinaryType(d);
        });
    }

    /// <summary>
    /// Gets the XML to SQL type converter using provider-specific XML handling.
    /// </summary>
    /// <returns>XML to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetXmlToSqlTypeConverter()
    {
        var mapping = GetProviderTypeMapping();
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            return mapping.CreateXmlType();
        });
    }

    /// <summary>
    /// Gets the geometric to SQL type converter using standardized geometry handling.
    /// </summary>
    /// <returns>Geometric to SQL type converter.</returns>
    protected virtual DotnetTypeToSqlTypeConverter GetGeometricToSqlTypeConverter()
    {
        return new DotnetTypeToSqlTypeConverter(d =>
        {
            var shortName = TypeMappingHelpers.GetAssemblyQualifiedShortName(d.DotnetType);
            if (string.IsNullOrWhiteSpace(shortName))
            {
                return null;
            }

            return CreateGeometryTypeForShortName(shortName);
        });
    }

    /// <summary>
    /// Creates a geometry type descriptor for the given assembly-qualified short name.
    /// This method should be overridden by providers that support geometry types.
    /// </summary>
    /// <param name="shortName">The assembly-qualified short name of the geometry type.</param>
    /// <returns>SQL type descriptor for geometry storage, or null if not supported.</returns>
    protected virtual SqlTypeDescriptor? CreateGeometryTypeForShortName(string shortName)
    {
        // Default implementation returns null (no geometry support)
        return null;
    }

    #endregion

    #region Standard Registration Implementation

    /// <summary>
    /// Registers standard .NET types to SQL type converters using common patterns.
    /// This method provides the shared registration logic that all providers can inherit.
    /// </summary>
    protected virtual void RegisterStandardDotnetTypeToSqlTypeConverters()
    {
        var booleanConverter = GetBooleanToSqlTypeConverter();
        var numericConverter = GetNumericToSqlTypeConverter();
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
        RegisterConverterForTypes(numericConverter, GetStandardNumericTypes());

        // Guid affinity
        RegisterConverter<Guid>(guidConverter);

        // Text affinity
        RegisterConverterForTypes(textConverter, GetStandardTextTypes());

        // Xml affinity
        RegisterConverterForTypes(xmlConverter, typeof(System.Xml.Linq.XDocument), typeof(System.Xml.Linq.XElement));

        // Json affinity
        RegisterConverterForTypes(jsonConverter, TypeMappingHelpers.GetStandardJsonTypes());

        // DateTime affinity
        RegisterConverterForTypes(dateTimeConverter, GetStandardDateTimeTypes());

        // Binary affinity
        RegisterConverterForTypes(byteArrayConverter, GetStandardBinaryTypes());

        // Object affinity
        RegisterConverter<object>(objectConverter);

        // Enumerable affinity
        RegisterConverterForTypes(enumerableConverter, GetStandardEnumerableTypes());

        // Enums (uses a placeholder to easily locate it)
        RegisterConverter<InternalEnumTypePlaceholder>(enumConverter);

        // Arrays (uses a placeholder to easily locate it)
        RegisterConverter<InternalArrayTypePlaceholder>(arrayConverter);

        // Poco (uses a placeholder to easily locate it)
        RegisterConverter<InternalPocoTypePlaceholder>(pocoConverter);

        // Geometry types (support provider-specific geometry types)
        var geometryTypes = GetProviderTypeMapping().GetSupportedGeometryTypes();
        if (geometryTypes.Length > 0)
        {
            RegisterConverterForTypes(geometricConverter, geometryTypes);
        }

        // Allow providers to register additional converters
        RegisterProviderSpecificConverters();
    }

    /// <summary>
    /// Allows providers to register additional provider-specific converters.
    /// Override this method to add converters that don't fit the standard patterns.
    /// </summary>
    protected virtual void RegisterProviderSpecificConverters()
    {
        // Default implementation does nothing
    }

    #endregion

    #region Standard Type Registration

    /// <summary>
    /// Gets the standard numeric types that should be registered for numeric conversion.
    /// </summary>
    /// <returns>Array of numeric types.</returns>
    protected static Type[] GetStandardNumericTypes()
    {
        return new[]
        {
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(System.Numerics.BigInteger),
            typeof(long),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(decimal),
            typeof(float),
            typeof(double),
        };
    }

    /// <summary>
    /// Gets the standard text types that should be registered for text conversion.
    /// </summary>
    /// <returns>Array of text types.</returns>
    protected static Type[] GetStandardTextTypes()
    {
        return new[]
        {
            typeof(string),
            typeof(char),
            typeof(char[]),
            typeof(MemoryStream),
            typeof(ReadOnlyMemory<byte>[]),
            typeof(Stream),
            typeof(TextReader),
        };
    }

    /// <summary>
    /// Gets the standard DateTime types that should be registered for DateTime conversion.
    /// </summary>
    /// <returns>Array of DateTime types.</returns>
    protected static Type[] GetStandardDateTimeTypes()
    {
        return new[]
        {
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateOnly),
            typeof(TimeOnly),
        };
    }

    /// <summary>
    /// Gets the standard binary types that should be registered for binary conversion.
    /// </summary>
    /// <returns>Array of binary types.</returns>
    protected static Type[] GetStandardBinaryTypes()
    {
        return new[]
        {
            typeof(byte[]),
            typeof(ReadOnlyMemory<byte>),
            typeof(Memory<byte>),
            typeof(Stream),
            typeof(BinaryReader),
            typeof(System.Collections.BitArray),
            typeof(System.Collections.Specialized.BitVector32),
        };
    }

    /// <summary>
    /// Gets the standard enumerable types that should be registered for enumerable conversion.
    /// </summary>
    /// <returns>Array of enumerable types.</returns>
    protected static Type[] GetStandardEnumerableTypes()
    {
        return new[]
        {
            typeof(System.Collections.Immutable.ImmutableDictionary<string, string>),
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
            typeof(System.Collections.ObjectModel.Collection<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlySet<>),
            typeof(ICollection<>),
            typeof(IEnumerable<>),
        };
    }

    #endregion
}