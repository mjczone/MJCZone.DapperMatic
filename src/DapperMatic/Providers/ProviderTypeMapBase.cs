using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace DapperMatic.Providers;

public abstract class ProviderTypeMapBase : IProviderTypeMap
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once CollectionNeverUpdated.Global
    public static readonly ConcurrentDictionary<DbProviderType, List<IProviderTypeMap>> TypeMaps =
        new();

    protected abstract DbProviderType ProviderType { get; }
    protected abstract ProviderSqlType[] ProviderSqlTypes { get; }

    public virtual bool TryGetRecommendedDotnetTypeMatchingSqlType(
        string fullSqlType,
        out (
            Type dotnetType,
            int? length,
            int? precision,
            int? scale,
            bool? isAutoIncrementing,
            Type[] allSupportedTypes
        )? recommendedDotnetType
    )
    {
        recommendedDotnetType = null;

        if (TypeMaps.TryGetValue(ProviderType, out var additionalTypeMaps))
        {
            foreach (var typeMap in additionalTypeMaps)
            {
                if (typeMap.TryGetRecommendedDotnetTypeMatchingSqlType(fullSqlType, out var rdt))
                {
                    recommendedDotnetType = rdt;
                    return true;
                }
            }
        }

        // perform some detective reasoning to pinpoint a recommended type
        var numbers = fullSqlType.ExtractNumbers();

        // try to find a sql provider type match
        var fullSqlTypeAlpha = fullSqlType.ToAlpha();
        var sqlType = ProviderSqlTypes.FirstOrDefault(t =>
            t.Name.ToAlpha().Equals(fullSqlTypeAlpha, StringComparison.OrdinalIgnoreCase)
        );
        if (sqlType == null)
            return false;

        var isAutoIncrementing = sqlType.AutoIncrementsAutomatically;

        switch (sqlType.Affinity)
        {
            case ProviderSqlTypeAffinity.Binary:
                recommendedDotnetType = (typeof(byte[]), null, null, null, null, [typeof(byte[])]);
                break;
            case ProviderSqlTypeAffinity.Boolean:
                recommendedDotnetType = (
                    typeof(bool),
                    null,
                    null,
                    null,
                    null,
                    [
                        typeof(bool),
                        typeof(short),
                        typeof(int),
                        typeof(long),
                        typeof(ushort),
                        typeof(uint),
                        typeof(ulong),
                        typeof(string)
                    ]
                );
                break;
            case ProviderSqlTypeAffinity.DateTime:
                if (sqlType.IsDateOnly == true)
                    recommendedDotnetType = (
                        typeof(DateOnly),
                        null,
                        null,
                        null,
                        null,
                        [typeof(DateOnly), typeof(DateTime), typeof(string)]
                    );
                else if (sqlType.IsTimeOnly == true)
                    recommendedDotnetType = (
                        typeof(TimeOnly),
                        null,
                        null,
                        null,
                        null,
                        [typeof(TimeOnly), typeof(DateTime), typeof(string)]
                    );
                else if (sqlType.IsYearOnly == true)
                    recommendedDotnetType = (
                        typeof(int),
                        null,
                        null,
                        null,
                        null,
                        [
                            typeof(short),
                            typeof(int),
                            typeof(long),
                            typeof(ushort),
                            typeof(uint),
                            typeof(ulong),
                            typeof(string)
                        ]
                    );
                else if (sqlType.IncludesTimeZone == true)
                    recommendedDotnetType = (
                        typeof(DateTimeOffset),
                        null,
                        null,
                        null,
                        null,
                        [typeof(DateTimeOffset), typeof(DateTime), typeof(string)]
                    );
                else
                    recommendedDotnetType = (
                        typeof(DateTime),
                        null,
                        null,
                        null,
                        null,
                        [typeof(DateTime), typeof(DateTimeOffset), typeof(string)]
                    );
                break;
            case ProviderSqlTypeAffinity.Integer:
                int? intPrecision = numbers.Length > 0 ? numbers[0] : null;
                if (sqlType.MinValue.HasValue && sqlType.MinValue == 0)
                {
                    if (sqlType.MaxValue.HasValue)
                    {
                        if (sqlType.MaxValue.Value <= ushort.MaxValue)
                            recommendedDotnetType = (
                                typeof(ushort),
                                null,
                                intPrecision,
                                null,
                                isAutoIncrementing,
                                [
                                    typeof(short),
                                    typeof(int),
                                    typeof(long),
                                    typeof(ushort),
                                    typeof(uint),
                                    typeof(ulong),
                                    typeof(string)
                                ]
                            );
                        else if (sqlType.MaxValue.Value <= uint.MaxValue)
                            recommendedDotnetType = (
                                typeof(uint),
                                null,
                                intPrecision,
                                null,
                                isAutoIncrementing,
                                [
                                    typeof(int),
                                    typeof(long),
                                    typeof(uint),
                                    typeof(ulong),
                                    typeof(string)
                                ]
                            );
                        else if (sqlType.MaxValue.Value <= ulong.MaxValue)
                            recommendedDotnetType = (
                                typeof(ulong),
                                null,
                                intPrecision,
                                null,
                                isAutoIncrementing,
                                [typeof(long), typeof(ulong), typeof(string)]
                            );
                    }
                    if (recommendedDotnetType == null)
                    {
                        recommendedDotnetType = (
                            typeof(uint),
                            null,
                            intPrecision,
                            null,
                            isAutoIncrementing,
                            [typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(string)]
                        );
                    }
                }
                if (recommendedDotnetType == null)
                {
                    if (sqlType.MaxValue.HasValue)
                    {
                        if (sqlType.MaxValue.Value <= short.MaxValue)
                            recommendedDotnetType = (
                                typeof(short),
                                null,
                                intPrecision,
                                null,
                                isAutoIncrementing,
                                [typeof(short), typeof(int), typeof(long), typeof(string)]
                            );
                        else if (sqlType.MaxValue.Value <= int.MaxValue)
                            recommendedDotnetType = (
                                typeof(int),
                                null,
                                intPrecision,
                                null,
                                isAutoIncrementing,
                                [typeof(int), typeof(long), typeof(string)]
                            );
                        else if (sqlType.MaxValue.Value <= long.MaxValue)
                            recommendedDotnetType = (
                                typeof(long),
                                null,
                                intPrecision,
                                null,
                                isAutoIncrementing,
                                [typeof(long), typeof(string)]
                            );
                    }
                    if (recommendedDotnetType == null)
                    {
                        recommendedDotnetType = (
                            typeof(int),
                            null,
                            intPrecision,
                            null,
                            isAutoIncrementing,
                            [typeof(int), typeof(long), typeof(string)]
                        );
                    }
                }
                break;
            case ProviderSqlTypeAffinity.Real:
                int? precision = numbers.Length > 0 ? numbers[0] : null;
                int? scale = numbers.Length > 1 ? numbers[1] : null;
                recommendedDotnetType = (
                    typeof(decimal),
                    null,
                    precision,
                    scale,
                    isAutoIncrementing,
                    [typeof(decimal), typeof(float), typeof(double), typeof(string)]
                );
                break;
            case ProviderSqlTypeAffinity.Text:
                int? length = numbers.Length > 0 ? numbers[0] : null;
                if (length > 8000)
                    length = int.MaxValue;
                recommendedDotnetType = (
                    typeof(string),
                    null,
                    length,
                    null,
                    null,
                    [typeof(string)]
                );
                break;
            case ProviderSqlTypeAffinity.Geometry:
            case ProviderSqlTypeAffinity.RangeType:
            case ProviderSqlTypeAffinity.Other:
                if (
                    sqlType.Name.Contains("json", StringComparison.OrdinalIgnoreCase)
                    || sqlType.Name.Contains("xml", StringComparison.OrdinalIgnoreCase)
                )
                    recommendedDotnetType = (
                        typeof(string),
                        null,
                        null,
                        null,
                        null,
                        [typeof(string)]
                    );
                else
                    recommendedDotnetType = (
                        typeof(object),
                        null,
                        null,
                        null,
                        null,
                        [typeof(object), typeof(string)]
                    );
                break;
        }

        return recommendedDotnetType != null;
    }

    public virtual bool TryGetRecommendedSqlTypeMatchingDotnetType(
        Type dotnetType,
        int? length,
        int? precision,
        int? scale,
        bool? autoIncrement,
        out ProviderSqlType? recommendedSqlType
    )
    {
        recommendedSqlType = null;

        if (TypeMaps.TryGetValue(ProviderType, out var additionalTypeMaps))
        {
            foreach (var typeMap in additionalTypeMaps)
            {
                if (
                    typeMap.TryGetRecommendedSqlTypeMatchingDotnetType(
                        dotnetType,
                        length,
                        precision,
                        scale,
                        autoIncrement,
                        out var rdt
                    )
                )
                {
                    recommendedSqlType = rdt;
                    return true;
                }
            }
        }

        if (ProviderType == DbProviderType.PostgreSql)
        {
            // Handle well-known types
            var typeName = dotnetType.Name;
            switch (typeName)
            {
                case "IPAddress":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("inet", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlCidr4":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("cidr", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "PhysicalAddress":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("macaddr", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlTsQuery":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("tsquery", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlTsVector":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("tsvector", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlPoint":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("point", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlLSeg":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("lseg", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlPath":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("path", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlPolygon":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("polygon", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlLine":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("line", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlCircle":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("circle", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "NpgsqlBox":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("box", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
                case "PostgisGeometry":
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Name.Equals("geometry", StringComparison.OrdinalIgnoreCase)
                    );
                    break;
            }

            if (
                dotnetType == typeof(Dictionary<string, string>)
                || dotnetType == typeof(IDictionary<string, string>)
            )
                recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                    t.Name.Equals("hstore", StringComparison.OrdinalIgnoreCase)
                );

            if (recommendedSqlType != null)
                return true;
        }

        // the dotnetType could be a nullable type, so we need to check for that
        // and get the underlying type
        if (dotnetType.IsGenericType && dotnetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            dotnetType = Nullable.GetUnderlyingType(dotnetType)!;
        }

        // We're trying to find the right type to use as a lookup type
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

        // WARNING!! The following showcases why the order within each affinity group of the provider sql types matters, as the recommended type
        //           is going to be the first match for the given scenario
        switch (dotnetType)
        {
            case not null when dotnetType == typeof(sbyte):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(sbyte.MinValue) <= sbyte.MinValue
                            && t.MaxValue.GetValueOrDefault(sbyte.MaxValue) >= sbyte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(sbyte.MaxValue) >= sbyte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(sbyte.MinValue) <= sbyte.MinValue
                            && t.MaxValue.GetValueOrDefault(sbyte.MaxValue) >= sbyte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(sbyte.MaxValue) >= sbyte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(sbyte.MinValue) <= sbyte.MinValue
                        && t.MaxValue.GetValueOrDefault(sbyte.MaxValue) >= sbyte.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(byte):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(byte.MinValue) <= byte.MinValue
                            && t.MaxValue.GetValueOrDefault(byte.MaxValue) >= byte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(byte.MaxValue) >= byte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(byte.MinValue) <= byte.MinValue
                            && t.MaxValue.GetValueOrDefault(byte.MaxValue) >= byte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(byte.MaxValue) >= byte.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(byte.MinValue) <= byte.MinValue
                        && t.MaxValue.GetValueOrDefault(byte.MaxValue) >= byte.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(short):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(short.MinValue) <= short.MinValue
                            && t.MaxValue.GetValueOrDefault(short.MaxValue) >= short.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(short.MaxValue) >= short.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(short.MinValue) <= short.MinValue
                            && t.MaxValue.GetValueOrDefault(short.MaxValue) >= short.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(short.MaxValue) >= short.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(short.MinValue) <= short.MinValue
                        && t.MaxValue.GetValueOrDefault(short.MaxValue) >= short.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(int):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(int.MinValue) <= int.MinValue
                            && t.MaxValue.GetValueOrDefault(int.MaxValue) >= int.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(int.MaxValue) >= int.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(int.MinValue) <= int.MinValue
                            && t.MaxValue.GetValueOrDefault(int.MaxValue) >= int.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(int.MaxValue) >= int.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(int.MinValue) <= int.MinValue
                        && t.MaxValue.GetValueOrDefault(int.MaxValue) >= int.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(long):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(long.MinValue) <= long.MinValue
                            && t.MaxValue.GetValueOrDefault(long.MaxValue) >= long.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(long.MaxValue) >= long.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(long.MinValue) <= long.MinValue
                            && t.MaxValue.GetValueOrDefault(long.MaxValue) >= long.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(long.MaxValue) >= long.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(long.MinValue) <= long.MinValue
                        && t.MaxValue.GetValueOrDefault(long.MaxValue) >= long.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(ushort):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(ushort.MinValue) <= ushort.MinValue
                            && t.MaxValue.GetValueOrDefault(ushort.MaxValue) >= ushort.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(ushort.MaxValue) >= ushort.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(ushort.MinValue) <= ushort.MinValue
                            && t.MaxValue.GetValueOrDefault(ushort.MaxValue) >= ushort.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(ushort.MaxValue) >= ushort.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.MinValue.GetValueOrDefault(0) == 0
                            && t.MaxValue.GetValueOrDefault(ushort.MaxValue) >= ushort.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.MaxValue.GetValueOrDefault(ushort.MaxValue) >= ushort.MaxValue
                        );
                break;
            case not null when dotnetType == typeof(uint):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(uint.MinValue) <= uint.MinValue
                            && t.MaxValue.GetValueOrDefault(uint.MaxValue) >= uint.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(uint.MaxValue) >= uint.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(uint.MinValue) <= uint.MinValue
                            && t.MaxValue.GetValueOrDefault(uint.MaxValue) >= uint.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(uint.MaxValue) >= uint.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.MinValue.GetValueOrDefault(0) == 0
                            && t.MaxValue.GetValueOrDefault(uint.MaxValue) >= uint.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.MaxValue.GetValueOrDefault(uint.MaxValue) >= uint.MaxValue
                        );
                break;
            case not null when dotnetType == typeof(ulong):
                if (autoIncrement.GetValueOrDefault(false))
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MinValue.GetValueOrDefault(ulong.MinValue) <= ulong.MinValue
                            && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.AutoIncrementsAutomatically
                            && t.MaxValue.GetValueOrDefault(long.MaxValue) >= long.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MinValue.GetValueOrDefault(ulong.MinValue) <= ulong.MinValue
                            && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.CanUseToAutoIncrement
                            && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer && t.CanUseToAutoIncrement
                        );
                else
                    recommendedSqlType =
                        ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.MinValue.GetValueOrDefault(0) == 0
                            && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                        )
                        ?? ProviderSqlTypes.FirstOrDefault(t =>
                            t.Affinity == ProviderSqlTypeAffinity.Integer
                            && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                        );
                break;
            case not null when dotnetType == typeof(bool):
                recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                    t.Affinity == ProviderSqlTypeAffinity.Boolean
                );
                break;
            case not null when dotnetType == typeof(decimal):
                recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                    t.Affinity == ProviderSqlTypeAffinity.Real
                    && t.MinValue.GetValueOrDefault((double)decimal.MinValue)
                        <= (double)decimal.MinValue
                    && t.MaxValue.GetValueOrDefault((double)decimal.MaxValue)
                        >= (double)decimal.MaxValue
                );
                break;
            case not null when dotnetType == typeof(double):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Real
                        && t.Name.Equals("double", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.Name.Contains("double", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(double.MinValue) <= double.MinValue
                        && t.MaxValue.GetValueOrDefault(double.MaxValue) >= double.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(float):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Real
                        && t.Name.Equals("float", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.Name.Contains("float", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(float.MinValue) <= float.MinValue
                        && t.MaxValue.GetValueOrDefault(float.MaxValue) >= float.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(DateTime):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime && t.IncludesTimeZone != true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime
                    );
                break;
            case not null when dotnetType == typeof(DateTimeOffset):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime && t.IncludesTimeZone == true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime
                    );
                break;
            case not null when dotnetType == typeof(DateOnly):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime && t.IsDateOnly == true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime
                    );
                break;
            case not null when dotnetType == typeof(TimeOnly):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime && t.IsTimeOnly == true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.DateTime
                    );
                break;
            case not null when dotnetType == typeof(TimeSpan):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MinValue.GetValueOrDefault(0) == 0
                        && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Integer
                        && t.MaxValue.GetValueOrDefault(ulong.MaxValue) >= ulong.MaxValue
                    );
                break;
            case not null when dotnetType == typeof(byte[]):
                recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                    t.Affinity == ProviderSqlTypeAffinity.Binary
                );
                break;
            case not null when dotnetType == typeof(Guid):
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text && t.IsGuidOnly == true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && !string.IsNullOrWhiteSpace(t.FormatWithLength)
                        && t.IsFixedLength == true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && !string.IsNullOrWhiteSpace(t.FormatWithLength)
                        && t.IsFixedLength != true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text && t.IsFixedLength != true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                    );
                break;
            case not null when dotnetType == typeof(string):
            case not null when dotnetType == typeof(char[]):
                if (length.HasValue && length.Value > 8000)
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && t.IsMaxStringLengthType == true
                        && t.IsFixedLength != true
                    );
                if (recommendedSqlType == null && length.HasValue && length.Value <= 8000)
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && !string.IsNullOrWhiteSpace(t.FormatWithLength)
                        && t.IsFixedLength != true
                    );
                if (recommendedSqlType == null)
                    recommendedSqlType = ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text && t.IsFixedLength != true
                    );
                break;
            case not null when dotnetType == typeof(Dictionary<,>):
            case not null when dotnetType == typeof(IDictionary<,>):
            case not null when dotnetType == typeof(IEnumerable<>):
            case not null when dotnetType == typeof(ICollection<>):
            case not null when dotnetType == typeof(List<>):
            case not null when dotnetType == typeof(IList<>):
            case not null when dotnetType == typeof(object[]):
            case not null when dotnetType == typeof(object):
            case not null when dotnetType.IsClass:
                recommendedSqlType =
                    ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && t.Name.Contains("json", StringComparison.OrdinalIgnoreCase)
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && t.IsMaxStringLengthType == true
                        && t.IsFixedLength != true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text
                        && !string.IsNullOrWhiteSpace(t.FormatWithLength)
                        && t.IsFixedLength != true
                    )
                    ?? ProviderSqlTypes.FirstOrDefault(t =>
                        t.Affinity == ProviderSqlTypeAffinity.Text && t.IsFixedLength != true
                    );
                break;
        }

        return recommendedSqlType != null;
    }
}
