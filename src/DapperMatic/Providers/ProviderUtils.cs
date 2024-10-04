namespace DapperMatic.Providers;

internal static class ProviderUtils
{
    public static string GetCheckConstraintName(string tableName, string columnName)
    {
        return "ck".ToRawIdentifier([tableName, columnName]);
    }

    public static string GetDefaultConstraintName(string tableName, string columnName)
    {
        return "df".ToRawIdentifier([tableName, columnName]);
    }

    public static string GetUniqueConstraintName(string tableName, params string[] columnNames)
    {
        return "uc".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GetPrimaryKeyConstraintName(string tableName, params string[] columnNames)
    {
        return "pk".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GetIndexName(string tableName, params string[] columnNames)
    {
        return "ix".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GetForeignKeyConstraintName(
        string tableName,
        string columnName,
        string refTableName,
        string refColumnName
    )
    {
        return "fk".ToRawIdentifier([tableName, columnName, refTableName, refColumnName]);
    }

    public static string GetForeignKeyConstraintName(
        string tableName,
        string[] columnNames,
        string refTableName,
        string[] refColumnNames
    )
    {
        return "fk".ToRawIdentifier([tableName, .. columnNames, refTableName, .. refColumnNames]);
    }
}
