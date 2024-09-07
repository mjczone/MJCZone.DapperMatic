namespace DapperMatic.Models;

public class PrimaryKey
{
    public PrimaryKey(string name, string[] columnNames)
    {
        Name = name;
        Columns = columnNames;
    }

    public string Name { get; set; }
    public string[] Columns { get; set; }
}
