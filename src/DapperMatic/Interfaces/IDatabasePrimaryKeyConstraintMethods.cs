using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Interfaces;

public interface IDatabasePrimaryKeyConstraintMethods
{
    Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        DxPrimaryKeyConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> DoesPrimaryKeyConstraintExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<DxPrimaryKeyConstraint?> GetPrimaryKeyConstraintAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> DropPrimaryKeyConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
}
