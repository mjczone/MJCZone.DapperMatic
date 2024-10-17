using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Providers;

public record DotnetTypeToSqlTypeMap(
    Type DotnetType,
    string SqlType,
    string[] OtherSupportedSqlTypes
);

public record SqlTypeToDotnetTypeMap(
    string SqlType,
    Type DotnetType,
    Type[] OtherSupportedDotnetTypes
);

public record ProviderSqlType(
    string SqlType,
    string? AliasForSqlType,
    string? SqlTypeWithLength,
    string? SqlTypeWithPrecision,
    string? SqlTypeWithPrecisionAndScale,
    string? SqlTypeWithMaxLength,
    bool CanAutoIncrement,
    bool NotNullable,
    int? DefaultLength,
    int? DefaultPrecision,
    int? DefaultScale
);

public static class ProviderSqlTypeExtensions
{
    public static bool SupportsLength(this ProviderSqlType providerSqlType) =>
        !string.IsNullOrWhiteSpace(providerSqlType.SqlTypeWithLength);

    public static bool SupportsPrecision(this ProviderSqlType providerSqlType) =>
        !string.IsNullOrWhiteSpace(providerSqlType.SqlTypeWithPrecision);

    public static bool SupportsScale(this ProviderSqlType providerSqlType) =>
        !string.IsNullOrWhiteSpace(providerSqlType.SqlTypeWithPrecisionAndScale);
}

