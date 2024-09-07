namespace DapperMatic.Models;

public class Table
{
    public Table(string name, string? schemaName)
    {
        Name = name;
        Schema = schemaName;
    }

    public string Name { get; set; }
    public string? Schema { get; set; }
    public PrimaryKey? PrimaryKey { get; set; }
    public Column[] Columns { get; set; } = [];
    public UniqueConstraint[] UniqueConstraints { get; set; } = [];
    public TableIndex[] Indexes { get; set; } = [];
    public ForeignKey[] ForeignKeys { get; set; } = [];
}
