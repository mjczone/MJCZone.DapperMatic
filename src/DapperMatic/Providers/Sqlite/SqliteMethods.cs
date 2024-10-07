using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.Sqlite;

    internal SqliteMethods() { }

    public override async Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // sample output: 3.44.1
        var sql = $@"SELECT sqlite_version()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, transaction: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return SqliteSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    public override char[] QuoteChars => ['"'];
}