public abstract class ProviderTypeMapBase : IProviderTypeMap
{
    public abstract void AddDotnetTypeToSqlTypeMap(Func<Type, string?> map);
    public abstract void AddSqlTypeToDotnetTypeMap(
        Func<
            string,
            (Type dotnetType, int? length, int? precision, int? scale, Type[] otherSupportedTypes)?
        > map
    );
    public abstract IReadOnlyList<ProviderSqlType> GetProviderSqlTypes();
    public abstract bool TryAddOrUpdateProviderSqlType(ProviderSqlType providerSqlType);
    public abstract bool TryGetRecommendedDotnetTypeMatchingSqlType(
        string fullSqlType,
        out (
            Type dotnetType,
            int? length,
            int? precision,
            int? scale,
            Type[] otherSupportedTypes
        )? recommendedDotnetType
    );
    public abstract bool TryGetRecommendedSqlTypeMatchingDotnetType(
        Type dotnetType,
        out ProviderSqlType? recommendedSqlType
    );
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class ProviderTypeMapBase<TProviderTypeMap> : ProviderTypeMapBase
    where TProviderTypeMap : class, IProviderTypeMap
{
    protected ProviderTypeMapBase(
        ProviderSqlType[] providerSqlTypes,
        DotnetTypeToSqlTypeMap[] dotnetTypeToSqlTypeMaps,
        SqlTypeToDotnetTypeMap[] sqlTypeToDotnetTypeMaps
    )
    {
        foreach (var type in providerSqlTypes)
        {
            _providerSqlTypes.TryAdd(
                type.SqlType.ToAlpha(),
                new ProviderSqlType(
                    type.SqlType,
                    type.AliasForSqlType,
                    type.SqlTypeWithLength,
                    type.SqlTypeWithPrecision,
                    type.SqlTypeWithPrecisionAndScale,
                    type.SqlTypeWithMaxLength,
                    type.CanAutoIncrement,
                    type.NotNullable,
                    type.DefaultLength,
                    type.DefaultPrecision,
                    type.DefaultScale
                )
            );
        }

        foreach (var type in dotnetTypeToSqlTypeMaps)
        {
            _dotnetTypeToSqlTypeMap.TryAdd(
                type.DotnetType,
                new DotnetTypeToSqlTypeMap(
                    type.DotnetType,
                    type.SqlType,
                    type.OtherSupportedSqlTypes
                )
            );
        }

        foreach (var type in sqlTypeToDotnetTypeMaps)
        {
            _sqlTypeToDotnetTypeMap.TryAdd(
                type.SqlType.ToAlpha(),
                new SqlTypeToDotnetTypeMap(
                    type.SqlType,
                    type.DotnetType,
                    type.OtherSupportedDotnetTypes
                )
            );
        }
    }

    private static ConcurrentDictionary<string, ProviderSqlType> _providerSqlTypes = new();
    private static ConcurrentDictionary<string, SqlTypeToDotnetTypeMap> _sqlTypeToDotnetTypeMap =
        new();
    private static ConcurrentDictionary<Type, DotnetTypeToSqlTypeMap> _dotnetTypeToSqlTypeMap =
        new();

    public override IReadOnlyList<ProviderSqlType> GetProviderSqlTypes()
    {
        return new ReadOnlyCollection<ProviderSqlType>([.. _providerSqlTypes.Values]);
    }

    public override bool TryGetRecommendedDotnetTypeMatchingSqlType(
        string fullSqlType,
        out (
            Type dotnetType,
            int? length,
            int? precision,
            int? scale,
            Type[] otherSupportedTypes
        )? recommendedDotnetType
    )
    {
        recommendedDotnetType = null;

        // start with the dynamic mapping references in reverse order
        foreach (var key in dynamicSqlTypeToDotnetTypeMaps.Keys.OrderByDescending(d => d))
        {
            var map = dynamicSqlTypeToDotnetTypeMaps[key];
            var result = map(fullSqlType);
            if (result != null)
            {
                recommendedDotnetType = result.Value;
                return true;
            }
        }

        var sqlTypeKey = fullSqlType.ToAlpha();
        if (_sqlTypeToDotnetTypeMap.TryGetValue(sqlTypeKey, out var sqlTypeToDotnetTypeMapping))
        {
            var dotnetType = sqlTypeToDotnetTypeMapping.DotnetType;
            int? length = null;
            int? precision = null;
            int? scale = null;

            var numbers = fullSqlType.ExtractNumbers();

            if (numbers.Length == 1 && dotnetType == typeof(string))
            {
                length = numbers.FirstOrDefault();
            }
            else
            {
                precision = numbers.FirstOrDefault();
                if (numbers.Length > 1)
                {
                    scale = numbers.Skip(1).FirstOrDefault();
                }
            }

            if (
                _providerSqlTypes.TryGetValue(sqlTypeKey, out var providerSqlType)
                && recommendedDotnetType.HasValue
            )
            {
                if (!string.IsNullOrWhiteSpace(providerSqlType.SqlTypeWithLength))
                {
                    length = (int?)numbers.FirstOrDefault() ?? providerSqlType.DefaultLength;
                    precision = null;
                    scale = null;
                }

                if (
                    !string.IsNullOrWhiteSpace(providerSqlType.SqlTypeWithPrecision)
                    && numbers.Length == 1
                )
                {
                    precision = (int?)numbers.FirstOrDefault() ?? providerSqlType.DefaultPrecision;
                }
                else if (
                    !string.IsNullOrWhiteSpace(providerSqlType.SqlTypeWithPrecisionAndScale)
                    && numbers.Length > 1
                )
                {
                    precision = (int?)numbers.FirstOrDefault() ?? providerSqlType.DefaultPrecision;
                    scale = (int?)numbers.Skip(1).FirstOrDefault() ?? providerSqlType.DefaultScale;
                }
            }

            recommendedDotnetType = (
                dotnetType,
                length,
                precision,
                scale,
                sqlTypeToDotnetTypeMapping.OtherSupportedDotnetTypes
            );
            return true;
        }

        return false;
    }

    public override bool TryGetRecommendedSqlTypeMatchingDotnetType(
        Type dotnetType,
        out ProviderSqlType? recommendedSqlType
    )
    {
        // the dotnetType could be a nullable type, so we need to check for that
        // and get the underlying type
        if (dotnetType.IsGenericType && dotnetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            dotnetType = Nullable.GetUnderlyingType(dotnetType)!;
        }

        //TODO: Add support for arrays, lists, and other collection types
        //      We're trying to find the right type to use as a lookup type into the provider data map
        // IDictionary<,>	Dictionary<,>	IEnumerable<>	ICollection<>	List<>	object[]
        if (dotnetType.IsArray)
        {
            // dotnetType = dotnetType.GetElementType()!;
            dotnetType = typeof(object[]);
        }
        else if (
            dotnetType.IsGenericType
            && dotnetType.GetGenericTypeDefinition() == typeof(List<>)
        )
        {
            // dotnetType = dotnetType.GetGenericArguments()[0];
            dotnetType = typeof(List<>);
        }
        else if (
            dotnetType.IsGenericType
            && dotnetType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
        )
        {
            // dotnetType = dotnetType.GetGenericArguments()[1];
            dotnetType = typeof(IDictionary<,>);
        }
        else if (
            dotnetType.IsGenericType
            && dotnetType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
        )
        {
            // dotnetType = dotnetType.GetGenericArguments()[1];
            dotnetType = typeof(Dictionary<,>);
        }
        else if (
            dotnetType.IsGenericType
            && dotnetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        )
        {
            // dotnetType = dotnetType.GetGenericArguments()[0];
            dotnetType = typeof(IEnumerable<>);
        }
        else if (
            dotnetType.IsGenericType
            && dotnetType.GetGenericTypeDefinition() == typeof(ICollection<>)
        )
        {
            // dotnetType = dotnetType.GetGenericArguments()[0];
            dotnetType = typeof(ICollection<>);
        }
        else if (
            dotnetType.IsGenericType
            && dotnetType.GetGenericTypeDefinition() == typeof(IList<>)
        )
        {
            // dotnetType = dotnetType.GetGenericArguments()[0];
            dotnetType = typeof(IList<>);
        }
        else if (dotnetType.IsGenericType)
        {
            // could probably just stick with this, but the above
            // is more explicit for now
            dotnetType = dotnetType.GetGenericTypeDefinition();
        }

        recommendedSqlType = null;

        // start with the dynamic mapping references in reverse order
        foreach (var key in dynamicDotnetTypeToSqlTypeMaps.Keys.OrderByDescending(d => d))
        {
            var map = dynamicDotnetTypeToSqlTypeMaps[key];
            var result = map(dotnetType);
            if (
                !string.IsNullOrWhiteSpace(result)
                && _providerSqlTypes.TryGetValue(result.ToAlpha(), out var sqlType)
            )
            {
                recommendedSqlType = sqlType;
                return true;
            }
        }

        if (
            _dotnetTypeToSqlTypeMap.TryGetValue(dotnetType, out var dotnetTypeToSqlTypeMapping)
            && _providerSqlTypes.TryGetValue(
                dotnetTypeToSqlTypeMapping.SqlType.ToAlpha(),
                out var providerSqlType
            )
        )
        {
            recommendedSqlType = providerSqlType;
            return true;
        }

        // if we still haven't found a match, let's see if it's a custom class
        // with an empty constructor
        if (
            dotnetType.IsClass
            && !dotnetType.IsAbstract
            && dotnetType.GetConstructor(Type.EmptyTypes) != null
        )
        {
            // use the `typeof(object)` as a fallback in this case
            if (
                _dotnetTypeToSqlTypeMap.TryGetValue(
                    typeof(object),
                    out var objectTypeToSqlTypeMapping
                )
                && _providerSqlTypes.TryGetValue(
                    objectTypeToSqlTypeMapping.SqlType.ToAlpha(),
                    out providerSqlType
                )
            )
            {
                recommendedSqlType = providerSqlType;
                return true;
            }
        }

        return false;
    }

    protected static readonly ConcurrentDictionary<
        int,
        Func<Type, string?>
    > dynamicDotnetTypeToSqlTypeMaps = new();

    public override void AddDotnetTypeToSqlTypeMap(Func<Type, string?> map)
    {
        dynamicDotnetTypeToSqlTypeMaps.TryAdd(dynamicDotnetTypeToSqlTypeMaps.Count + 1, map);
    }

    protected static readonly ConcurrentDictionary<
        int,
        Func<
            string,
            (Type dotnetType, int? length, int? precision, int? scale, Type[] otherSupportedTypes)?
        >
    > dynamicSqlTypeToDotnetTypeMaps = new();

    public override void AddSqlTypeToDotnetTypeMap(
        Func<
            string,
            (Type dotnetType, int? length, int? precision, int? scale, Type[] otherSupportedTypes)?
        > map
    )
    {
        dynamicSqlTypeToDotnetTypeMaps.TryAdd(dynamicSqlTypeToDotnetTypeMaps.Count + 1, map);
    }

    public override bool TryAddOrUpdateProviderSqlType(ProviderSqlType providerSqlType)
    {
        if (_providerSqlTypes.TryGetValue(providerSqlType.SqlType.ToAlpha(), out var existingType))
        {
            _providerSqlTypes.TryUpdate(
                providerSqlType.SqlType.ToAlpha(),
                new ProviderSqlType(
                    providerSqlType.SqlType,
                    providerSqlType.AliasForSqlType ?? existingType.AliasForSqlType,
                    providerSqlType.SqlTypeWithLength ?? existingType.SqlTypeWithLength,
                    providerSqlType.SqlTypeWithPrecision ?? existingType.SqlTypeWithPrecision,
                    providerSqlType.SqlTypeWithPrecisionAndScale
                        ?? existingType.SqlTypeWithPrecisionAndScale,
                    providerSqlType.SqlTypeWithMaxLength ?? existingType.SqlTypeWithMaxLength,
                    providerSqlType.CanAutoIncrement,
                    providerSqlType.NotNullable,
                    providerSqlType.DefaultLength ?? existingType.DefaultLength,
                    providerSqlType.DefaultPrecision ?? existingType.DefaultPrecision,
                    providerSqlType.DefaultScale ?? existingType.DefaultScale
                ),
                existingType
            );
            return true;
        }

        return _providerSqlTypes.TryAdd(providerSqlType.SqlType.ToAlpha(), providerSqlType);
    }
}
