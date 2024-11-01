namespace DapperMatic.Providers;

/// <summary>
/// The provider SQL type.
/// </summary>
/// <param name="SqlType"></param>
/// <param name="AliasForSqlType"></param>
/// <param name="SqlTypeWithLength"></param>
/// <param name="SqlTypeWithPrecision"></param>
/// <param name="SqlTypeWithPrecisionAndScale"></param>
/// <param name="SqlTypeWithMaxLength"></param>
/// <param name="CanAutoIncrement"></param>
/// <param name="NotNullable"></param>
/// <param name="DefaultLength"></param>
/// <param name="DefaultPrecision"></param>
/// <param name="DefaultScale"></param>
public class ProviderSqlType(
	ProviderSqlTypeAffinity affinity,
	string name,
	Type? recommendedDotnetType = null,
	string? aliasOf = null,
	string? formatWithLength = null,
	string? formatWithPrecision = null,
	string? formatWithPrecisionAndScale = null,
	int? defaultLength = null,
	int? defaultPrecision = null,
	int? defaultScale = null,
	bool canUseToAutoIncrement = false,
	bool autoIncrementsAutomatically = false,
	double? minValue = null,
	double? maxValue = null,
	bool includesTimeZone = false,
	bool isDateOnly = false,
	bool isTimeOnly = false,
	bool isYearOnly = false,
	bool isMaxStringLengthType = false,
	bool isFixedLength = false,
	bool isGuidOnly = false)
{
	public ProviderSqlTypeAffinity Affinity { get; init; } = affinity;
	public string Name { get; init; } = name;
	public Type? RecommendedDotnetType { get; init; } = recommendedDotnetType;
	public string? AliasOf { get; set; } = aliasOf;
	public string? FormatWithLength { get; init; } = formatWithLength;
	public string? FormatWithPrecision { get; init; } = formatWithPrecision;
	public string? FormatWithPrecisionAndScale { get; init; } = formatWithPrecisionAndScale;
	public int? DefaultLength { get; set; } = defaultLength;
	public int? DefaultPrecision { get; set; } = defaultPrecision;
	public int? DefaultScale { get; set; } = defaultScale;
	public bool CanUseToAutoIncrement { get; init; } = canUseToAutoIncrement;
	public bool AutoIncrementsAutomatically { get; init; } = autoIncrementsAutomatically;
	public double? MinValue { get; init; } = minValue;
	public double? MaxValue { get; init; } = maxValue;
	public bool IncludesTimeZone { get; init; } = includesTimeZone;
	public bool IsDateOnly { get; init; } = isDateOnly;
	public bool IsTimeOnly { get; init; } = isTimeOnly;
	public bool IsYearOnly { get; init; } = isYearOnly;
	public bool IsMaxStringLengthType { get; init; } = isMaxStringLengthType;
	public bool IsFixedLength { get; init; } = isFixedLength;
	public bool IsGuidOnly { get; init; } = isGuidOnly;
}


public static class ProviderSqlTypeExtensions
{
    public static bool SupportsLength(this ProviderSqlType providerSqlType) =>
        !string.IsNullOrWhiteSpace(providerSqlType.FormatWithLength);

    public static bool SupportsPrecision(this ProviderSqlType providerSqlType) =>
        !string.IsNullOrWhiteSpace(providerSqlType.FormatWithPrecision);

    public static bool SupportsPrecisionAndScale(this ProviderSqlType providerSqlType) =>
        !string.IsNullOrWhiteSpace(providerSqlType.FormatWithPrecisionAndScale);
}