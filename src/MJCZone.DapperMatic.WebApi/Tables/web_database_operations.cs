namespace MJCZone.DapperMatic.WebApi.Tables;

/// <summary>
/// Represents a web database operation entity.
/// </summary>
public class web_database_operations
{
    /// <summary>
    /// Gets or sets the unique identifier for the query operation.
    /// </summary>
    public virtual Guid id { get; set; }

    /// <summary>
    /// Gets or sets the name of the query operation.
    /// </summary>
    public virtual string? name { get; set; }

    /// <summary>
    /// Gets or sets the description of the query operation.
    /// </summary>
    public virtual string? description { get; set; }

    /// <summary>
    /// Gets or sets the provider type for the database.
    /// Some operations work across multiple databases that share the same provider type, and this field denotes that.
    /// Likewise, some simpler operations may work across multiple providers, so this field is optional.
    /// </summary>
    public virtual DbProviderType? provider_type { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the tenant.
    /// Some operations work across multiple tenants, so this field is optional.
    /// </summary>
    public virtual string? tenant_identifier { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the database.
    /// Some operations work across multiple databases, so this field is optional.
    /// </summary>
    public virtual Guid? database_id { get; set; }

    /// <summary>
    /// Gets or sets a space or comma separated list of methods that this query operation supports (e.g., GET, POST, PUT, PATCH, DELETE).
    /// </summary>
    public virtual string? methods { get; set; }

    /// <summary>
    /// Gets or sets the sql statement for the query.
    /// </summary>
    public virtual string? sql_statement { get; set; }

    /// <summary>
    /// Gets or sets the result type of the query.
    /// </summary>
    public virtual OperationResultType? result_type { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the query.
    /// </summary>
    public virtual string? sql_parameters { get; set; }

    /// <summary>
    /// Gets or sets the default parameters for the query.
    /// </summary>
    public virtual string? default_sql_parameters { get; set; }

    /// <summary>
    /// Gets or sets the sample parameters for the query.
    /// </summary>
    public virtual string? sample_sql_parameters { get; set; }

    /// <summary>
    /// Gets or sets the sample results for the query.
    /// </summary>
    public virtual string? sample_results { get; set; }

    /// <summary>
    /// Gets or sets the OpenAPI operation for the query.
    /// </summary>
    public virtual string? openapi_operation { get; set; }

    /// <summary>
    /// Gets or sets the tags for the query operation.
    /// </summary>
    public virtual string? tags { get; set; }

    /// <summary>
    /// Gets or sets the management roles for the query operation.
    /// </summary>
    public virtual string? management_roles { get; set; }

    /// <summary>
    /// Gets or sets the execution roles for the query operation.
    /// </summary>
    public virtual string? execution_roles { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the query operation was created.
    /// </summary>
    public virtual DateTime? created_at { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the query operation was last updated.
    /// </summary>
    public virtual DateTime? updated_at { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who created the query operation.
    /// </summary>
    public virtual string? created_by { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who last updated the query operation.
    /// </summary>
    public virtual string? updated_by { get; set; }
}
