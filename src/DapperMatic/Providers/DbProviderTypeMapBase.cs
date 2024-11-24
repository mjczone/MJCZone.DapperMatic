using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace DapperMatic.Providers;

// Add RegisterSqlTypeToDotnetTypeDescriptorConverter method to allow for custom type mappings
// Add RegisterDotnetTypeDescriptorToSqlTypeConverter method to allow for custom type mappings
/// <summary>
/// Manages mappings between .NET types and database types.
/// </summary>
/// <remarks>
/// It's important that this class remaing a generic class so that the static members are not shared between
/// different implementations of the class. This is because the static members are used to store mappings
/// between types and their corresponding SQL types. If the static members were shared between different
/// implementations, then the mappings would be shared between different implementations, which would cause
/// unexpected behavior.
///
/// Database type mappings are tricky because different databases have different types, and .NET types can
/// be mapped to different database types depending on the desired length, precision, and scale of the type,
/// whether the type is nullable, fixed length, auto-incrementing, etc. This class is designed
/// to provide a way to map .NET types to database types in a way that is flexible and extensible.
/// </remarks>
/// <typeparam name="TImpl"></typeparam>

public abstract partial class DbProviderTypeMapBase<TImpl> : IDbProviderTypeMap
    where TImpl : IDbProviderTypeMap
{
    protected static readonly ConcurrentDictionary<
        Type,
        List<DotnetTypeToSqlTypeConverter>
    > DotnetTypeToSqlTypeConverters = new();
    protected static readonly ConcurrentDictionary<
        string,
        List<SqlTypeToDotnetTypeConverter>
    > SqlTypeToDotnetTypeConverters = new();

    // protected static readonly ConcurrentDictionary<
    //     string,
    //     DbProviderSqlType
    // > ProviderSqlTypeLookup = new();

    protected DbProviderTypeMapBase()
    {
        if (DotnetTypeToSqlTypeConverters.IsEmpty)
            RegisterDotnetTypeToSqlTypeConverters();
        if (SqlTypeToDotnetTypeConverters.IsEmpty)
            RegisterSqlTypeToDotnetTypeConverters();
    }

    protected abstract void RegisterDotnetTypeToSqlTypeConverters();
    protected abstract void RegisterSqlTypeToDotnetTypeConverters();

    protected SqlTypeDescriptor GetSqlTypeDescriptor(string fullSqlType)
    {
        return new SqlTypeDescriptor(fullSqlType);
    }

    protected DotnetTypeDescriptor GetDotnetTypeDescriptor(Type type)
    {
        return new DotnetTypeDescriptor(type);
    }

    public bool TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
        string fullSqlType,
        out DotnetTypeDescriptor? dotnetTypeDescriptor
    )
    {
        var sqlTypeDescriptor = GetSqlTypeDescriptor(fullSqlType);
        return TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
            sqlTypeDescriptor,
            out dotnetTypeDescriptor
        );
    }

    public bool TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
        SqlTypeDescriptor sqlTypeDescriptor,
        out DotnetTypeDescriptor? dotnetTypeDescriptor
    )
    {
        if (
            !SqlTypeToDotnetTypeConverters.TryGetValue(
                sqlTypeDescriptor.BaseTypeName,
                out var converters
            )
            || converters == null
        )
        {
            dotnetTypeDescriptor = null;
            return false;
        }

        foreach (var converter in converters)
        {
            if (converter.TryConvert(sqlTypeDescriptor, out var rdt))
            {
                if (rdt != null)
                {
                    dotnetTypeDescriptor = rdt;
                    return true;
                }
            }
        }

        dotnetTypeDescriptor = null;
        return false;
    }

    public bool TryGetProviderSqlTypeMatchingDotnetType(
        Type type,
        out SqlTypeDescriptor? sqlTypeDescriptor
    )
    {
        var dotnetTypeDescriptor = GetDotnetTypeDescriptor(type);
        return TryGetProviderSqlTypeMatchingDotnetType(dotnetTypeDescriptor, out sqlTypeDescriptor);
    }

    public bool TryGetProviderSqlTypeMatchingDotnetType(
        DotnetTypeDescriptor dotnetTypeDescriptor,
        out SqlTypeDescriptor? sqlTypeDescriptor
    )
    {
        if (
            !DotnetTypeToSqlTypeConverters.TryGetValue(
                dotnetTypeDescriptor.DotnetType,
                out var converters
            )
            || converters == null
        )
        {
            // if the type is a generic type, try to find a converter for the generic type definition
            if (dotnetTypeDescriptor.DotnetType.IsGenericType)
            {
                var genericType = dotnetTypeDescriptor.DotnetType.GetGenericTypeDefinition();
                DotnetTypeToSqlTypeConverters.TryGetValue(genericType, out converters);
            }

            // if the type is an enum type, try to find the Enum placeholder type
            if (converters == null && dotnetTypeDescriptor.DotnetType.IsEnum)
            {
                DotnetTypeToSqlTypeConverters.TryGetValue(
                    typeof(InternalEnumTypePlaceholder),
                    out converters
                );
            }

            // if the type is an array type, try to find the Array placeholder type
            if (converters == null && dotnetTypeDescriptor.DotnetType.IsArray)
            {
                DotnetTypeToSqlTypeConverters.TryGetValue(
                    typeof(InternalArrayTypePlaceholder),
                    out converters
                );
            }

            // if the type is an poco type, try first to see if there's a registration
            // for a type that can be cast to from this type, otherwise
            // look for the Poco placeholder type
            if (
                converters == null
                && (
                    dotnetTypeDescriptor.DotnetType.IsClass
                    || dotnetTypeDescriptor.DotnetType.IsInterface
                )
            )
            {
                foreach (var registeredType in DotnetTypeToSqlTypeConverters.Keys)
                {
                    if (
                        registeredType == typeof(object)
                        || typeof(IEnumerable).IsAssignableFrom(registeredType)
                        || (!registeredType.IsClass && !registeredType.IsInterface)
                    )
                        continue;

                    if (registeredType.IsAssignableFrom(dotnetTypeDescriptor.DotnetType))
                    {
                        DotnetTypeToSqlTypeConverters.TryGetValue(registeredType, out converters);
                    }
                }

                if (converters == null)
                {
                    DotnetTypeToSqlTypeConverters.TryGetValue(
                        typeof(InternalPocoTypePlaceholder),
                        out converters
                    );
                }
            }
        }

        if (converters == null || converters.Count == 0)
        {
            sqlTypeDescriptor = null;
            return false;
        }

        foreach (var converter in converters)
        {
            if (converter.TryConvert(dotnetTypeDescriptor, out var std))
            {
                if (std != null)
                {
                    sqlTypeDescriptor = std;
                    return true;
                }
            }
        }

        sqlTypeDescriptor = null;
        return false;
    }
}

