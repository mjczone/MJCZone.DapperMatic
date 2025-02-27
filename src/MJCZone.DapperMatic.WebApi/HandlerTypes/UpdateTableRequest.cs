using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.WebApi.Handlers;

/// <summary>
/// Represents a request to update a table with various modifications.
/// </summary>
public class UpdateTableRequest
{
    /// <summary>
    /// Gets or sets the new name for the table.
    /// </summary>
    public string? RenameTableTo { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of columns to be renamed, where the key is the current name and the value is the new name.
    /// </summary>
    public Dictionary<string, string>? RenameColumnsTo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to drop the primary key constraint.
    /// </summary>
    public bool? DropPrimaryKeyConstraint { get; set; }

    /// <summary>
    /// Gets or sets the list of columns to be dropped.
    /// </summary>
    public List<string>? DropColumns { get; set; }

    /// <summary>
    /// Gets or sets the list of check constraints to be dropped.
    /// </summary>
    public List<string>? DropCheckConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of default constraints to be dropped.
    /// </summary>
    public List<string>? DropDefaultConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of unique constraints to be dropped.
    /// </summary>
    public List<string>? DropUniqueConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of foreign key constraints to be dropped.
    /// </summary>
    public List<string>? DropForeignKeyConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of indexes to be dropped.
    /// </summary>
    public List<string>? DropIndexes { get; set; }

    /// <summary>
    /// Gets or sets the list of columns to be added.
    /// </summary>
    public List<DmColumn>? AddColumns { get; set; }

    /// <summary>
    /// Gets or sets the list of check constraints to be added.
    /// </summary>
    public List<DmCheckConstraint>? AddCheckConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of default constraints to be added.
    /// </summary>
    public List<DmDefaultConstraint>? AddDefaultConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of unique constraints to be added.
    /// </summary>
    public List<DmUniqueConstraint>? AddUniqueConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of foreign key constraints to be added.
    /// </summary>
    public List<DmForeignKeyConstraint>? AddForeignKeyConstraints { get; set; }

    /// <summary>
    /// Gets or sets the list of indexes to be added.
    /// </summary>
    public List<DmIndex>? AddIndexes { get; set; }
}
