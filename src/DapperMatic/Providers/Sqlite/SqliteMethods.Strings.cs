namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    #region Schema Strings
    #endregion // Schema Strings

    #region Table Strings
    #endregion // Table Strings

    #region Column Strings
    #endregion // Column Strings

    #region Check Constraint Strings
    #endregion // Check Constraint Strings

    #region Default Constraint Strings
    #endregion // Default Constraint Strings

    #region Primary Key Strings
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    #endregion // Unique Constraint Strings

    #region Foreign Key Constraint Strings
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
    protected override string SqlDropIndex(string? schemaName, string tableName, string indexName)
    {
        return @$"DROP INDEX {GetSchemaQualifiedIdentifierName(schemaName, indexName)}";
    }
    #endregion // Index Strings
}
