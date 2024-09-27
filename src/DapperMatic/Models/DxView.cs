using System.Diagnostics.CodeAnalysis;

namespace DapperMatic.Models;

[Serializable]
public class DxView
{
    /// <summary>
    /// Used for deserialization
    /// </summary>
    public DxView() { }

    [SetsRequiredMembers]
    public DxView(string? schemaName, string viewName, string definition)
    {
        SchemaName = schemaName;
        ViewName = viewName;
        Definition = definition;
    }

    public string? SchemaName { get; set; }
    public required string ViewName { get; set; }
    public required string Definition { get; set; }
}
