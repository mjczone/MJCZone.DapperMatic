namespace DapperMatic.Providers;

internal static class ProviderUtils
{
    public static string GetCheckConstraintName(string tableName, string columnName)
    {
        return "ck_".ToRawIdentifier([tableName, columnName]);
    }

    public static string GetDefaultConstraintName(string tableName, string columnName)
    {
        return "df_".ToRawIdentifier([tableName, columnName]);
    }

    public static string GetUniqueConstraintName(string tableName, params string[] columnNames)
    {
        return "uc_".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GetPrimaryKeyConstraintName(string tableName, params string[] columnNames)
    {
        return "pk_".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GetIndexName(string tableName, params string[] columnNames)
    {
        return "ix_".ToRawIdentifier([tableName, .. columnNames]);
    }

    public static string GetForeignKeyConstraintName(
        string tableName,
        string columnName,
        string refTableName,
        string refColumnName
    )
    {
        return "fk_".ToRawIdentifier([tableName, columnName, refTableName, refColumnName]);
    }

    public static string GetForeignKeyConstraintName(
        string tableName,
        string[] columnNames,
        string refTableName,
        string[] refColumnNames
    )
    {
        return "fk_".ToRawIdentifier([tableName, .. columnNames, refTableName, .. refColumnNames]);
    }
}
