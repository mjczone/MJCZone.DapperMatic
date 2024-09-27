using System.Data;
using DapperMatic.Models;

namespace DapperMatic;

public partial interface IDatabaseViewMethods
{
    Task<bool> DoesViewExistAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> CreateViewIfNotExistsAsync(
        IDbConnection db,
        DxView view,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> CreateViewIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        string definition,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<DxView?> GetViewAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<List<DxView>> GetViewsAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<List<string>> GetViewNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? viewNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> DropViewIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> RenameViewIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string viewName,
        string newViewName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
