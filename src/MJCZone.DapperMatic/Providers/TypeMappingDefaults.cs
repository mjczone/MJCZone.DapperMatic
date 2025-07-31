namespace MJCZone.DapperMatic.Providers;

/// <summary>
/// Provides default values used across all database providers for type mapping operations.
/// </summary>
public static class TypeMappingDefaults
{
    /// <summary>
    /// Default length for string/varchar columns when no length is specified.
    /// </summary>
    public const int DefaultStringLength = 255;

    /// <summary>
    /// Default length for binary/varbinary columns when no length is specified.
    /// </summary>
    public const int DefaultBinaryLength = 255;

    /// <summary>
    /// Default precision for decimal/numeric columns.
    /// </summary>
    public const int DefaultDecimalPrecision = 16;

    /// <summary>
    /// Default scale for decimal/numeric columns.
    /// </summary>
    public const int DefaultDecimalScale = 4;

    /// <summary>
    /// Default length for enum columns stored as varchar.
    /// </summary>
    public const int DefaultEnumLength = 128;

    /// <summary>
    /// String length required to store a GUID as a string.
    /// </summary>
    public const int GuidStringLength = 36;

    /// <summary>
    /// Represents maximum/unlimited length for text columns.
    /// </summary>
    public const int MaxLength = int.MaxValue;
}