namespace DapperMatic.Models;

public class ForeignKey
{
    public ForeignKey(string name, string column, string referenceTable, string referenceColumn)
    {
        Name = name;
        Column = column;
        ReferenceTable = referenceTable;
        ReferenceColumn = referenceColumn;
    }

    public string Name { get; set; }
    public string Column { get; set; }
    public string ReferenceTable { get; set; }
    public string ReferenceColumn { get; set; }
    public ReferentialAction OnDelete { get; set; } = ReferentialAction.NoAction;
    public ReferentialAction OnUpdate { get; set; } = ReferentialAction.NoAction;
}
