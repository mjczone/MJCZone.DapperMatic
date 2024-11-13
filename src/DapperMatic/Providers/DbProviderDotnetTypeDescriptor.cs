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
        bool? autoIncrement = null,
        bool? unicode = null,
        Type[]? otherSupportedTypes = null
    )
    {
        DotnetType = dotnetType.OrUnderlyingTypeIfNullable();
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
    public Type[] SupportedTypes
    {
        get => [DotnetType, .. otherSupportedTypes.Where(t => t != DotnetType).Distinct()];
        set => otherSupportedTypes = value;
    }

    /// <summary>
    /// Describes the object as a string
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(DotnetType.GetFriendlyName());
        if (Length.GetValueOrDefault(0) > 0)
        {
            sb.Append($" LENGTH({Length})");
        }
        if (Precision.GetValueOrDefault(0) > 0)
        {
            sb.Append($" PRECISION({Precision})");
        }
        if (AutoIncrement.GetValueOrDefault(false) == true)
        {
            sb.Append(" AUTO_INCREMENT");
        }
        if (Unicode.GetValueOrDefault(false) == true)
        {
            sb.Append(" UNICODE");
        }
        return sb.ToString();
    }
}
