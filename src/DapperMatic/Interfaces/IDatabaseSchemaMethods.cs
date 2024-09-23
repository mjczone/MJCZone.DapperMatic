using System.Data;

namespace DapperMatic;

public partial interface IDatabaseSchemaMethods
{
    Task<bool> SupportsSchemasAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> SchemaExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<string>> GetSchemaNamesAsync(
        IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}