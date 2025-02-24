namespace MJCZone.DapperMatic.WebApi.Models;

/// <summary>
/// A class that represents the a database connection string and provider type.
/// </summary>
public class DatabaseEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseEntry"/> class.
    /// </summary>
    public DatabaseEntry() { }

    /// <summary>
    /// Gets or sets the unique identifier for the database.
    /// </summary>
    public virtual Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier for the database.
    /// Databases can be scoped to a tenant, which is an optional field.
    /// </summary>
    public string? TenantIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the name for the database.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets an optional slug for the database.
    /// </summary>
    /// <remarks>
    /// This is optional and can be used to create more readable paths for operations.
    /// </remarks>
    /// <example>
    /// The slug will appear in queries as follows: /api/dm/operations/exec/{slug}/{operationId}.
    /// </example>
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the description for the database.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the provider type for the database.
    /// </summary>
    public DbProviderType? ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the connection string vault name for the database.
    /// </summary>
    public string? ConnectionStringVaultName { get; set; }

    /// <summary>
    /// Gets or sets the connection string name for the database.
    /// </summary>
    public string? ConnectionStringName { get; set; }

    /// <summary>
    /// Gets or sets the management roles for the database. This represents
    /// the roles allowed to manage this database and create operations for it.
    /// </summary>
    public List<string>? ManagementRoles { get; set; }

    /// <summary>
    /// Gets or sets the execution roles for the database. This represents
    /// the roles allowed to execute operations for this database.
    /// </summary>
    public List<string>? ExecutionRoles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the database is active.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the query operation was created.
    /// </summary>
    public virtual DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the query operation was last modified.
    /// </summary>
    public virtual DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the user who created the query operation.
    /// </summary>
    public virtual string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the query operation.
    /// </summary>
    public virtual string? ModifiedBy { get; set; }
}
