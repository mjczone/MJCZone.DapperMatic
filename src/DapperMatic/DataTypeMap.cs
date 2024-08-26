namespace DapperMatic;

public class DataTypeMap
{
    public Type DotnetType { get; set; } = null!;
    public string SqlType { get; set; } = null!;
    public string? SqlTypeWithLength { get; set; }
    public string? SqlTypeWithMaxLength { get; set; }
    public string? SqlTypeWithPrecisionAndScale { get; set; }
}
