namespace DapperMatic.Models;

public class DxCommand
{
    public DxCommand() { }

    public DxCommand(string sql, IDictionary<string, object?>? parameters = null)
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string? Sql { get; set; }
    public IDictionary<string, object?>? Parameters { get; set; }
}
