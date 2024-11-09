using System.Text;

namespace DapperMatic.Providers;

// The struct to store the parameters:
public class DbProviderDotnetTypeDescriptor
{
    public DbProviderDotnetTypeDescriptor(
        Type dotnetType,
        int? length = null,
        int? precision = null,
        int? scale = null,
        bool? autoIncrement = false,
        bool? unicode = false,
        Type[]? otherSupportedTypes = null
    )
    {
        DotnetType =
            (
                dotnetType.IsGenericType
                && dotnetType.GetGenericTypeDefinition() == typeof(Nullable<>)
            )
                ? Nullable.GetUnderlyingType(dotnetType)!
                : dotnetType;
        Length = length;
        Precision = precision;
        Scale = scale;
        AutoIncrement = autoIncrement;
        Unicode = unicode;
        this.otherSupportedTypes = otherSupportedTypes ?? [];
    }

    /// <summary>
    /// Non-nullable type used to determine or map to a recommended sql type
    /// </summary>
    public Type DotnetType { get; init; }
    public int? Length { get; init; }
    public int? Precision { get; init; }
    public int? Scale { get; init; }
    public bool? AutoIncrement { get; init; }
    public bool? Unicode { get; init; }

    private Type[] otherSupportedTypes = [];
    public Type[] AllSupportedTypes
    {
        get => [DotnetType, .. otherSupportedTypes.Where(t => t != DotnetType).Distinct()];
        set => otherSupportedTypes = value;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(DotnetType.GetFriendlyName());
        if (Length.HasValue)
        {
            sb.Append($" LENGTH({Length})");
        }
        if (Precision.HasValue)
        {
            sb.Append($" PRECISION({Precision})");
        }
        if (AutoIncrement.HasValue)
        {
            sb.Append(" AUTO_INCREMENT");
        }
        if (Unicode.HasValue)
        {
            sb.Append(" UNICODE");
        }
        return sb.ToString();
    }
}
