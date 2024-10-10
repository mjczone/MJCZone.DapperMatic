using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase
{
    #region Schema Strings
    protected virtual string SqlCreateSchema(string schemaName)
    {
        return @$"CREATE SCHEMA {NormalizeSchemaName(schemaName)}";
    }

    protected virtual (string sql, object parameters) SqlGetSchemaNames(
        string? schemaNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(schemaNameFilter)
            ? ""
            : ToLikeString(schemaNameFilter);

        var sql =
            $@"
            SELECT SCHEMA_NAME
            FROM INFORMATION_SCHEMA.SCHEMATA
            {(string.IsNullOrWhiteSpace(where) ? "" : $"WHERE SCHEMA_NAME LIKE @where")}
            ORDER BY SCHEMA_NAME";

        return (sql, new { where });
    }

    protected virtual string SqlDropSchema(string schemaName)
    {
        return @$"DROP SCHEMA {NormalizeSchemaName(schemaName)}";
    }
    #endregion // Schema Strings

    #region Table Strings
    protected virtual (string sql, object parameters) SqlDoesTableExist(
        string? schemaName,
        string tableName
    )
    {
        var sql =
            @$"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE 
                TABLE_TYPE='BASE TABLE'
                {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND TABLE_SCHEMA = @schemaName")}
                AND TABLE_NAME = @tableName";

        return (
            sql,
            new
            {
                schemaName = NormalizeSchemaName(schemaName),
                tableName = NormalizeName(tableName)
            }
        );
    }

    protected virtual (string sql, object parameters) SqlGetTableNames(
        string? schemaName,
        string? tableNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(tableNameFilter) ? "" : ToLikeString(tableNameFilter);

        var sql =
            $@"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE 
                    TABLE_TYPE = 'BASE TABLE' 
                    AND TABLE_SCHEMA = @schemaName
                    {(string.IsNullOrWhiteSpace(where) ? null : " AND TABLE_NAME LIKE @where")}
                ORDER BY TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected virtual string SqlDropTable(string? schemaName, string tableName)
    {
        return @$"DROP TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }

    protected virtual string SqlRenameTable(
        string? schemaName,
        string tableName,
        string newTableName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} RENAME TO {NormalizeName(newTableName)}";
    }

    protected virtual string SqlTruncateTable(string? schemaName, string tableName)
    {
        return @$"TRUNCATE TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }
    #endregion // Table Strings

    #region Column Strings
    protected virtual string SqlInlineAddDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression
    )
    {
        return @$"CONSTRAINT {NormalizeName(constraintName)} DEFAULT {expression}";
    }

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

    protected virtual string SqlDropColumn(string? schemaName, string tableName, string columnName)
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP COLUMN {NormalizeName(columnName)}";
    }
    #endregion // Column Strings

    #region Check Constraint Strings
    protected virtual string SqlAlterTableAddCheckConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        string expression
    )
    {
        if (expression.Trim().StartsWith('(') && expression.Trim().EndsWith(')'))
            expression = expression.Trim().Substring(1, expression.Length - 2);

        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ADD CONSTRAINT {NormalizeName(constraintName)} CHECK ({expression})";
    }

    protected virtual string SqlDropCheckConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Check Constraint Strings

    #region Default Constraint Strings
    protected virtual string SqlAlterTableAddDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression
    )
    {
        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        return @$"
            ALTER TABLE {schemaQualifiedTableName}
                ADD CONSTRAINT {NormalizeName(constraintName)} DEFAULT {expression} FOR {NormalizeName(columnName)}
        ";
    }

    protected virtual string SqlDropDefaultConstraint(
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Default Constraint Strings

    #region Primary Key Strings
    protected virtual string SqlAlterTableAddPrimaryKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        bool supportsOrderedKeysInConstraints
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} 
                    ADD CONSTRAINT {NormalizeName(constraintName)} 
                        PRIMARY KEY ({string.Join(", ", columns.Select(c => {
                            var columnName = NormalizeName(c.ColumnName);
                            return c.Order == DxColumnOrder.Ascending
                                ? columnName
                                : $"{columnName} DESC";
                        }))})";
    }

    protected virtual string SqlDropPrimaryKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Primary Key Strings

    #region Unique Constraint Strings
    protected virtual string SqlAlterTableAddUniqueConstraint(
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        bool supportsOrderedKeysInConstraints
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} 
                    ADD CONSTRAINT {NormalizeName(constraintName)} 
                        UNIQUE ({string.Join(", ", columns.Select(c => {
                            var columnName = NormalizeName(c.ColumnName);
                            return c.Order == DxColumnOrder.Ascending
                                ? columnName
                                : $"{columnName} DESC";
                        }))})";
    }

    protected virtual string SqlDropUniqueConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
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

    protected virtual string SqlDropForeignKeyConstraint(
        string? schemaName,
        string tableName,
        string constraintName
    )
    {
        return @$"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} DROP CONSTRAINT {NormalizeName(constraintName)}";
    }
    #endregion // Foreign Key Constraint Strings

    #region Index Strings
    protected virtual string SqlCreateIndex(
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false
    )
    {
        return @$"CREATE {(isUnique ? "UNIQUE " : "")}INDEX {NormalizeName(indexName)} ON {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ({string.Join(", ", columns.Select(c => c.ToString()))})";
    }

    protected virtual string SqlDropIndex(string? schemaName, string tableName, string indexName)
    {
        return @$"DROP INDEX {NormalizeName(indexName)} ON {GetSchemaQualifiedIdentifierName(schemaName, tableName)}";
    }
    #endregion // Index Strings

    #region View Strings

    protected virtual string SqlCreateView(string? schemaName, string viewName, string definition)
    {
        return @$"CREATE VIEW {GetSchemaQualifiedIdentifierName(schemaName, viewName)} AS {definition}";
    }

    protected virtual (string sql, object parameters) SqlGetViewNames(
        string? schemaName,
        string? viewNameFilter = null
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"SELECT 
                    TABLE_NAME AS ViewName
                FROM 
                    INFORMATION_SCHEMA.VIEWS
                WHERE 
                    TABLE_NAME IS NOT NULL
                    {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND TABLE_SCHEMA = @schemaName")}
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected virtual (string sql, object parameters) SqlGetViews(
        string? schemaName,
        string? viewNameFilter
    )
    {
        var where = string.IsNullOrWhiteSpace(viewNameFilter) ? "" : ToLikeString(viewNameFilter);

        var sql =
            @$"SELECT 
                    TABLE_SCHEMA AS SchemaName
                    TABLE_NAME AS ViewName,
                    VIEW_DEFINITION AS Definition
                FROM 
                    INFORMATION_SCHEMA.VIEWS
                WHERE 
                    TABLE_NAME IS NOT NULL
                    {(string.IsNullOrWhiteSpace(schemaName) ? "" : " AND TABLE_SCHEMA = @schemaName")}
                    {(string.IsNullOrWhiteSpace(where) ? "" : " AND TABLE_NAME LIKE @where")}
                ORDER BY
                    TABLE_NAME";

        return (sql, new { schemaName = NormalizeSchemaName(schemaName), where });
    }

    protected virtual string NormalizeViewDefinition(string definition)
    {
        return definition;
    }

    protected virtual string SqlDropView(string? schemaName, string viewName)
    {
        return @$"DROP VIEW {GetSchemaQualifiedIdentifierName(schemaName, viewName)}";
    }
    #endregion // View Strings
}
