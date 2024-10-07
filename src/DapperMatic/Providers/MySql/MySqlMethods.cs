using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.MySql;

    public override async Task<bool> SupportsCheckConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var versionStr =
            await ExecuteScalarAsync<string>(db, "SELECT VERSION()", transaction: tx)
                .ConfigureAwait(false) ?? "";
        var version = ProviderUtils.ExtractVersionFromVersionString(versionStr);
        return (
            (
                versionStr.Contains("MariaDB", StringComparison.OrdinalIgnoreCase)
                && version > new Version(10, 2, 1)
            )
            || version >= new Version(8, 0, 16)
        );
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
        var sql = $@"SELECT VERSION()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, transaction: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return MySqlSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    public override char[] QuoteChars => ['`'];
}
