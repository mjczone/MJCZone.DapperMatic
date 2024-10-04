using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.Sqlite;

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

    public override char[] QuoteChars => ['"'];
}
