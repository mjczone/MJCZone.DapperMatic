using System.Data;
using DapperMatic.Interfaces;
using DapperMatic.Providers.Base;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.Sqlite;

    public override IProviderTypeMap ProviderTypeMap => SqliteProviderTypeMap.Instance.Value;

    protected override string DefaultSchema => "";

    internal SqliteMethods() { }

    public override async Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // sample output: 3.44.1
        const string sql = "SELECT sqlite_version()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, tx: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override char[] QuoteChars => ['"'];
}
