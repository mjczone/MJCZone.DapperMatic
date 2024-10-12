using System.Data;
using DapperMatic.Interfaces;
using DapperMatic.Providers.Base;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.MySql;

    public override IProviderTypeMap ProviderTypeMap => MySqlProviderTypeMap.Instance;

    protected override string DefaultSchema => "";

    public override async Task<bool> SupportsCheckConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var versionStr =
            await ExecuteScalarAsync<string>(db, "SELECT VERSION()", tx: tx).ConfigureAwait(false)
            ?? "";
        var version = ProviderUtils.ExtractVersionFromVersionString(versionStr);
        return (
                   versionStr.Contains("MariaDB", StringComparison.OrdinalIgnoreCase)
                   && version > new Version(10, 2, 1)
               )
               || version >= new Version(8, 0, 16);
    }

    public override Task<bool> SupportsOrderedKeysInConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    internal MySqlMethods() { }

    public override async Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // sample output: 8.0.27, 8.4.2
        var sql = @"SELECT VERSION()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, tx: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override char[] QuoteChars => ['`'];
}
