namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a request to create a new database schema.
/// </summary>
public class CreateSchemaRequest
{
    /// <summary>
    /// Gets or sets the name of the schema to create.
    /// </summary>
    public string? SchemaName { get; set; }
}
