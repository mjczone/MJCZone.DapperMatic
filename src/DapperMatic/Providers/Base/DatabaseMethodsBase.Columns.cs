using System.Data;
using System.Text;
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
        if (string.IsNullOrWhiteSpace(column.TableName))
            throw new ArgumentException("Table name is required", nameof(column.TableName));

        if (string.IsNullOrWhiteSpace(column.ColumnName))
            throw new ArgumentException("Column name is required", nameof(column.ColumnName));

        var table = await GetTableAsync(
                db,
                column.SchemaName,
                column.TableName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(table?.TableName))
            return false;

        if (
            table.Columns.Any(c =>
                c.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
            )
        )
            return false;

        var tableConstraints = new DxTable(table.SchemaName, table.TableName);

        // attach the existing primary key constraint if it exists to ensure that it doesn't get recreated
        if (table.PrimaryKeyConstraint != null)
            tableConstraints.PrimaryKeyConstraint = table.PrimaryKeyConstraint;

        var columnDefinitionSql = SqlInlineColumnDefinition(table, column, tableConstraints);

        var sql = new StringBuilder();
        sql.Append(
            $"ALTER TABLE {GetSchemaQualifiedIdentifierName(column.SchemaName, column.TableName)} ADD {columnDefinitionSql}"
        );

        await ExecuteAsync(db, sql.ToString(), tx).ConfigureAwait(false);

        // ONLY add the primary key constraint if it didn't exist before and if it wasn't
        // already added as part of the column definition (in which case that tableConstraints.PrimaryKeyConstraint will be null)
        // will be null.
        if (tableConstraints.PrimaryKeyConstraint != null)
        {
            await CreatePrimaryKeyConstraintIfNotExistsAsync(
                    db,
                    tableConstraints.PrimaryKeyConstraint,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var checkConstraint in tableConstraints.CheckConstraints)
        {
            await CreateCheckConstraintIfNotExistsAsync(
                db,
                checkConstraint,
                tx: tx,
                cancellationToken: cancellationToken
            );
        }

        foreach (var defaultConstraint in tableConstraints.DefaultConstraints)
        {
            await CreateDefaultConstraintIfNotExistsAsync(
                db,
                defaultConstraint,
                tx: tx,
                cancellationToken: cancellationToken
            );
        }

        foreach (var uniqueConstraint in tableConstraints.UniqueConstraints)
        {
            await CreateUniqueConstraintIfNotExistsAsync(
                db,
                uniqueConstraint,
                tx: tx,
                cancellationToken: cancellationToken
            );
        }

        foreach (var foreignKeyConstraint in tableConstraints.ForeignKeyConstraints)
        {
            await CreateForeignKeyConstraintIfNotExistsAsync(
                db,
                foreignKeyConstraint,
                tx: tx,
                cancellationToken: cancellationToken
            );
        }

        foreach (var index in tableConstraints.Indexes)
        {
            await CreateIndexIfNotExistsAsync(
                    db,
                    index,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        return true;
    }

    public virtual async Task<bool> CreateColumnIfNotExistsAsync(
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
        bool isNullable = true,
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
        return await CreateColumnIfNotExistsAsync(
                db,
                new DxColumn(
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
                    onUpdate
                ),
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

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
        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var column = table.Columns.FirstOrDefault(c =>
            c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
        );

        if (column == null)
            return false;

        // drop any related constraints
        if (column.IsPrimaryKey)
        {
            await DropPrimaryKeyConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        if (column.IsForeignKey)
        {
            await DropForeignKeyConstraintOnColumnIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    column.ColumnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        if (column.IsUnique)
        {
            await DropUniqueConstraintOnColumnIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    column.ColumnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        if (column.IsIndexed)
        {
            await DropIndexesOnColumnIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    column.ColumnName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        await DropCheckConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        await DropDefaultConstraintOnColumnIfExistsAsync(
                db,
                schemaName,
                tableName,
                columnName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        var sql = SqlDropColumn(schemaName, tableName, columnName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

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
