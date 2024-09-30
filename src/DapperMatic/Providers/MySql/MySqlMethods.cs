using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    protected override string DefaultSchema => "";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDbProviderDataTypeMap(DbProviderType.MySql);

    internal MySqlMethods() { }

    public override async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"SELECT VERSION()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return MySqlSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    protected override string GetSchemaQualifiedTableName(string schemaName, string tableName)
    {
        return tableName;
    }
}
