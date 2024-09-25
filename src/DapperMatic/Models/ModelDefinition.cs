namespace DapperMatic.Models;

[Serializable]
public class ModelDefinition
{
    public Type? Type { get; set; }
    public DxTable? Table { get; set; }
}
