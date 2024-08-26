using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> TableExistsAsync(
        IDbConnection db,
        string table,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetTablesAsync(
        IDbConnection db,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates a table if it does not exist.
    /// </summary>
    /// <param name="db"></param>
    /// <param name="table"></param>
    /// <param name="schema"></param>
    /// <param name="primaryKeyDotnetType">The type of the primary key column (int, short, long, Guid, string are the only valid types available)</param>
    /// <param name="primaryKeyColumnLengths">If the TPrimaryKeyDotnetType is of type string, this represents the length of the primary key column</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        string table,
        string? schema = null,
        string[]? primaryKeyColumnNames = null,
        Type[]? primaryKeyDotnetTypes = null,
        int?[]? primaryKeyColumnLengths = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropTableIfExistsAsync(
        IDbConnection db,
        string table,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
