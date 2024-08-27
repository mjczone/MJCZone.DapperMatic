namespace DapperMatic.Models;

public class PrimaryKey
{
    public PrimaryKey(string name, string[] columns)
    {
        Name = name;
        Columns = columns;
    }

    public string Name { get; set; }
    public string[] Columns { get; set; }
}
