using System.ComponentModel.DataAnnotations;
using MJCZone.DapperMatic.DataAnnotations;

namespace MJCZone.DapperMatic.WebApi.Tables;

/// <summary>
/// Represents a web database entity.
/// </summary>
[DmTable("web_databases")]
[DmIndex(true, nameof(tenant_identifier), nameof(name))]
// Slug is not required, and must only be unique in the tenant scope WHEN provided,
// which is why we are not making the slug a unique index, just keep this in mind.
[DmIndex(false, nameof(tenant_identifier), nameof(slug))]
public class web_databases
{
    /// <summary>
    /// Gets or sets the unique identifier for the database.
    /// </summary>
    [DmPrimaryKeyConstraint]
    public Guid id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [StringLength(128)]
    public string? tenant_identifier { get; set; }

    /// <summary>
    /// Gets or sets the name of the database.
    /// </summary>
    [StringLength(128)]
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slug of the database.
    /// </summary>
    [StringLength(128)]
    public string slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the database.
    /// </summary>
    [StringLength(2048)]
    public string? description { get; set; }

    /// <summary>
    /// Gets or sets the provider type of the database.
    /// </summary>
    [StringLength(128)]
    public string provider_type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the connection string vault.
    /// </summary>
    [StringLength(256)]
    public string? connection_string_vault_name { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string.
    /// Actual connection strings are stored encrypted in the connection string vault.
    /// </summary>
    [StringLength(256)]
    public string connection_string_name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the execution roles for the database.
    /// </summary>
    public string? execution_roles { get; set; }

    /// <summary>
    /// Gets or sets the management roles for the database.
    /// </summary>
    public string? management_roles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the database is active.
    /// </summary>
    public bool is_active { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the database was created.
    /// </summary>
    public DateTime created_at { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the database was last updated.
    /// </summary>
    public DateTime updated_at { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the database.
    /// </summary>
    [StringLength(128)]
    public string? created_by { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the database.
    /// </summary>
    [StringLength(128)]
    public string? updated_by { get; set; }
}
