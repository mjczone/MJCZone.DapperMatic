using System.Data;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.PostgreSql;

    public override Task<bool> SupportsOrderedKeysInConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    internal PostgreSqlMethods() { }

    public override async Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // sample output: PostgreSQL 15.7 (Debian 15.7-1.pgdg110+1) on x86_64-pc-linux-gnu, compiled by gcc (Debian 10.2.1-6) 10.2.1 20210110, 64-bit
        var sql = $@"SELECT VERSION()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, transaction: tx).ConfigureAwait(false) ?? "";
        return ProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return PostgreSqlSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    public override char[] QuoteChars => ['"'];

    /// <summary>
    /// Postgresql is case sensitive, so we need to normalize names to lowercase.
    /// </summary>
    public override string NormalizeName(string name)
    {
        return base.NormalizeName(name).ToLowerInvariant();
    }

    protected override string ToLikeString(string text, string allowedSpecialChars = "-_.*")
    {
        return base.ToLikeString(text, allowedSpecialChars).ToLowerInvariant();
    }
}
