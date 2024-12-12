using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MJCZone.DapperMatic;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Utility methods.")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Utility methods.")]
internal static partial class ExtensionMethods
{
    /// <summary>
    /// Determines if the specified type is a struct.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a struct; otherwise, false.</returns>
    public static bool IsStruct(this Type type)
    {
        return type.IsValueType && !type.IsEnum && !typeof(Delegate).IsAssignableFrom(type);
    }

    /// <summary>
    /// Returns the underlying type if the specified type is nullable.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>The underlying type if the specified type is nullable; otherwise, the original type.</returns>
    public static Type OrUnderlyingTypeIfNullable(this Type type)
    {
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            ? Nullable.GetUnderlyingType(type)!
            : type;
    }

    /// <summary>
    /// Converts an object to a dictionary of property names and values.
    /// </summary>
    /// <param name="source">The object to convert.</param>
    /// <returns>A dictionary of property names and values.</returns>
    public static IDictionary<string, object?> ToObjectDictionary(this object source)
    {
        if (source is IDictionary<string, object?> dict2)
        {
            return dict2;
        }

        // If source is already a dictionary, return it as-is
        if (source is IDictionary<string, object> dict)
        {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return dict;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }
        // Otherwise use reflection to convert the object into a dictionary
        else
        {
            var type = source.GetType();
            if (type == null)
            {
                return new Dictionary<string, object?>();
            }

            return type.GetProperties()
                .ToDictionary(
                    propInfo => propInfo.Name,
                    propInfo => propInfo.GetValue(source, null)
                );
        }
    }

    /// <summary>
    /// Gets the friendly name of the specified type.
    /// </summary>
    /// <param name="type">The type to get the friendly name for.</param>
    /// <returns>The friendly name of the type.</returns>
    public static string GetFriendlyName(this Type type)
    {
        if (type == null)
        {
            return "(Unknown Type)";
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericTypeName = type.GetGenericTypeDefinition().Name;
        var friendlyGenericTypeName = genericTypeName[..genericTypeName.LastIndexOf('`')];

        var genericArguments = type.GetGenericArguments();
        var genericArgumentNames = genericArguments.Select(GetFriendlyName).ToArray();
        var genericTypeArgumentsString = string.Join(", ", genericArgumentNames);

        return $"{friendlyGenericTypeName}<{genericTypeArgumentsString}>";
    }

    /// <summary>
    /// Gets the value of a field from an object.
    /// </summary>
    /// <typeparam name="TValue">The type of the field value.</typeparam>
    /// <param name="instance">The object instance.</param>
    /// <param name="name">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    public static TValue? GetFieldValue<TValue>(this object instance, string name)
    {
        var type = instance.GetType();
        var field = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
            )
            .FirstOrDefault(e =>
                typeof(TValue).IsAssignableFrom(e.FieldType)
                && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            );
        return (TValue?)field?.GetValue(instance) ?? default;
    }

    /// <summary>
    /// Gets the value of a property from an object.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="instance">The object instance.</param>
    /// <param name="name">The name of the property.</param>
    /// <returns>The value of the property.</returns>
    public static TValue? GetPropertyValue<TValue>(this object instance, string name)
    {
        var type = instance.GetType();
        var property = type.GetProperties(
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance
            )
            .FirstOrDefault(e =>
                typeof(TValue).IsAssignableFrom(e.PropertyType)
                && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            );
        return (TValue?)property?.GetValue(instance);
    }

    /// <summary>
    /// Tries to get the value of a field from an object.
    /// </summary>
    /// <typeparam name="TValue">The type of the field value.</typeparam>
    /// <param name="instance">The object instance.</param>
    /// <param name="name">The name of the field.</param>
    /// <param name="value">The value of the field if found; otherwise, the default value.</param>
    /// <returns>True if the field value was found; otherwise, false.</returns>
    public static bool TryGetFieldValue<TValue>(
        this object instance,
        string name,
        out TValue? value
    )
    {
        value = instance.GetFieldValue<TValue>(name);
        return value != null;
    }

