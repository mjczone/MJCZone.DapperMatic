using System.Data;
using DapperMatic.Providers.Base;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods : DatabaseMethodsBase<MySqlProviderTypeMap>, IMySqlMethods
{
    internal MySqlMethods(): base(DbProviderType.MySql) { }

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
        var version = DbProviderUtils.ExtractVersionFromVersionString(versionStr);
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
        return DbProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override char[] QuoteChars => ['`'];
}
