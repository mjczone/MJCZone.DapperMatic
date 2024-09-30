using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override Task<bool> CreateCheckConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnName,
        string constraintName,
        string expression,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DropCheckConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return base.DropCheckConstraintIfExistsAsync(
            db,
            schemaName,
            tableName,
            constraintName,
            tx,
            cancellationToken
        );
    }
}