    /// <summary>
    /// Tries to get the value of a property from an object.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="instance">The object instance.</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property if found; otherwise, the default value.</param>
    /// <returns>True if the property value was found; otherwise, false.</returns>
    public static bool TryGetPropertyValue<TValue>(
        this object instance,
        string name,
        out TValue? value
    )
    {
        value = instance.GetPropertyValue<TValue>(name);
        return value != null;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex ExtractNumbersRegex();

    /// <summary>
    /// Extracts numbers from a string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>An array of extracted numbers.</returns>
#pragma warning disable SA1202 // Elements should be ordered by access
    public static int[] ExtractNumbers(this string input)
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        MatchCollection matches = ExtractNumbersRegex().Matches(input);

        var numbers = new List<int>();
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Value, out var number))
            {
                numbers.Add(number);
            }
        }

        return [.. numbers];
    }

    /// <summary>
    /// Discards length, precision, and scale from a SQL type name.
    /// </summary>
    /// <param name="sqlTypeName">The SQL type name.</param>
    /// <returns>The SQL type name without length, precision, and scale.</returns>
    public static string DiscardLengthPrecisionAndScaleFromSqlTypeName(this string sqlTypeName)
    {
        // extract the type name from the sql type name where a sqlTypeName might be "time(5, 2) without time zone" and the return value would be "time without time zone",
        // it could also be "time (  122, 2 ) without time zone" and the return value would be "time without time zone
        var openIndex = sqlTypeName.IndexOf('(', StringComparison.OrdinalIgnoreCase);
        var closeIndex = sqlTypeName.IndexOf(')', StringComparison.OrdinalIgnoreCase);
        var txt = (
            openIndex > 0 && closeIndex > 0
                ? sqlTypeName.Remove(openIndex, closeIndex - openIndex + 1)
                : sqlTypeName
        ).Trim();
        while (txt.Contains("  ", StringComparison.OrdinalIgnoreCase))
        {
            txt = txt.Replace("  ", " ", StringComparison.OrdinalIgnoreCase);
        }

        return txt;
    }

    /// <summary>
    /// Converts a string to a quoted SQL identifier.
    /// </summary>
    /// <param name="prefix">The prefix for the identifier.</param>
    /// <param name="quoteChar">The quote characters.</param>
    /// <param name="identifierSegments">The identifier segments.</param>
    /// <returns>The quoted SQL identifier.</returns>
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
    /// Converts a string to a valid unquoted raw SQL identifier.
    /// </summary>
    /// <param name="prefix">The prefix for the identifier.</param>
    /// <param name="identifierSegments">The identifier segments.</param>
    /// <returns>The unquoted raw SQL identifier.</returns>
    public static string ToRawIdentifier(this string prefix, params string[] identifierSegments)
    {
        var sb = new StringBuilder(prefix.ToAlphaNumeric("_"));
        foreach (var segment in identifierSegments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                continue;
            }

            sb.Append('_');
            sb.Append(segment.ToAlphaNumeric("_"));
        }
        return sb.ToString().Trim('_');
    }

    /// <summary>
    /// Determines if a character is alphanumeric.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is alphanumeric; otherwise, false.</returns>
    public static bool IsAlphaNumeric(this char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9';
    }

    /// <summary>
    /// Determines if a character is alphabetic.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>True if the character is alphabetic; otherwise, false.</returns>
    public static bool IsAlpha(this char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    }

    /// <summary>
    /// Converts a string to an alphanumeric string.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <param name="additionalAllowedCharacters">Additional characters to allow.</param>
    /// <returns>The alphanumeric string.</returns>
    public static string ToAlphaNumeric(this string text, string additionalAllowedCharacters = "")
    {
        // using Regex
        // var rgx = new Regex("[^a-zA-Z0-9_.]");
        // return rgx.Replace(text, string.Empty, StringComparison.OrdinalIgnoreCase);

        // using IsLetterOrDigit (faster, BUT allows non-ASCII letters and digits)
        // char[] allowed = additionalAllowedCharacters.ToCharArray();
        // char[] arr = text.Where(c =>
        //         char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || allowed.Contains(c, StringComparison.OrdinalIgnoreCase)
        //     )
        //     .ToArray();
        // return new string(arr);

        return string.Concat(
            Array.FindAll(
                text.ToCharArray(),
                c =>
                    c.IsAlphaNumeric()
                    || additionalAllowedCharacters.Contains(c, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    /// <summary>
    /// Converts a string to an alphabetic string.
    /// </summary>
    /// <param name="text">The input string.</param>
    /// <param name="additionalAllowedCharacters">Additional characters to allow.</param>
    /// <returns>The alphabetic string.</returns>
    public static string ToAlpha(this string text, string additionalAllowedCharacters = "")
    {
        return string.Concat(
            Array.FindAll(
                text.ToCharArray(),
                c =>
                    c.IsAlpha()
                    || additionalAllowedCharacters.Contains(c, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    /// <summary>
    /// Determines if two strings are equal when converted to alphabetic strings.
    /// </summary>
    /// <param name="text">The first string.</param>
    /// <param name="textToDetermineMatch">The second string.</param>
    /// <param name="ignoreCase">Whether to ignore case when comparing.</param>
    /// <param name="additionalAllowedCharacters">Additional characters to allow.</param>
    /// <returns>True if the strings are equal; otherwise, false.</returns>
    public static bool EqualsAlpha(
        this string text,
        string textToDetermineMatch,
        bool ignoreCase = true,
        string additionalAllowedCharacters = ""
    )
    {
        return text.ToAlpha(additionalAllowedCharacters)
            .Equals(
                textToDetermineMatch.ToAlpha(additionalAllowedCharacters),
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            );
    }

    /// <summary>
    /// Determines if two strings are equal when converted to alphanumeric strings.
    /// </summary>
    /// <param name="text">The first string.</param>
    /// <param name="textToDetermineMatch">The second string.</param>
    /// <param name="ignoreCase">Whether to ignore case when comparing.</param>
    /// <param name="additionalAllowedCharacters">Additional characters to allow.</param>
    /// <returns>True if the strings are equal; otherwise, false.</returns>
    public static bool EqualsAlphaNumeric(
        this string text,
        string textToDetermineMatch,
        bool ignoreCase = true,
        string additionalAllowedCharacters = ""
    )
    {
        return text.ToAlphaNumeric(additionalAllowedCharacters)
            .Equals(
                textToDetermineMatch.ToAlphaNumeric(additionalAllowedCharacters),
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal
            );
    }

    /// <summary>
    /// Converts a string to snake case.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>The snake case string.</returns>
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

    /// <summary>
    /// Wildcard pattern matching algorithm. Accepts wildcards (*) and question marks (?).
    /// </summary>
    /// <param name="text">A string.</param>
    /// <param name="wildcardPattern">Wildcard pattern string.</param>
    /// <param name="ignoreCase">Ignore the case of the string when evaluating a match.</param>
    /// <returns>True if the string matches the wildcard pattern; otherwise, false.</returns>
    public static bool IsWildcardPatternMatch(
        this string text,
        string wildcardPattern,
        bool ignoreCase = true
    )
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(wildcardPattern))
        {
            return false;
        }

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
