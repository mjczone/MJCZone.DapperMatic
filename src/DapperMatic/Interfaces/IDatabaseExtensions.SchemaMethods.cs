using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> SupportsSchemasAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> SchemaExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetSchemasAsync(
        IDbConnection db,
        string? filter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
