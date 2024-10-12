namespace DapperMatic.Providers;

public class ProviderDataType
{
    public ProviderDataType() { }

    public ProviderDataType(
        string sqlTypeFormat,
        Type primaryDotnetType,
        Type[] supportedDotnetTypes,
        string? sqlTypeFormaWithLength = null,
        string? sqlTypeFormatWithPrecision = null,
        string? sqlTypeFormatWithPrecisionAndScale = null,
        string? sqlTypeFormatWithMaxLength = null,
        int? defaultLength = null,
        int? defaultPrecision = null,
        int? defaultScale = null,
        Func<string, bool>? isRecommendedSqlTypeMatch = null,
        Func<Type, bool>? isRecommendedDotNetTypeMatch = null
    )
    {
        PrimaryDotnetType = primaryDotnetType;
        SupportedDotnetTypes = supportedDotnetTypes;
        SqlTypeFormat = sqlTypeFormat;
        SqlTypeWithLengthFormat = sqlTypeFormaWithLength;
        SqlTypeWithPrecisionFormat = sqlTypeFormatWithPrecision;
        SqlTypeWithPrecisionAndScaleFormat = sqlTypeFormatWithPrecisionAndScale;
        SqlTypeWithMaxLengthFormat = sqlTypeFormatWithMaxLength;
        DefaultLength = defaultLength;
        DefaultPrecision = defaultPrecision;
        DefaultScale = defaultScale;
        if (isRecommendedSqlTypeMatch != null)
            IsRecommendedSqlTypeMatch = isRecommendedSqlTypeMatch;
        if (isRecommendedDotNetTypeMatch != null)
            IsRecommendedDotNetTypeMatch = isRecommendedDotNetTypeMatch;
    }

    public bool DefaultIsRecommendedSqlTypeMatch(string sqlTypeWithLengthPrecisionOrScale)
    {
        if (sqlTypeWithLengthPrecisionOrScale.EndsWith("[]") != SqlTypeFormat.EndsWith("[]"))
            return false;

        var typeAlpha = sqlTypeWithLengthPrecisionOrScale.ToAlpha();
        return SqlTypeFormat.ToAlpha().Equals(typeAlpha, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Indicates whether this provider data type is the right one for a particular SQL type.
    /// There could be multiple provider data types that support 'varchar(255)' for example,
    /// but only one(s) that are the preferred one(s) should be used when deciding which
    /// provider data type to use.
    /// </summary>
    public Func<string, bool> IsRecommendedSqlTypeMatch
    {
        get
        {
            recommendedSqlTypeMatch ??= DefaultIsRecommendedSqlTypeMatch;
            return recommendedSqlTypeMatch;
        }
        set => recommendedSqlTypeMatch = value;
    }
    private Func<string, bool>? recommendedSqlTypeMatch = null;

    /// <summary>
    /// Indicates whether this provider data type is the right one for a particular .NET type.
    /// There could be multiple provider data types that support 'typeof(string)' for example,
    /// but only one(s) that are the preferred one(s) should be used when deciding which
    ///
    public Func<Type, bool> IsRecommendedDotNetTypeMatch { get; set; } = (x) => false;

    private ProviderSqlType DefaultSqlDataTypeParser(string sqlTypeWithLengthPrecisionOrScale)
    {
        var sqlDataType = new ProviderSqlType { SqlType = sqlTypeWithLengthPrecisionOrScale };

        var parts = sqlTypeWithLengthPrecisionOrScale.Split(
            new[] { '(', ')' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (parts.Length > 1)
        {
            if (SupportsLength)
            {
                if (int.TryParse(parts[1], out var length))
                    sqlDataType.Length = length;
            }
            else if (SupportsPrecision)
            {
                var csv = parts[1].Split(',');

                if (int.TryParse(csv[0], out var precision))
                    sqlDataType.Precision = precision;

                if (SupportsScale && csv.Length > 1 && int.TryParse(csv[1], out var scale))
                    sqlDataType.Scale = scale;
            }
        }

        return sqlDataType;
    }

    private Func<string, ProviderSqlType>? parseSqlType = null;
    public Func<string, ProviderSqlType> ParseSqlType
    {
        get
        {
            parseSqlType ??= DefaultSqlDataTypeParser;
            return parseSqlType;
        }
        set => parseSqlType = value;
    }

    /// <summary>
    /// This is the primary .NET type to use for this SQL type.
    ///
    /// Do not use this property as a discriminator to determine if this
    /// provider data type is the right one for:
    /// - a particular SQL type
    /// - a particular .NET type
    ///
    /// Use the 'IsRecommendedSqlTypeMatch' and 'IsRecommendedDotNetTypeMatch'
    /// predicate properties for that.
    /// </summary>
    public Type PrimaryDotnetType { get; set; } = null!;

    /// <summary>
    /// The .NET types that are supported by this SQL type.
    /// </summary>
    public Type[] SupportedDotnetTypes { get; set; } = null!;

    /// <summary>
    /// The type format string for the SQL type, WITHOUT any length, precision, or scale.
    /// </summary>
    public string SqlTypeFormat { get; set; } = null!;

    /// <summary>
    /// The type format string for the SQL type, WITH length.
    /// </summary>
    public string? SqlTypeWithLengthFormat { get; set; }

    /// <summary>
    /// The type format string for the SQL type, WITH MAX length (int.MaxValue).
    /// </summary>
    public string? SqlTypeWithMaxLengthFormat { get; set; }

    /// <summary>
    /// The type format string for the SQL type, WITH precision.
    /// </summary>
    public string? SqlTypeWithPrecisionFormat { get; set; }

    /// <summary>
    /// The type format string for the SQL type, WITH precision and scale.
    /// </summary>
    public string? SqlTypeWithPrecisionAndScaleFormat { get; set; }

    /// <summary>
    /// This indicates whether the SQL type supports a length.
    /// </summary>
    public bool SupportsLength => !string.IsNullOrWhiteSpace(SqlTypeWithLengthFormat);

    /// <summary>
    /// This indicates whether the SQL type supports a precision.
    /// </summary>
    public bool SupportsPrecision => !string.IsNullOrWhiteSpace(SqlTypeWithPrecisionFormat);

    /// <summary>
    /// This indicates whether the SQL type supports a scale.
    /// </summary>
    public bool SupportsScale => !string.IsNullOrWhiteSpace(SqlTypeWithPrecisionAndScaleFormat);

    /// <summary>
    /// The default length for this SQL type, if not specified.
    /// </summary>
    public int? DefaultLength { get; set; }

    /// <summary>
    /// The default precision for this SQL type.
    /// </summary>
    public int? DefaultPrecision { get; set; }

    /// <summary>
    /// The default scale for this SQL type.
    /// </summary>
    public int? DefaultScale { get; set; }
}
