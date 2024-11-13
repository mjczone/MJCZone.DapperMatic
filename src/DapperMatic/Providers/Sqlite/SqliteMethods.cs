using System.Data;
using DapperMatic.Providers.Base;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase<SqliteMethods>, ISqliteMethods
{
    public override DbProviderType ProviderType => DbProviderType.Sqlite;

    public override IDbProviderTypeMap ProviderTypeMap => SqliteProviderTypeMap.Instance.Value;

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
        return DbProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override char[] QuoteChars => ['"'];
}
