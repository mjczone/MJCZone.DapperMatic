using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseColumnMethods
{
    public virtual async Task<bool> ColumnExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
                await GetColumnAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
                    .ConfigureAwait(false)
            ) != null;
    }

    public virtual async Task<bool> CreateColumnIfNotExistsAsync(
        IDbConnection db,
        DxColumn column,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateColumnIfNotExistsAsync(
                db,
                column.SchemaName,
                column.TableName,
                column.ColumnName,
                column.DotnetType,
                column.ProviderDataType,
                column.Length,
                column.Precision,
                column.Scale,
                column.CheckExpression,
                column.DefaultExpression,
                column.IsNullable,
                column.IsPrimaryKey,
                column.IsAutoIncrement,
                column.IsUnique,
                column.IsIndexed,
                column.IsForeignKey,
                column.ReferencedTableName,
                column.ReferencedColumnName,
                column.OnDelete,
                column.OnUpdate,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public abstract Task<bool> CreateColumnIfNotExistsAsync(
        IDbConnection db,
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
    );

    public virtual async Task<DxColumn?> GetColumnAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await GetColumnsAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
                .ConfigureAwait(false)
        ).FirstOrDefault();
    }

    public virtual async Task<List<string>> GetColumnNamesAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var columns = await GetColumnsAsync(
                db,
                schemaName,
                tableName,
                columnNameFilter,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
        return columns.Select(x => x.ColumnName).ToList();
    }

    public abstract Task<List<DxColumn>> GetColumnsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    public virtual async Task<bool> DropColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await ColumnExistsAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var compoundTableName = await SupportsSchemasAsync(db, tx, cancellationToken)
            .ConfigureAwait(false)
            ? $"{schemaName}.{tableName}"
            : tableName;

        // drop column
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {compoundTableName} DROP COLUMN {columnName}",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> RenameColumnIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string columnName,
        string newColumnName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await ColumnExistsAsync(db, schemaName, tableName, columnName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        if (
            await ColumnExistsAsync(db, schemaName, tableName, newColumnName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var compoundTableName = await SupportsSchemasAsync(db, tx, cancellationToken)
            .ConfigureAwait(false)
            ? $"{schemaName}.{tableName}"
            : tableName;

        // As of version 3.25.0 released September 2018, SQLite supports renaming columns
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {compoundTableName} 
                    RENAME COLUMN {columnName}
                            TO {newColumnName}",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
