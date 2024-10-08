using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseColumnMethods
{
    public virtual async Task<bool> DoesColumnExistAsync(
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

    public virtual async Task<List<DxColumn>> GetColumnsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string? columnNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            return [];

        var filter = string.IsNullOrWhiteSpace(columnNameFilter)
            ? null
            : ToSafeString(columnNameFilter);

        return string.IsNullOrWhiteSpace(filter)
            ? table.Columns
            : table.Columns.Where(c => IsWildcardPatternMatch(c.ColumnName, filter)).ToList();
    }

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
            !await DoesColumnExistAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        // drop column
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} DROP COLUMN {columnName}",
                tx: tx
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
            !await DoesColumnExistAsync(
                    db,
                    schemaName,
                    tableName,
                    columnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        if (
            await DoesColumnExistAsync(
                    db,
                    schemaName,
                    tableName,
                    newColumnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false)
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        // As of version 3.25.0 released September 2018, SQLite supports renaming columns
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} 
                    RENAME COLUMN {columnName}
                            TO {newColumnName}",
                tx: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
