namespace DapperMatic.Providers.PostgreSql;

public sealed class PostgreSqlProviderTypeMap : ProviderTypeMapBase<PostgreSqlProviderTypeMap>
{
    internal static readonly Lazy<PostgreSqlProviderTypeMap> Instance =
        new(() => new PostgreSqlProviderTypeMap());

    #region Default Provider SQL Types
    private static readonly ProviderSqlType[] DefaultProviderSqlTypes = [];
    private static readonly DotnetTypeToSqlTypeMap[] DefaultDotnetToSqlTypeMap = [];
    private static readonly SqlTypeToDotnetTypeMap[] DefaultSqlTypeToDotnetTypeMap = [];
    #endregion // Default Provider SQL Types

    internal PostgreSqlProviderTypeMap()
        : base(DefaultProviderSqlTypes, DefaultDotnetToSqlTypeMap, DefaultSqlTypeToDotnetTypeMap)
    { }
}
