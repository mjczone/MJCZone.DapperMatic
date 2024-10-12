using System.Data;
using DapperMatic.Interfaces;
using DapperMatic.Providers.Base;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.SqlServer;

    public override IProviderTypeMap ProviderTypeMap => SqlServerProviderTypeMap.Instance;

    private static string _defaultSchema = "dbo";
    protected override string DefaultSchema => _defaultSchema;

    public static void SetDefaultSchema(string schema)
    {
        _defaultSchema = schema;
    }

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
            await ExecuteScalarAsync<string>(db, sql, tx: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override char[] QuoteChars => ['[', ']'];
}
