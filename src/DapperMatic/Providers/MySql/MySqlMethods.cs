using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.MySql;
    protected override string DefaultSchema => "";

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
