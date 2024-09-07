using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    Task<bool> ColumnExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<string>> GetColumnsAsync(
        IDbConnection db,
        string tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> CreateColumnIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        Type dotnetType,
        // TPropertyType will determine the columnName type at runtime,
        // e.g. string will be NVARCHAR or TEXT depending on length, int will be INTEGER, etc.
        // However, the type can be overridden by specifying the type parameter.
        string? type = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? schemaName = null,
        string? defaultValue = null,
        bool nullable = true,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DropColumnIfExistsAsync(
        IDbConnection db,
        string tableName,
        string columnName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
