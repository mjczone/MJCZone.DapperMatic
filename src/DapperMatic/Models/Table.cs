namespace DapperMatic.Models;

public class Table
{
    public Table(string name, string? schema)
    {
        Name = name;
        Schema = schema;
    }

    public string Name { get; set; }
    public string? Schema { get; set; }
    public PrimaryKey? PrimaryKey { get; set; }
    public Column[] Columns { get; set; } = [];
    public UniqueConstraint[] UniqueConstraints { get; set; } = [];
    public Index[] Indexes { get; set; } = [];
    public ForeignKey[] ForeignKeys { get; set; } = [];
}
