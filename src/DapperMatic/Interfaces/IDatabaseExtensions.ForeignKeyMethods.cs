using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> SupportsNamedForeignKeysAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ForeignKeyExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetForeignKeysAsync(
        IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> CreateForeignKeyIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schemaName = null,
        string onDelete = "NO ACTION",
        string onUpdate = "NO ACTION",
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropForeignKeyIfExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
