namespace DapperMatic.Models;

public class UniqueConstraint
{
    public UniqueConstraint(string name, string[] columns)
    {
        Name = name;
        Columns = columns;
    }

    public string Name { get; set; }
    public string[] Columns { get; set; }
}
