namespace DapperMatic.Providers.SqlServer;

public sealed class SqlServerProviderTypeMap : ProviderTypeMapBase<SqlServerProviderTypeMap>
{
    internal static readonly Lazy<SqlServerProviderTypeMap> Instance =
        new(() => new SqlServerProviderTypeMap());

    #region Default Provider SQL Types
    private static readonly ProviderSqlType[] DefaultProviderSqlTypes = [];
    private static readonly DotnetTypeToSqlTypeMap[] DefaultDotnetToSqlTypeMap = [];
    private static readonly SqlTypeToDotnetTypeMap[] DefaultSqlTypeToDotnetTypeMap = [];
    #endregion // Default Provider SQL Types

    internal SqlServerProviderTypeMap()
        : base(DefaultProviderSqlTypes, DefaultDotnetToSqlTypeMap, DefaultSqlTypeToDotnetTypeMap)
    { }
}
