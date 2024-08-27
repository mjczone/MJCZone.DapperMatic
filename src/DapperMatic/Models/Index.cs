namespace DapperMatic.Models;

public class Index
{
    public Index(string name, string[] columns, bool unique = false)
    {
        Name = name;
        Columns = columns;
        Unique = unique;
    }

    public string Name { get; set; }

    /// <summary>
    /// Column names (optionally, appended with ` ASC` or ` DESC`)
    /// </summary>
    public string[] Columns { get; set; }
    public bool Unique { get; set; }
}
