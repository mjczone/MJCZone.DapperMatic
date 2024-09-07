using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> TableExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetTableNamesAsync(
        IDbConnection db,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates a table if it does not exist.
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tableName"></param>
    /// <param name="schemaName"></param>
    /// <param name="primaryKeyDotnetType">The type of the primary key columnName (int, short, long, Guid, string are the only valid types available)</param>
    /// <param name="primaryKeyColumnLengths">If the TPrimaryKeyDotnetType is of type string, this represents the length of the primary key columnName</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        string[]? primaryKeyColumnNames = null,
        Type[]? primaryKeyDotnetTypes = null,
        int?[]? primaryKeyColumnLengths = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropTableIfExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
