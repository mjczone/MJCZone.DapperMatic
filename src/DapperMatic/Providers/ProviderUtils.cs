using System.Text.RegularExpressions;

namespace DapperMatic.Providers;

public static class ProviderUtils
{
    public static string GenerateCheckConstraintName(string tableName, string columnName)
    {
        return "ck".ToRawIdentifier([tableName, columnName]);
    }

    public static string GenerateDefaultConstraintName(string tableName, string columnName)
    {
        return "df".ToRawIdentifier([tableName, columnName]);
    }

    public static string GenerateUniqueConstraintName(string tableName, params string[] columnNames)
    {
        return "uc".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GeneratePrimaryKeyConstraintName(
        string tableName,
        params string[] columnNames
    )
    {
        return "pk".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GenerateIndexName(string tableName, params string[] columnNames)
    {
        return "ix".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GenerateForeignKeyConstraintName(
        string tableName,
        string columnName,
        string refTableName,
        string refColumnName
    )
    {
        return "fk".ToRawIdentifier([tableName, columnName, refTableName, refColumnName]);
    }

    public static string GenerateForeignKeyConstraintName(
        string tableName,
        string[] columnNames,
        string refTableName,
        string[] refColumnNames
    )
    {
        return "fk".ToRawIdentifier([tableName, .. columnNames, refTableName, .. refColumnNames]);
    }

    static readonly Regex pattern = new(@"\d+(\.\d+)+");

    public static Version ExtractVersionFromVersionString(string versionString)
    {
        var m = pattern.Match(versionString);
        var version = m.Value;
        return Version.TryParse(version, out var vs)
            ? vs
            : throw new ArgumentException(
                $"Could not extract version from: {versionString}",
                nameof(versionString)
            );
    }
}
