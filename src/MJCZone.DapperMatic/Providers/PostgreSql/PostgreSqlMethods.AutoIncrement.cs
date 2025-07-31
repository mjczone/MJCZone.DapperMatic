namespace MJCZone.DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
{
    /// <summary>
    /// Checks PostgreSQL-specific metadata for auto-increment indicators.
    /// </summary>
    /// <param name="metadata">Provider-specific metadata object.</param>
    /// <returns>True if the metadata indicates auto-increment, false otherwise.</returns>
    protected override bool CheckProviderSpecificAutoIncrement(object metadata)
    {
        // PostgreSQL uses is_identity flag or attidentity column
        return metadata switch
        {
            bool isIdentity => isIdentity,
            int isIdentity => isIdentity == 1,
            string attidentity => !string.IsNullOrWhiteSpace(attidentity),
            _ => false
        };
    }
}