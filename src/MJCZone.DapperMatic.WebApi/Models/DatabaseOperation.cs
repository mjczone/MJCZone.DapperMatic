using Microsoft.OpenApi.Models;

namespace MJCZone.DapperMatic.WebApi.Models;

/// <summary>
/// Represents an operation that queries a database.
/// </summary>
public class DatabaseOperation
{
    /// <summary>
    /// Gets or sets the unique identifier for the query operation.
    /// </summary>
    public virtual Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the query operation.
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the query operation.
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// Gets or sets the provider type for the database.
    /// Some operations work across multiple databases that share the same provider type, and this field denotes that.
    /// Likewise, some simpler operations may work across multiple providers, so this field is optional.
    /// </summary>
    public virtual DbProviderType? ProviderType { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the tenant.
    /// Some operations work across multiple tenants, so this field is optional.
    /// </summary>
    public virtual string? TenantIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the database.
    /// Some operations work across multiple databases, so this field is optional.
    /// </summary>
    public virtual Guid? DatabaseId { get; set; }

    /// <summary>
    /// Gets or sets a space or comma separated list of methods that this query operation supports (e.g., GET, POST, PUT, PATCH, DELETE).
    /// </summary>
    public virtual string? Methods { get; set; }

    /// <summary>
    /// Gets or sets the sql statement for the query.
    /// </summary>
    public virtual string? SqlStatement { get; set; }

    /// <summary>
    /// Gets or sets the result type of the query.
    /// </summary>
    public virtual OperationResultType? ResultType { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the query.
    /// </summary>
    public virtual Dictionary<string, object?>? SqlParameters { get; set; }

    /// <summary>
    /// Gets or sets the default parameters for the query.
    /// </summary>
    public virtual Dictionary<string, object?>? DefaultSqlParameters { get; set; }

    /// <summary>
    /// Gets or sets some sample parameters for the query.
    /// </summary>
    public virtual Dictionary<string, object?>? SampleSqlParameters { get; set; }

    /// <summary>
    /// Gets or sets some sample results for the query, using the sample parameters.
    /// </summary>
    public virtual object? SampleResults { get; set; }

    /// <summary>
    /// Gets or sets the openapi operation definition for the query.
    /// </summary>
    /// <remarks>
    /// This is used to generate the openapi documentation for the query.
    /// Refer to the following documentation links:
    /// - <a href="https://swagger.io/specification/">OpenAPI Specification</a>
    /// - <a href="https://swagger.io/docs/specification/v3_0/paths-and-operations/">Paths and Operations</a>
    /// - <a href="https://swagger.io/docs/specification/v3_0/parameters/">Parameters</a>
    /// - <a href="https://tools.ietf.org/html/draft-fge-json-schema-validation-00">JSON Schema Definition</a>.
    /// </remarks>
    /// <example>
    /// <code>
    /// {
    ///  "summary": "Gets a list of users",
    ///  "description": "This endpoint returns a list of users",
    ///  "operationId": "GetUsers",
    ///  "parameters": [
    ///    {
    ///      "name": "page",
    ///      "in": "query",
    ///      "description": "The page number",
    ///      "required": false,
    ///      "schema": {
    ///        "type": "integer",
    ///        "format": "int32"
    ///      }
    ///    },
    ///    {
    ///      "name": "pageSize",
    ///      "in": "query",
    ///      "description": "The page size",
    ///      "required": false,
    ///      "schema": {
    ///        "type": "integer",
    ///        "format": "int32"
    ///      }
    ///    }
    ///  ],
    ///  "responses": {
    ///    "200": {
    ///      "description": "A list of users",
    ///      "content": {
    ///        "application/json": {
    ///          "schema": {
    ///            "type": "array",
    ///            "items": {
    ///              "type": "object",
    ///              "properties": {
    ///                "Id": {
    ///                  "type": "integer",
    ///                  "format": "int32"
    ///                },
    ///                "Name": {
    ///                  "type": "string"
    ///                }
    ///              }
    ///            }
    ///          }
    ///        }
    ///      }
    ///    }
    ///  }
    /// }
    /// </code>
    /// </example>
    public virtual OpenApiOperation? OpenApiOperation { get; set; }

    /// <summary>
    /// Gets or sets any useful tags for the query that can be used when applying pre-execution parameter transforms, or post-execution result transforms.
    /// </summary>
    public virtual List<string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the management roles for the operation. This represents
    /// the roles allowed to make edits to this operation. If left null,
    /// the management roles for the database will be used.
    /// </summary>
    public List<string>? ManagementRoles { get; set; }

    /// <summary>
    /// Gets or sets the execution roles for the operation. This represents
    /// the roles allowed to execute this operation. If left null,
    /// the execution roles for the database will be used.
    /// </summary>
    public List<string>? ExecutionRoles { get; set; }

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
