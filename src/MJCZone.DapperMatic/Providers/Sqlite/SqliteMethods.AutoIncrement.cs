namespace MJCZone.DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    /// <summary>
    /// Checks SQLite-specific metadata for auto-increment indicators.
    /// </summary>
    /// <param name="metadata">Provider-specific metadata object.</param>
    /// <returns>True if the metadata indicates auto-increment, false otherwise.</returns>
    protected override bool CheckProviderSpecificAutoIncrement(object metadata)
    {
        // SQLite's parser already sets IsAutoIncrement on the column during parsing
        // This is mainly here for consistency
        return metadata switch
        {
            bool isAutoIncrement => isAutoIncrement,
            int isAutoIncrement => isAutoIncrement == 1,
            _ => false
        };
    }
}