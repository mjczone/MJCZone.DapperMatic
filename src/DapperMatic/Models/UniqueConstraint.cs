namespace DapperMatic.Models;

public class UniqueConstraint
{
    public UniqueConstraint(string name, string[] columnNames)
    {
        Name = name;
        Columns = columnNames;
    }

    public string Name { get; set; }
    public string[] Columns { get; set; }
}
