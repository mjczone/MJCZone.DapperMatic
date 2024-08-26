using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> UniqueConstraintExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetUniqueConstraintsAsync(
        IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string[] columns,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropUniqueConstraintIfExistsAsync(
        IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
