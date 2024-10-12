namespace DapperMatic.Providers;

public class ProviderSqlType
{
    public string SqlType { get; set; } = null!;
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
}
