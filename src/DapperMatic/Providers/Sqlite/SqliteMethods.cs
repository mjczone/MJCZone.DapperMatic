using System.Data;
using System.Data.Common;
using System.Text;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase, IDatabaseMethods
{
    protected override string DefaultSchema => "";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DbProviderType.Sqlite);

    internal SqliteMethods() { }

    public override async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"select sqlite_version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return SqliteSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }
}