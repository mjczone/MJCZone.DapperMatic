using System.Collections.Concurrent;

namespace DapperMatic.Providers;

public abstract class DbProviderTypeMapBase : IDbProviderTypeMap
{
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once CollectionNeverUpdated.Global
    public static readonly ConcurrentDictionary<DbProviderType, List<IDbProviderTypeMap>> TypeMaps =
        new();

    protected abstract DbProviderType ProviderType { get; }
    protected abstract DbProviderSqlType[] ProviderSqlTypes { get; }

    private Dictionary<string, DbProviderSqlType>? _lookup = null;
    protected virtual Dictionary<string, DbProviderSqlType> ProviderSqlTypeLookup =>
        _lookup ??= ProviderSqlTypes.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

    public abstract string SqTypeForStringLengthMax { get; }
    public abstract string SqTypeForBinaryLengthMax { get; }
    public abstract string SqlTypeForJson { get; }
    public virtual string SqTypeForUnknownDotnetType => SqTypeForStringLengthMax;
    protected virtual int DefaultLength { get; set; } = 255;

    public virtual bool UseIntegersForEnumTypes { get; set; } = false;

    public virtual bool TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
        string fullSqlType,
        out DbProviderDotnetTypeDescriptor? descriptor
    )
    {
        descriptor = new DbProviderDotnetTypeDescriptor(typeof(string));

        // Prioritize any custom mappings
        if (TypeMaps.TryGetValue(ProviderType, out var additionalTypeMaps))
        {
            foreach (var typeMap in additionalTypeMaps)
            {
                if (
                    typeMap.TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
                        fullSqlType,
                        out var rdt
                    )
                )
                {
                    descriptor = rdt;
                    return true;
                }
            }
        }

        if (
            !TryGetProviderSqlTypeFromFullSqlTypeName(fullSqlType, out var providerSqlType)
            || providerSqlType == null
        )
            return false;

        // perform some detective reasoning to pinpoint a recommended type
        var numbers = fullSqlType.ExtractNumbers();
        var isAutoIncrementing = providerSqlType.AutoIncrementsAutomatically;
        var unicode = providerSqlType.IsUnicode;

        switch (providerSqlType.Affinity)
        {
            case DbProviderSqlTypeAffinity.Binary:
                descriptor = new(typeof(byte[]));
                break;
            case DbProviderSqlTypeAffinity.Boolean:
                descriptor = new(
                    typeof(bool),
                    otherSupportedTypes:
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
                break;
            case DbProviderSqlTypeAffinity.DateTime:
                if (providerSqlType.IsDateOnly == true)
                    descriptor = new(
                        typeof(DateOnly),
                        otherSupportedTypes: [typeof(DateOnly), typeof(DateTime), typeof(string)]
                    );
                else if (providerSqlType.IsTimeOnly == true)
                    descriptor = new(
                        typeof(TimeOnly),
                        otherSupportedTypes: [typeof(TimeOnly), typeof(DateTime), typeof(string)]
                    );
                else if (providerSqlType.IsYearOnly == true)
                    descriptor = new(
                        typeof(int),
                        otherSupportedTypes:
                        [
                            typeof(short),
                            typeof(long),
                            typeof(ushort),
                            typeof(uint),
                            typeof(ulong),
                            typeof(string)
                        ]
                    );
                else if (providerSqlType.IncludesTimeZone == true)
                    descriptor = new DbProviderDotnetTypeDescriptor(
                        typeof(DateTimeOffset),
                        otherSupportedTypes: [typeof(DateTime), typeof(string)]
                    );
                else
                    descriptor = new(
                        typeof(DateTime),
                        otherSupportedTypes: [typeof(DateTimeOffset), typeof(string)]
                    );
                break;
            case DbProviderSqlTypeAffinity.Integer:
                int? intPrecision = numbers.Length > 0 ? numbers[0] : null;
                if (providerSqlType.MinValue.HasValue && providerSqlType.MinValue == 0)
                {
                    if (providerSqlType.MaxValue.HasValue)
                    {
                        if (providerSqlType.MaxValue.Value <= ushort.MaxValue)
                            descriptor = new(
                                typeof(ushort),
                                precision: intPrecision,
                                autoIncrement: isAutoIncrementing,
                                otherSupportedTypes:
                                [
                                    typeof(short),
                                    typeof(int),
                                    typeof(long),
                                    typeof(uint),
                                    typeof(ulong),
                                    typeof(string)
                                ]
                            );
                        else if (providerSqlType.MaxValue.Value <= uint.MaxValue)
                            descriptor = new(
                                typeof(uint),
                                precision: intPrecision,
                                autoIncrement: isAutoIncrementing,
                                otherSupportedTypes:
                                [
                                    typeof(int),
                                    typeof(long),
                                    typeof(ulong),
                                    typeof(string)
                                ]
                            );
                        else if (providerSqlType.MaxValue.Value <= ulong.MaxValue)
                            descriptor = new(
                                typeof(ulong),
                                precision: intPrecision,
                                autoIncrement: isAutoIncrementing,
                                otherSupportedTypes: [typeof(long), typeof(string)]
                            );
                    }
                    descriptor ??= new(
                        typeof(uint),
                        precision: intPrecision,
                        autoIncrement: isAutoIncrementing,
                        otherSupportedTypes:
                        [
                            typeof(int),
                            typeof(long),
                            typeof(ulong),
                            typeof(string)
                        ]
                    );
                }
                if (descriptor == null)
                {
                    if (providerSqlType.MaxValue.HasValue)
                    {
                        if (providerSqlType.MaxValue.Value <= short.MaxValue)
                            descriptor = new(
                                typeof(short),
                                precision: intPrecision,
                                autoIncrement: isAutoIncrementing,
                                otherSupportedTypes: [typeof(int), typeof(long), typeof(string)]
                            );
                        else if (providerSqlType.MaxValue.Value <= int.MaxValue)
                            descriptor = new(
                                typeof(int),
                                precision: intPrecision,
                                autoIncrement: isAutoIncrementing,
                                otherSupportedTypes: [typeof(long), typeof(string)]
                            );
                        else if (providerSqlType.MaxValue.Value <= long.MaxValue)
                            descriptor = new(
                                typeof(long),
                                precision: intPrecision,
                                autoIncrement: isAutoIncrementing,
                                otherSupportedTypes: [typeof(string)]
                            );
                    }
                    descriptor ??= new(
                        typeof(int),
                        precision: intPrecision,
                        autoIncrement: isAutoIncrementing,
                        otherSupportedTypes: [typeof(long), typeof(string)]
                    );
                }
                break;
            case DbProviderSqlTypeAffinity.Real:
                int? precision = numbers.Length > 0 ? numbers[0] : null;
                int? scale = numbers.Length > 1 ? numbers[1] : null;
                descriptor = new(
                    typeof(decimal),
                    precision: precision,
                    scale: scale,
                    autoIncrement: isAutoIncrementing,
                    otherSupportedTypes:
                    [
                        typeof(decimal),
                        typeof(float),
                        typeof(double),
                        typeof(string)
                    ]
                );
                break;
            case DbProviderSqlTypeAffinity.Text:
                int? length = numbers.Length > 0 ? numbers[0] : null;
                if (length >= 8000)
                    length = int.MaxValue;
                descriptor = new(
                    typeof(string),
                    length: length,
                    unicode: unicode,
                    otherSupportedTypes: [typeof(string)]
                );
                break;
            case DbProviderSqlTypeAffinity.Geometry:
            case DbProviderSqlTypeAffinity.RangeType:
            case DbProviderSqlTypeAffinity.Other:
            default:
                if (
                    providerSqlType.Name.Contains("json", StringComparison.OrdinalIgnoreCase)
                    || providerSqlType.Name.Contains("xml", StringComparison.OrdinalIgnoreCase)
                )
                    descriptor = new(
                        typeof(string),
                        length: int.MaxValue,
                        unicode: unicode,
                        otherSupportedTypes: [typeof(string)]
                    );
                else
                    descriptor = new(
                        typeof(object),
                        otherSupportedTypes: [typeof(object), typeof(string)]
                    );
                break;
        }

        return descriptor != null;
    }

    public virtual bool TryGetProviderSqlTypeMatchingDotnetType(
        DbProviderDotnetTypeDescriptor descriptor,
        out DbProviderSqlType? providerSqlType
    )
    {
        providerSqlType = null;

        // Prioritize any custom mappings
        if (TypeMaps.TryGetValue(ProviderType, out var additionalTypeMaps))
        {
            foreach (var typeMap in additionalTypeMaps)
            {
                if (typeMap.TryGetProviderSqlTypeMatchingDotnetType(descriptor, out var rdt))
                {
                    providerSqlType = rdt;
                    return true;
                }
            }
        }

        var dotnetType = descriptor.DotnetType;

        // Enums become strings, or integers if UseIntegersForEnumTypes is true
        if (dotnetType.IsEnum)
        {
            return TryGetProviderSqlTypeMatchingEnumType(
                dotnetType,
                descriptor.Length,
                ref providerSqlType
            );
        }

        // char becomes string(1)
        if (dotnetType == typeof(char) && (descriptor.Length == null || descriptor.Length == 1))
        {
            return TryGetProviderSqlTypeMatchingDotnetType(
                new DbProviderDotnetTypeDescriptor(typeof(string), 1, unicode: descriptor.Unicode),
                out providerSqlType
            );
        }

        if (TryGetProviderSqlTypeMatchingDotnetTypeInternal(descriptor, out providerSqlType))
            return true;

        return providerSqlType != null;
    }

    protected abstract bool TryGetProviderSqlTypeMatchingDotnetTypeInternal(
        DbProviderDotnetTypeDescriptor descriptor,
        out DbProviderSqlType? providerSqlType
    );

    protected virtual bool TryGetProviderSqlTypeFromFullSqlTypeName(
        string fullSqlType,
        out DbProviderSqlType? providerSqlType
    )
    {
        // perform some detective reasoning to pinpoint a recommended type
        var numbers = fullSqlType.ExtractNumbers();

        // try to find a sql provider type match by removing the length, precision, and scale
        // from the sql type name and converting it to an alpha only representation of the type
        var fullSqlTypeAlpha = fullSqlType
            .DiscardLengthPrecisionAndScaleFromSqlTypeName()
            .ToAlpha("[]");

        providerSqlType = ProviderSqlTypes.FirstOrDefault(t =>
            t.Name.DiscardLengthPrecisionAndScaleFromSqlTypeName()
                .ToAlpha("[]")
                .Equals(fullSqlTypeAlpha, StringComparison.OrdinalIgnoreCase)
        );

        return providerSqlType != null;
    }

    protected virtual bool TryGetProviderSqlTypeMatchingEnumType(
        Type dotnetType,
        int? length,
        ref DbProviderSqlType? providerSqlType
    )
    {
        if (UseIntegersForEnumTypes)
        {
            return TryGetProviderSqlTypeMatchingDotnetType(
                new DbProviderDotnetTypeDescriptor(typeof(int), length),
                out providerSqlType
            );
        }

        if (length == null)
        {
            var maxEnumNameLength = Enum.GetNames(dotnetType).Max(m => m.Length);
            var x = 64;
            while (x < maxEnumNameLength)
                x *= 2;
            length = x;
        }

        return TryGetProviderSqlTypeMatchingDotnetType(
            new DbProviderDotnetTypeDescriptor(typeof(string), length),
            out providerSqlType
        );
    }
}
