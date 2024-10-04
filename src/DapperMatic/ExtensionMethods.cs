using System.Text;

namespace DapperMatic;

internal static class ExtensionMethods
{
    public static string ToQuotedIdentifier(
        this string prefix,
        char quoteChar,
        params string[] identifierSegments
    )
    {
        return prefix.ToQuotedIdentifier(new[] { quoteChar }, identifierSegments);
    }

    public static string ToQuotedIdentifier(
        this string prefix,
        char[] quoteChar,
        params string[] identifierSegments
    )
    {
        if (quoteChar.Length == 0)
            return prefix.ToRawIdentifier(identifierSegments);
        if (quoteChar.Length == 1)
            return quoteChar[0] + prefix.ToRawIdentifier(identifierSegments) + quoteChar[0];

        return quoteChar[0] + prefix.ToRawIdentifier(identifierSegments) + quoteChar[1];
    }

    /// <summary>
    /// Returns a string as a valid unquoted raw SQL identifier.
    /// All non-alphanumeric characters are removed from segment string values.
    /// The segment string values are joined using an underscore.
    /// </summary>
    public static string ToRawIdentifier(this string prefix, params string[] identifierSegments)
    {
        var sb = new StringBuilder(prefix.ToAlphaNumeric("_"));
        foreach (var segment in identifierSegments)
        {
            if (string.IsNullOrWhiteSpace(segment))
                continue;

            sb.Append('_');
            sb.Append(segment.ToAlphaNumeric("_"));
        }
        return sb.ToString().Trim('_');
    }

    public static string ToAlphaNumeric(this string text, string additionalAllowedCharacters = "")
    {
        // var rgx = new Regex("[^a-zA-Z0-9_.]");
        // return rgx.Replace(text, "");
        char[] allowed = additionalAllowedCharacters.ToCharArray();
        char[] arr = text.Where(c =>
                char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || allowed.Contains(c)
            )
            .ToArray();

        return new string(arr);
    }

    /// <summary>
    /// Converts a string to snake case, e.g. "MyProperty" becomes "my_property", and "IOas_d_DEfH" becomes "i_oas_d_d_ef_h".
    /// </summary>
    public static string ToSnakeCase(this string str)
    {
        str = str.Trim();
        var sb = new StringBuilder();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (
                i > 0
                && char.IsUpper(c)
                && (char.IsLower(str[i - 1]) || (i < str.Length - 1 && char.IsLower(str[i + 1])))
            )
            {
                sb.Append('_');
            }
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
