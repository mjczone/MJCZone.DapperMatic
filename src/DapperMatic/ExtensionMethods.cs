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

    public static bool IsAlphaNumeric(this char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
    }

    public static bool IsAlpha(this char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    public static string ToAlphaNumeric(this string text, string additionalAllowedCharacters = "")
    {
        // using Regex
        // var rgx = new Regex("[^a-zA-Z0-9_.]");
        // return rgx.Replace(text, "");

        // using IsLetterOrDigit (faster, BUT allows non-ASCII letters and digits)
        // char[] allowed = additionalAllowedCharacters.ToCharArray();
        // char[] arr = text.Where(c =>
        //         char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || allowed.Contains(c)
        //     )
        //     .ToArray();
        // return new string(arr);

        return String.Concat(
            Array.FindAll(
                text.ToCharArray(),
                c => c.IsAlphaNumeric() || additionalAllowedCharacters.Contains(c)
            )
        );
    }

    public static string ToAlpha(this string text, string additionalAllowedCharacters = "")
    {
        return String.Concat(
            Array.FindAll(
                text.ToCharArray(),
                c => c.IsAlpha() || additionalAllowedCharacters.Contains(c)
            )
        );
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