public abstract partial class DbProviderTypeMapBase<TImpl> : IDbProviderTypeMap
    where TImpl : IDbProviderTypeMap
{
    /// <summary>
    /// Provides a way to extend the type mapping for a given .NET type to a SQL type.
    /// </summary>
    /// <param name="type">The .NET type to convert to a SQL type.</param>
    /// <param name="converter">The converter to register</param>
    /// <returns>self for a fluent api</returns>
    public static void RegisterConverter(
        Type type,
        DotnetTypeToSqlTypeConverter converter,
        bool prepend = false
    )
    {
        if (converter == null)
            return;

        if (!DotnetTypeToSqlTypeConverters.TryGetValue(type, out var converters))
        {
            converters = [];
            DotnetTypeToSqlTypeConverters[type] = converters;
        }

        if (prepend)
            converters.Insert(0, converter);
        else
            converters.Add(converter);
    }

    /// <summary>
    /// Provides a way to extend the type mapping for a given .NET type to a SQL type.
    /// </summary>
    /// <typeparam name="T">The .NET type to convert to a SQL type.</typeparam>
    /// <param name="converter">The converter to register</param>
    /// <returns>self for a fluent api</returns>
    public static void RegisterConverter<T>(DotnetTypeToSqlTypeConverter converter)
    {
        RegisterConverter(typeof(T), converter);
    }

    protected static void RegisterConverterForTypes(
        DotnetTypeToSqlTypeConverter converter,
        params Type?[] types
    )
    {
        foreach (var type in types)
        {
            if (type != null)
            {
                RegisterConverter(type, converter);
            }
        }
    }

    protected static void RegisterConverterForTypes(
        DotnetTypeToSqlTypeConverter converter,
        params string[] clrTypeNames
    )
    {
        foreach (var typeName in clrTypeNames)
        {
            if (Type.GetType(typeName, false, true) is Type type)
                RegisterConverter(type, converter);
        }
    }

    /// <summary>
    /// Provides a way to extend the type mapping for a given SQL type to a .NET type.
    /// </summary>
    /// <param name="baseTypeName">The base type name to convert to a .NET type.</param>
    /// <param name="converter">The converter to register</param>
    /// <returns>self for a fluent api</returns>
    public static void RegisterConverter(
        string baseTypeName,
        SqlTypeToDotnetTypeConverter converter,
        bool prepend = false
    )
    {
        if (converter == null)
            return;

        if (!SqlTypeToDotnetTypeConverters.TryGetValue(baseTypeName, out var converters))
        {
            converters = [];
            SqlTypeToDotnetTypeConverters[baseTypeName] = converters;
        }

        if (prepend)
            converters.Insert(0, converter);
        else
            converters.Add(converter);
    }

    protected static void RegisterConverterForTypes(
        SqlTypeToDotnetTypeConverter converter,
        params string[] baseTypeNames
    )
    {
        foreach (var baseTypeName in baseTypeNames)
            RegisterConverter(baseTypeName, converter);
    }
}

internal class InternalEnumTypePlaceholder { }

internal class InternalArrayTypePlaceholder { }

internal class InternalPocoTypePlaceholder { }
