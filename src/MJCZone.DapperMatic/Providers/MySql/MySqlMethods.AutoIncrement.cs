namespace MJCZone.DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    /// <summary>
    /// Checks MySQL-specific metadata for auto-increment indicators.
    /// </summary>
    /// <param name="metadata">Provider-specific metadata object.</param>
    /// <returns>True if the metadata indicates auto-increment, false otherwise.</returns>
    protected override bool CheckProviderSpecificAutoIncrement(object metadata)
    {
        // MySQL uses EXTRA column that contains "auto_increment"
        return metadata switch
        {
            string extra => !string.IsNullOrWhiteSpace(extra) &&
                           extra.Contains("auto_increment", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}