using System.Data;
using DapperMatic.Providers.Base;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
    : DatabaseMethodsBase<PostgreSqlProviderTypeMap>,
        IPostgreSqlMethods
{
    internal PostgreSqlMethods()
        : base(DbProviderType.PostgreSql) { }

    private static string _defaultSchema = "public";
    protected override string DefaultSchema => _defaultSchema;

    public static void SetDefaultSchema(string schema)
    {
        _defaultSchema = schema;
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
        // sample output: PostgreSQL 15.7 (Debian 15.7-1.pgdg110+1) on x86_64-pc-linux-gnu, compiled by gcc (Debian 10.2.1-6) 10.2.1 20210110, 64-bit
        const string sql = "SELECT VERSION()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, tx: tx).ConfigureAwait(false) ?? "";
        return DbProviderUtils.ExtractVersionFromVersionString(versionString);
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
