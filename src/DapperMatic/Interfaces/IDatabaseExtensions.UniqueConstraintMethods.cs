using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> UniqueConstraintExistsAsync(
        IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetUniqueConstraintsAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string[] columnNames,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropUniqueConstraintIfExistsAsync(
        IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
