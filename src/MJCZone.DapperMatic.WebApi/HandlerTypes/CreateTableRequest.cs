using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a request to create a table.
/// </summary>
public class CreateTableRequest
{
    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public required string TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public List<DmColumn>? Columns { get; set; }

    /// <summary>
    /// Gets or sets the primary key constraint of the table.
    /// </summary>
    public DmPrimaryKeyConstraint? PrimaryKeyConstraint { get; set; }

    /// <summary>
    /// Gets or sets the check constraints of the table.
    /// </summary>
    public List<DmCheckConstraint>? CheckConstraints { get; set; }

    /// <summary>
    /// Gets or sets the default constraints of the table.
    /// </summary>
    public List<DmDefaultConstraint>? DefaultConstraints { get; set; }

    /// <summary>
    /// Gets or sets the unique constraints of the table.
    /// </summary>
    public List<DmUniqueConstraint>? UniqueConstraints { get; set; }

    /// <summary>
    /// Gets or sets the foreign key constraints of the table.
    /// </summary>
    public List<DmForeignKeyConstraint>? ForeignKeyConstraints { get; set; }

    /// <summary>
    /// Gets or sets the indexes of the table.
    /// </summary>
    public List<DmIndex>? Indexes { get; set; }
}
