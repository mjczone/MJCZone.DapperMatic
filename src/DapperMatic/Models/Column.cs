namespace DapperMatic.Models;

public class Column
{
    public Column(string name, Type dotnetType)
    {
        Name = name;
        DotnetType = dotnetType;
    }

    public string Name { get; set; }
    public Type DotnetType { get; set; }
    public int? Length { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool Nullable { get; set; }
    public string? DefaultValue { get; set; }
    public bool AutoIncrement { get; set; }
    public bool PrimaryKey { get; set; }
    public bool Unique { get; set; }
    public bool Indexed { get; set; }
    public bool ForeignKey { get; set; }
    public string? ReferenceTable { get; set; }
    public string? ReferenceColumn { get; set; }
    public ReferentialAction? OnDelete { get; set; }
    public ReferentialAction? OnUpdate { get; set; }
    public string? Comment { get; set; }
}
