using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.SqlServer;

    internal SqlServerMethods() { }

    public override async Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        /*
            SELECT
            SERVERPROPERTY('Productversion') As [SQL Server Version]        --> 15.0.2000.5, 15.0.4390.2
            SERVERPROPERTY('Productlevel') As [SQL Server Build Level],     --> RTM
            SERVERPROPERTY('edition') As [SQL Server Edition]               --> Express Edition (64-bit), Developer Edition (64-bit), etc.
         */

        var sql = $@"SELECT SERVERPROPERTY('Productversion')";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, transaction: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return SqlServerSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    public override char[] QuoteChars => ['[', ']'];
}
