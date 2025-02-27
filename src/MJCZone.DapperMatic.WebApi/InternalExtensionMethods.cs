namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides internal extension methods for the application.
/// </summary>
internal static class InternalExtensionMethods
{
    /// <summary>
    /// Validates the specified filter expression.
    /// </summary>
    /// <param name="filter">The filter expression to validate.</param>
    /// <returns><see langword="true"/> if the filter expression is valid; otherwise, <see langword="false"/>.</returns>
    public static bool ValidateFilterExpression(this string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        // the filter can ONLY be alphanumeric, underscore, or hyphen or '*' or '?'
        return filter!.All(c =>
            char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '*' || c == '?'
        );
    }
}
