namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Utility class for working with file paths.
/// </summary>
internal static class PathUtils
{
    /// <summary>
    /// Normalizes the path.
    /// </summary>
    /// <param name="path">The default DapperMatic connection strings vault file name.</param>
    /// <returns>The normalized path.</returns>
    internal static string? NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        // fix the path separators
        path = path.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(path);
    }
}
