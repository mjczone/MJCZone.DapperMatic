using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Providers;

public abstract class ProviderTypeMapBase : IProviderTypeMap
{
    public abstract ProviderDataType[] GetProviderDataTypes();

    public virtual ProviderDataType GetRecommendedDataTypeForDotnetType(Type dotnetType)
    {
        var providerDataTypes = GetProviderDataTypes();
        var providerDataType =
            providerDataTypes.FirstOrDefault(x => x.IsRecommendedDotNetTypeMatch(dotnetType))
            ?? providerDataTypes.FirstOrDefault(x => x.PrimaryDotnetType == dotnetType)
            ?? providerDataTypes.FirstOrDefault(x => x.SupportedDotnetTypes.Contains(dotnetType));

        Type? alternateType;

        if (
            providerDataType == null
            && (dotnetType.IsInterface || dotnetType.IsClass)
            && dotnetType.IsGenericType
        )
        {
            var genericTypeDefinition = dotnetType.GetGenericTypeDefinition();
            if (dotnetType.IsInterface && genericTypeDefinition == typeof(IDictionary<,>))
            {
                // see if the Dictionary type version is supported
                alternateType = typeof(Dictionary<,>).MakeGenericType(
                    dotnetType.GetGenericArguments()
                );
                providerDataType =
                    providerDataTypes.FirstOrDefault(x =>
                        x.IsRecommendedDotNetTypeMatch(alternateType)
                    )
                    ?? providerDataTypes.FirstOrDefault(x => x.PrimaryDotnetType == alternateType)
                    ?? providerDataTypes.FirstOrDefault(x =>
                        x.SupportedDotnetTypes.Contains(alternateType)
                    );
            }
            if (
                genericTypeDefinition == typeof(IList<>)
                || genericTypeDefinition == typeof(ICollection<>)
                || genericTypeDefinition == typeof(IEnumerable<>)
                || genericTypeDefinition == typeof(Collection<>)
            )
            {
                // see if the Dictionary type version is supported
                alternateType = typeof(List<>).MakeGenericType(dotnetType.GetGenericArguments());
                providerDataType =
                    providerDataTypes.FirstOrDefault(x =>
                        x.IsRecommendedDotNetTypeMatch(alternateType)
                    )
                    ?? providerDataTypes.FirstOrDefault(x => x.PrimaryDotnetType == alternateType)
                    ?? providerDataTypes.FirstOrDefault(x =>
                        x.SupportedDotnetTypes.Contains(alternateType)
                    );
            }
        }

        if (providerDataType == null && dotnetType.IsClass)
        {
            // because it's a class, let's find the Dictionary<string, object> data type
            alternateType = typeof(Dictionary<string, object>);
            providerDataType =
                providerDataTypes.FirstOrDefault(x => x.IsRecommendedDotNetTypeMatch(alternateType))
                ?? providerDataTypes.FirstOrDefault(x => x.PrimaryDotnetType == alternateType)
                ?? providerDataTypes.FirstOrDefault(x =>
                    x.SupportedDotnetTypes.Contains(alternateType)
                );
        }

        // ReSharper disable once InvertIf
        if (providerDataType == null && dotnetType.IsClass)
        {
            alternateType = typeof(object);
            providerDataType =
                providerDataTypes.FirstOrDefault(x => x.IsRecommendedDotNetTypeMatch(alternateType))
                ?? providerDataTypes.FirstOrDefault(x => x.PrimaryDotnetType == alternateType)
                ?? providerDataTypes.FirstOrDefault(x =>
                    x.SupportedDotnetTypes.Contains(alternateType)
                );
        }

        return providerDataType
            ?? throw new NotSupportedException(
                $"No provider data type found for .NET type {dotnetType}."
            );
    }

    public virtual ProviderDataType GetRecommendedDataTypeForSqlType(
        string sqlTypeWithLengthPrecisionOrScale
    )
    {
        var providerDataTypes = GetProviderDataTypes();
        var providerDataType = providerDataTypes.FirstOrDefault(x =>
            x.IsRecommendedSqlTypeMatch(sqlTypeWithLengthPrecisionOrScale)
        );

        return providerDataType
            ?? throw new NotSupportedException(
                $"No provider data type found for SQL type {sqlTypeWithLengthPrecisionOrScale}."
            );
    }

    public virtual ProviderDataType[] GetSupportedDataTypesForDotnetType(Type dotnetType)
    {
        var providerDataTypes = GetProviderDataTypes();
        return providerDataTypes.Where(x => x.SupportedDotnetTypes.Contains(dotnetType)).ToArray();
    }

    public abstract ProviderDataType[] GetDefaultProviderDataTypes();

    protected static readonly Type[] CommonTypes =
    [
        typeof(char),
        typeof(string),
        typeof(bool),
        typeof(byte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(TimeSpan),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(Guid)
    ];

    protected static readonly Type[] CommonDictionaryTypes =
    [
        // dictionary types
        .. CommonTypes
            .Select(t => typeof(Dictionary<,>).MakeGenericType(t, typeof(string)))
            .ToArray(),
        .. CommonTypes
            .Select(t => typeof(Dictionary<,>).MakeGenericType(t, typeof(object)))
            .ToArray()
    ];

    protected static readonly Type[] CommonEnumerableTypes =
    [
        // enumerable types
        .. CommonTypes.Select(t => typeof(List<>).MakeGenericType(t)).ToArray(),
        .. CommonTypes.Select(t => t.MakeArrayType()).ToArray()
    ];
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class ProviderTypeMapBase<TProviderTypeMap> : ProviderTypeMapBase
    where TProviderTypeMap : class, IProviderTypeMap
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Guid, ProviderDataType> ProviderDataTypes = [];
    private static readonly Lazy<TProviderTypeMap> LazyInstance =
        new(Activator.CreateInstance<TProviderTypeMap>);
    public static TProviderTypeMap Instance => LazyInstance.Value;

    public virtual void Reset()
    {
        ProviderDataTypes.Clear();
        foreach (var providerDataType in GetDefaultProviderDataTypes())
        {
            ProviderDataTypes.TryAdd(Guid.NewGuid(), providerDataType);
        }
    }

    public static void RemoveProviderDataTypes(Func<ProviderDataType, bool> predicate)
    {
        var keys = ProviderDataTypes.Keys;
        foreach (var key in keys)
        {
            if (
                ProviderDataTypes.TryGetValue(key, out var providerDataType)
                && predicate(providerDataType)
            )
            {
                ProviderDataTypes.TryRemove(key, out _);
            }
        }
    }

    public static void UpdateProviderDataTypes(
        Func<ProviderDataType, bool> predicate,
        Func<ProviderDataType, ProviderDataType> update
    )
    {
        var keys = ProviderDataTypes.Keys;
        foreach (var key in keys)
        {
            if (
                ProviderDataTypes.TryGetValue(key, out var providerDataType)
                && predicate(providerDataType)
            )
            {
                ProviderDataTypes.TryUpdate(key, update(providerDataType), providerDataType);
            }
        }
    }

    public static void RegisterProviderDataType(ProviderDataType providerDataType)
    {
        ProviderDataTypes.TryAdd(Guid.NewGuid(), providerDataType);
    }

    public override ProviderDataType[] GetProviderDataTypes()
    {
        return [.. ProviderDataTypes.Values];
    }
}
