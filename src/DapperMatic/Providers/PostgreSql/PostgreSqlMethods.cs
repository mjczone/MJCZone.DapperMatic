using System.Data;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.PostgreSql;
    public override bool SupportsOrderedKeysInConstraints => false;

    internal PostgreSqlMethods() { }

    public override async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"SELECT version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
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
