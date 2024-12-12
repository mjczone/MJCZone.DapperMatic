using System.Data;
using MJCZone.DapperMatic.Providers.Base;

namespace MJCZone.DapperMatic.Providers.PostgreSql;

/// <summary>
/// Provides PostgreSQL specific database methods.
/// </summary>
public partial class PostgreSqlMethods
    : DatabaseMethodsBase<PostgreSqlProviderTypeMap>,
        IPostgreSqlMethods
{
    private static string _defaultSchema = "public";

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlMethods"/> class.
    /// </summary>
    internal PostgreSqlMethods()
        : base(DbProviderType.PostgreSql) { }

    /// <summary>
    /// Gets the characters used for quoting identifiers.
    /// </summary>
    public override char[] QuoteChars => ['"'];

    /// <summary>
    /// Gets the default schema.
    /// </summary>
    protected override string DefaultSchema => _defaultSchema;

    /// <summary>
    /// Sets the default schema.
    /// </summary>
    /// <param name="schema">The schema name.</param>
    public static void SetDefaultSchema(string schema)
    {
        _defaultSchema = schema;
    }

    /// <summary>
    /// Determines whether the database supports ordered keys in constraints.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value.</returns>
    public override Task<bool> SupportsOrderedKeysInConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Gets the database version.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="tx">The transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the database version.</returns>
    public override async Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        // sample output: PostgreSQL 15.7 (Debian 15.7-1.pgdg110+1) on x86_64-pc-linux-gnu, compiled by gcc (Debian 10.2.1-6) 10.2.1 20210110, 64-bit
        const string sql = "SELECT VERSION()";
        var versionString =
            await ExecuteScalarAsync<string>(db, sql, tx: tx, cancellationToken: cancellationToken)
                .ConfigureAwait(false) ?? string.Empty;
        return DbProviderUtils.ExtractVersionFromVersionString(versionString);
    }

    /// <summary>
    /// Normalizes the name to lowercase.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>The normalized name.</returns>
    protected override string NormalizeName(string name)
    {
        return base.NormalizeName(name).ToLowerInvariant();
    }

    /// <summary>
    /// Converts the text to a LIKE string, normalizing to lowercase.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <param name="allowedSpecialChars">The allowed special characters.</param>
    /// <returns>The LIKE string.</returns>
    protected override string ToLikeString(string text, string allowedSpecialChars = "-_.*")
    {
        return base.ToLikeString(text, allowedSpecialChars).ToLowerInvariant();
    }
}
