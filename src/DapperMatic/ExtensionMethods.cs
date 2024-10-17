using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperMatic;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class ExtensionMethods
{
    [GeneratedRegex(@"\d+")]
    private static partial Regex ExtractNumbersRegex();

    public static int[] ExtractNumbers(this string input)
    {
        MatchCollection matches = ExtractNumbersRegex().Matches(input);

        var numbers = new List<int>();
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Value, out var number))
                numbers.Add(number);
        }

        return [.. numbers];
    }

    public static string ToQuotedIdentifier(
        this string prefix,
        char[] quoteChar,
        params string[] identifierSegments
    )
    {
        return quoteChar.Length switch
        {
            0 => prefix.ToRawIdentifier(identifierSegments),
            1 => quoteChar[0] + prefix.ToRawIdentifier(identifierSegments) + quoteChar[0],
            _ => quoteChar[0] + prefix.ToRawIdentifier(identifierSegments) + quoteChar[1]
        };
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
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9';
    }

    public static bool IsAlpha(this char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
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

        return string.Concat(
            Array.FindAll(
                text.ToCharArray(),
                c => c.IsAlphaNumeric() || additionalAllowedCharacters.Contains(c)
            )
        );
    }

    public static string ToAlpha(this string text, string additionalAllowedCharacters = "")
    {
        return string.Concat(
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
        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
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

    // create a wildcard pattern matching algorithm that accepts wildcards (*) and questions (?)
    // for example:
    // *abc* should match abc, abcd, abcdabc, etc.
    // a?c should match ac, abc, abcc, etc.
    // the method should take in a string and a wildcard pattern and return true/false whether the string
    // matches the wildcard pattern.
    /// <summary>
    /// Wildcard pattern matching algorithm. Accepts wildcards (*) and question marks (?)
    /// </summary>
    /// <param name="text">A string</param>
    /// <param name="wildcardPattern">Wildcard pattern string</param>
    /// <param name="ignoreCase">Ignore the case of the string when evaluating a match</param>
    /// <returns>bool</returns>
    public static bool IsWildcardPatternMatch(
        this string text,
        string wildcardPattern,
        bool ignoreCase = true
    )
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(wildcardPattern))
            return false;

        if (ignoreCase)
        {
            text = text.ToLowerInvariant();
            wildcardPattern = wildcardPattern.ToLowerInvariant();
        }

        var inputIndex = 0;
        var patternIndex = 0;
        var inputLength = text.Length;
        var patternLength = wildcardPattern.Length;
        var lastWildcardIndex = -1;
        var lastInputIndex = -1;

        while (inputIndex < inputLength)
        {
            if (
                patternIndex < patternLength
                && (
                    wildcardPattern[patternIndex] == '?'
                    || wildcardPattern[patternIndex] == text[inputIndex]
                )
            )
            {
                patternIndex++;
                inputIndex++;
            }
            else if (patternIndex < patternLength && wildcardPattern[patternIndex] == '*')
            {
                lastWildcardIndex = patternIndex;
                lastInputIndex = inputIndex;
                patternIndex++;
            }
            else if (lastWildcardIndex != -1)
            {
                patternIndex = lastWildcardIndex + 1;
                lastInputIndex++;
                inputIndex = lastInputIndex;
            }
            else
            {
                return false;
            }
        }

        while (patternIndex < patternLength && wildcardPattern[patternIndex] == '*')
        {
            patternIndex++;
        }

        return patternIndex == patternLength;
    }
}
