using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase
{
    #region Schema Strings
    #endregion // Schema Strings

    #region Table Strings
    #endregion // Table Strings

    #region Column Strings
    protected virtual string SqlInlineAddForeignKeyConstraint(
        string? schemaName,
        string constraintName,
        string referencedTableName,
        DxOrderedColumn referencedColumn,
        DxForeignKeyAction? onDelete = null,
        DxForeignKeyAction? onUpdate = null
    )
    {
        return @$"CONSTRAINT {NormalizeName(constraintName)} REFERENCES {GetSchemaQualifiedIdentifierName(schemaName, referencedTableName)} ({NormalizeName(referencedColumn.ColumnName)})"
            + (onDelete.HasValue ? $" ON DELETE {onDelete.Value.ToSql()}" : "")
            + (onUpdate.HasValue ? $" ON UPDATE {onUpdate.Value.ToSql()}" : "");
    }
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
    protected virtual string SqlAlterTableAddForeignKeyConstraint(
        string? schemaName,
        string constraintName,
        string tableName,
        DxOrderedColumn[] columns,
        string referencedTableName,
        DxOrderedColumn[] referencedColumns,
        DxForeignKeyAction onDelete,
        DxForeignKeyAction onUpdate
    )
    {
        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);
        var schemaQualifiedReferencedTableName = GetSchemaQualifiedIdentifierName(
            schemaName,
            referencedTableName
        );
        var columnNames = columns.Select(c => NormalizeName(c.ColumnName));
        var referencedColumnNames = referencedColumns.Select(c => NormalizeName(c.ColumnName));

        return @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {NormalizeName(constraintName)} 
                    FOREIGN KEY ({string.Join(", ", columnNames)})
                        REFERENCES {schemaQualifiedReferencedTableName} ({string.Join(", ", referencedColumnNames)})
                            ON DELETE {onDelete.ToSql()}
                            ON UPDATE {onUpdate.ToSql()}
        ";
    }
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
    protected virtual string SqlDropIndex(string? schemaName, string tableName, string indexName)
    {
        return @$"DROP INDEX {NormalizeName(indexName)} ON {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }
    #endregion // Index Strings
}
