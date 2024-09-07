using System.Data;

namespace DapperMatic;

public partial interface IDatabaseExtensions
{
    string GetLastSql(IDbConnection db);
    (string sql, object? parameters) GetLastSqlWithParams(IDbConnection db);
}
