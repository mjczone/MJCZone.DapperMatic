using System.Collections.Concurrent;
using System.Data;
using DapperMatic.Models;
using DapperMatic.Providers;

namespace DapperMatic;

public static partial class IDbConnectionExtensions
{
    #region IDatabaseMethods
    public static string GetLastSql(this IDbConnection db)
    {
        return Database(db).GetLastSql(db);
    }

    public static (string sql, object? parameters) GetLastSqlWithParams(this IDbConnection db)
    {
        return Database(db).GetLastSqlWithParams(db);
    }

    public static async Task<string> GetDatabaseVersionAsync(
        this IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).GetDatabaseVersionAsync(db, tx, cancellationToken);
    }

    public static Type GetDotnetTypeFromSqlType(this IDbConnection db, string sqlType)
    {
        return Database(db).GetDotnetTypeFromSqlType(sqlType);
    }
    #endregion // IDatabaseMethods

    #region Private static methods
    private static IDatabaseMethods Database(this IDbConnection db)
    {
        return DatabaseMethodsFactory.GetDatabaseMethods(db);
    }
    #endregion // Private static methods

    #region IDatabaseSchemaMethods

    public static async Task<bool> SupportsSchemasAsync(
        this IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .SupportsSchemasAsync(db, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateSchemaIfNotExistsAsync(
        this IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateSchemaIfNotExistsAsync(db, schemaName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> SchemaExistsAsync(
        this IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .SchemaExistsAsync(db, schemaName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<IEnumerable<string>> GetSchemaNamesAsync(
        this IDbConnection db,
        string? schemaNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetSchemaNamesAsync(db, schemaNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropSchemaIfExistsAsync(
        this IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropSchemaIfExistsAsync(db, schemaName, tx, cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseSchemaMethods

    #region IDatabaseTableMethods

    public static async Task<bool> TableExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .TableExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateTableIfNotExistsAsync(
        this IDbConnection db,
        DxTable table,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateTableIfNotExistsAsync(db, table, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateTableIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        DxColumn[]? columns = null,
        DxPrimaryKeyConstraint? primaryKey = null,
        DxCheckConstraint[]? checkConstraints = null,
        DxDefaultConstraint[]? defaultConstraints = null,
        DxUniqueConstraint[]? uniqueConstraints = null,
        DxForeignKeyConstraint[]? foreignKeyConstraints = null,
        DxIndex[]? indexes = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateTableIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                columns,
                primaryKey,
                checkConstraints,
                defaultConstraints,
                uniqueConstraints,
                foreignKeyConstraints,
                indexes,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxTable?> GetTableAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<List<DxTable>> GetTablesAsync(
        this IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetTablesAsync(db, schemaName, tableNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetTableNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetTableNamesAsync(db, schemaName, tableNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropTableIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropTableIfExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> RenameTableIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string newTableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .RenameTableIfExistsAsync(
                db,
                schemaName,
                tableName,
                newTableName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> TruncateTableIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .TruncateTableIfExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseTableMethods

    #region IDatabaseColumnMethods

    public static async Task<DxColumn?> GetColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetColumnAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetColumnNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetColumnNamesAsync(db, schemaName, tableName, columnNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<List<DxColumn>> GetColumnsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetColumnsAsync(db, schemaName, tableName, columnNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> RenameColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        string newColumnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .RenameColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                newColumnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseColumnMethods

    #region IDatabaseCheckConstraintMethods
    public static async Task<bool> CheckConstraintExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CheckConstraintExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> CheckConstraintExistsOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CheckConstraintExistsOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> ColumnExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ColumnExistsAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateCheckConstraintIfNotExistsAsync(
        this IDbConnection db,
        DxCheckConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateCheckConstraintIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateCheckConstraintIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnName,
        string constraintName,
        string expression,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateCheckConstraintIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                constraintName,
                expression,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateColumnIfNotExistsAsync(
        this IDbConnection db,
        DxColumn column,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateColumnIfNotExistsAsync(db, column, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateColumnIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        Type dotnetType,
        string? providerDataType = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? checkExpression = null,
        string? defaultExpression = null,
        bool isNullable = false,
        bool isPrimaryKey = false,
        bool isAutoIncrement = false,
        bool isUnique = false,
        bool isIndexed = false,
        bool isForeignKey = false,
        string? referencedTableName = null,
        string? referencedColumnName = null,
        DxForeignKeyAction? onDelete = null,
        DxForeignKeyAction? onUpdate = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateColumnIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                dotnetType,
                providerDataType,
                length,
                precision,
                scale,
                checkExpression,
                defaultExpression,
                isNullable,
                isPrimaryKey,
                isAutoIncrement,
                isUnique,
                isIndexed,
                isForeignKey,
                referencedTableName,
                referencedColumnName,
                onDelete,
                onUpdate,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateDefaultConstraintIfNotExistsAsync(
        this IDbConnection db,
        DxDefaultConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateDefaultConstraintIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateDefaultConstraintIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        string constraintName,
        string expression,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateDefaultConstraintIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                constraintName,
                expression,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DefaultConstraintExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DefaultConstraintExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DefaultConstraintExistsOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DefaultConstraintExistsOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropCheckConstraintIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropCheckConstraintIfExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropCheckConstraintOnColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropCheckConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropColumnIfExistsAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropDefaultConstraintIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropDefaultConstraintIfExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropDefaultConstraintOnColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropDefaultConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxCheckConstraint?> GetCheckConstraintAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetCheckConstraintAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<string?> GetCheckConstraintNameOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetCheckConstraintNameOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetCheckConstraintNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetCheckConstraintNamesAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxCheckConstraint?> GetCheckConstraintOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetCheckConstraintOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<DxCheckConstraint>> GetCheckConstraintsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetCheckConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseCheckConstraintMethods

    #region IDatabaseDefaultConstraintMethods

    public static async Task<DxDefaultConstraint?> GetDefaultConstraintAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetDefaultConstraintAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<string?> GetDefaultConstraintNameOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetDefaultConstraintNameOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetDefaultConstraintNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetDefaultConstraintNamesAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxDefaultConstraint?> GetDefaultConstraintOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetDefaultConstraintOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<DxDefaultConstraint>> GetDefaultConstraintsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetDefaultConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseDefaultConstraintMethods

    #region IDatabaseForeignKeyConstraintMethods

    public static async Task<bool> CreateForeignKeyConstraintIfNotExistsAsync(
        this IDbConnection db,
        DxForeignKeyConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateForeignKeyConstraintIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateForeignKeyConstraintIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] sourceColumns,
        string referencedTableName,
        DxOrderedColumn[] referencedColumns,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateForeignKeyConstraintIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                sourceColumns,
                referencedTableName,
                referencedColumns,
                onDelete,
                onUpdate,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> ForeignKeyConstraintExistsOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ForeignKeyConstraintExistsOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> ForeignKeyConstraintExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ForeignKeyConstraintExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxForeignKeyConstraint?> GetForeignKeyConstraintOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeyConstraintOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxForeignKeyConstraint?> GetForeignKeyConstraintAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeyConstraintAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<DxForeignKeyConstraint>> GetForeignKeyConstraintsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeyConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<string?> GetForeignKeyConstraintNameOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeyConstraintNameOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetForeignKeyConstraintNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeyConstraintNamesAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropForeignKeyConstraintOnColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropForeignKeyConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropForeignKeyConstraintIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropForeignKeyConstraintIfExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseForeignKeyConstraintMethods

    #region IDatabaseIndexMethods

    public static async Task<bool> CreateIndexIfNotExistsAsync(
        this IDbConnection db,
        DxIndex constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateIndexIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateIndexIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        DxOrderedColumn[] columns,
        bool isUnique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateIndexIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                indexName,
                columns,
                isUnique,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> IndexExistsOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .IndexExistsOnColumnAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> IndexExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .IndexExistsAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<DxIndex?> GetIndexOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexOnColumnAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<DxIndex?> GetIndexAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<List<DxIndex>> GetIndexesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexesAsync(db, schemaName, tableName, indexNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<string?> GetIndexNameOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexNameOnColumnAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetIndexNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? indexNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexNamesAsync(db, schemaName, tableName, indexNameFilter, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropIndexOnColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropIndexOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropIndexIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string indexName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropIndexIfExistsAsync(db, schemaName, tableName, indexName, tx, cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseIndexMethods

    #region IDatabaseUniqueConstraintMethods

    public static async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        this IDbConnection db,
        DxUniqueConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateUniqueConstraintIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateUniqueConstraintIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                columns,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> UniqueConstraintExistsOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .UniqueConstraintExistsOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> UniqueConstraintExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .UniqueConstraintExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxUniqueConstraint?> GetUniqueConstraintOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<DxUniqueConstraint?> GetUniqueConstraintAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<DxUniqueConstraint>> GetUniqueConstraintsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintsAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<string?> GetUniqueConstraintNameOnColumnAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintNameOnColumnAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<List<string>> GetUniqueConstraintNamesAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string? constraintNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintNamesAsync(
                db,
                schemaName,
                tableName,
                constraintNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropUniqueConstraintOnColumnIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropUniqueConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropUniqueConstraintIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropUniqueConstraintIfExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
    #endregion // IDatabaseUniqueConstraintMethods

    #region IDatabasePrimaryKeyConstraintMethods

    public static async Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        this IDbConnection db,
        DxPrimaryKeyConstraint constraint,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreatePrimaryKeyConstraintIfNotExistsAsync(db, constraint, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CreatePrimaryKeyConstraintIfNotExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] columns,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreatePrimaryKeyConstraintIfNotExistsAsync(
                db,
                schemaName,
                tableName,
                constraintName,
                columns,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public static async Task<bool> PrimaryKeyConstraintExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .PrimaryKeyConstraintExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<DxPrimaryKeyConstraint?> GetPrimaryKeyConstraintAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetPrimaryKeyConstraintAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> DropPrimaryKeyConstraintIfExistsAsync(
        this IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropPrimaryKeyConstraintIfExistsAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
    }
    #endregion // IDatabasePrimaryKeyConstraintMethods
}
