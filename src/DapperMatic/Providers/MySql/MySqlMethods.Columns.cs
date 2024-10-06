using System.Data;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override async Task<bool> CreateColumnIfNotExistsAsync(
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
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name cannot be null or empty", nameof(columnName));

        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);
        if (table == null)
            return false;

        if (
            table.Columns.Any(c =>
                c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
        )
            return false;

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var tableWithChanges = new DxTable(table.SchemaName, table.TableName);

        var columnSql = BuildColumnDefinitionSql(
            table,
            tableWithChanges,
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
        );

        var sql = new StringBuilder();
        sql.Append(
            $"ALTER TABLE {GetSchemaQualifiedTableName(schemaName, tableName)} ADD {columnSql}"
        );

        await ExecuteAsync(db, sql.ToString(), tx).ConfigureAwait(false);

        if (tableWithChanges.PrimaryKeyConstraint != null)
        {
            await CreatePrimaryKeyConstraintIfNotExistsAsync(
                    db,
                    tableWithChanges.PrimaryKeyConstraint,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var checkConstraint in tableWithChanges.CheckConstraints)
        {
            await CreateCheckConstraintIfNotExistsAsync(
                    db,
                    checkConstraint,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var defaultConstraint in tableWithChanges.DefaultConstraints)
        {
            await CreateDefaultConstraintIfNotExistsAsync(
                    db,
                    defaultConstraint,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var uniqueConstraint in tableWithChanges.UniqueConstraints)
        {
            await CreateUniqueConstraintIfNotExistsAsync(
                    db,
                    uniqueConstraint,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var foreignKeyConstraint in tableWithChanges.ForeignKeyConstraints)
        {
            await CreateForeignKeyConstraintIfNotExistsAsync(
                    db,
                    foreignKeyConstraint,
                    tx: tx,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var index in tableWithChanges.Indexes)
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

    public override async Task<bool> DropColumnIfExistsAsync(
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

        (schemaName, tableName, columnName) = NormalizeNames(schemaName, tableName, columnName);

        var sql = new StringBuilder();
        sql.Append(
            $"ALTER TABLE {GetSchemaQualifiedTableName(schemaName, tableName)} DROP COLUMN {columnName}"
        );
        await ExecuteAsync(db, sql.ToString(), tx).ConfigureAwait(false);
        return true;
    }

    private string BuildColumnDefinitionSql(
        DxTable parentTable,
        DxTable tableWithChanges,
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
        DxForeignKeyAction? onUpdate = null
    )
    {
        columnName = NormalizeName(columnName);
        var columnType = string.IsNullOrWhiteSpace(providerDataType)
            ? GetSqlTypeFromDotnetType(dotnetType, length, precision, scale)
            : providerDataType;

        var columnSql = new StringBuilder();
        columnSql.Append($"{columnName} {columnType}");

        if (isNullable)
        {
            columnSql.Append(" NULL");
        }
        else
        {
            columnSql.Append(" NOT NULL");
        }

        if (isAutoIncrement)
        {
            columnSql.Append(" AUTO_INCREMENT");
        }

        // add the primary key constraint to the table definition, instead of trying to add it as part of the column definition
        if (isPrimaryKey && parentTable.PrimaryKeyConstraint == null)
        {
            // if multiple primary key columns are added in a row, this will reset the primary key constraint
            // to include all previous primary columns, which is what we want
            DxOrderedColumn[] pkColumns =
            [
                .. tableWithChanges
                    .Columns.Where(c => c.IsPrimaryKey)
                    .Select(c => new DxOrderedColumn(c.ColumnName))
                    .ToArray(),
                new DxOrderedColumn(columnName)
            ];
            tableWithChanges.PrimaryKeyConstraint = new DxPrimaryKeyConstraint(
                DefaultSchema,
                tableWithChanges.TableName,
                ProviderUtils.GeneratePrimaryKeyConstraintName(tableWithChanges.TableName),
                pkColumns
            );
        }

        // only add unique constraints here if column is not part of an existing unique constraint
        if (
            isUnique
            && !isIndexed
            && parentTable.UniqueConstraints.All(uc =>
                !uc.Columns.Any(c =>
                    c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            tableWithChanges.UniqueConstraints.Add(
                new DxUniqueConstraint(
                    DefaultSchema,
                    tableWithChanges.TableName,
                    ProviderUtils.GenerateUniqueConstraintName(
                        tableWithChanges.TableName,
                        columnName
                    ),
                    [new DxOrderedColumn(columnName)]
                )
            );
        }

        // only add indexes here if column is not part of an existing existing index
        if (isIndexed)
        {
            if (
                parentTable.Indexes.All(ix =>
                    ix.Columns.Length > 1
                    || !ix.Columns.Any(c =>
                        c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    )
                )
            )
            {
                tableWithChanges.Indexes.Add(
                    new DxIndex(
                        DefaultSchema,
                        tableWithChanges.TableName,
                        ProviderUtils.GenerateIndexName(tableWithChanges.TableName, columnName),
                        [new DxOrderedColumn(columnName)],
                        isUnique
                    )
                );
            }
        }

        // only add default constraint here if column doesn't already have a default constraint
        if (!string.IsNullOrWhiteSpace(defaultExpression))
        {
            if (
                parentTable.DefaultConstraints.All(dc =>
                    !dc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                // MySQL doesn't allow default constraints to be named, so we just set the default instead
                // var defaultConstraintName = ProviderUtils.GetDefaultConstraintName(
                //     tableWithChanges.TableName,
                //     columnName
                // );

                defaultExpression = defaultExpression.Trim();
                var addParentheses =
                    defaultExpression.Contains(' ')
                    && !(defaultExpression.StartsWith("(") && defaultExpression.EndsWith(")"))
                    && !(defaultExpression.StartsWith("\"") && defaultExpression.EndsWith("\""))
                    && !(defaultExpression.StartsWith("'") && defaultExpression.EndsWith("'"));

                columnSql.Append(
                    $" DEFAULT {(addParentheses ? $"({defaultExpression})" : defaultExpression)}"
                );
            }
        }

        // only add check constraints here if column doesn't already have a check constraint
        if (
            !string.IsNullOrWhiteSpace(checkExpression)
            && (parentTable.CheckConstraints ?? []).All(ck =>
                string.IsNullOrWhiteSpace(ck.ColumnName)
                || !ck.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            tableWithChanges.CheckConstraints.Add(
                new DxCheckConstraint(
                    DefaultSchema,
                    tableWithChanges.TableName,
                    columnName,
                    ProviderUtils.GenerateCheckConstraintName(
                        tableWithChanges.TableName,
                        columnName
                    ),
                    checkExpression
                )
            );
        }

        // only add foreign key constraints here if separate foreign key constraints are not defined
        if (
            isForeignKey
            && !string.IsNullOrWhiteSpace(referencedTableName)
            && !string.IsNullOrWhiteSpace(referencedColumnName)
            && (
                (parentTable.ForeignKeyConstraints ?? []).All(fk =>
                    fk.SourceColumns.All(sc =>
                        !sc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    )
                )
            )
        )
        {
            referencedTableName = NormalizeName(referencedTableName);
            referencedColumnName = NormalizeName(referencedColumnName);

            var fkConstraintName = ProviderUtils.GenerateForeignKeyConstraintName(
                tableWithChanges.TableName,
                columnName,
                referencedTableName,
                referencedColumnName
            );

            tableWithChanges.ForeignKeyConstraints.Add(
                new DxForeignKeyConstraint(
                    DefaultSchema,
                    tableWithChanges.TableName,
                    fkConstraintName,
                    [new DxOrderedColumn(columnName)],
                    referencedTableName,
                    [new DxOrderedColumn(referencedColumnName)],
                    onDelete ?? DxForeignKeyAction.NoAction,
                    onUpdate ?? DxForeignKeyAction.NoAction
                )
            );
        }

        var columnSqlString = columnSql.ToString();

        Logger.LogDebug(
            "Column Definition SQL: \n{sql}\n for column '{columnName}' in table '{tableName}'",
            columnSqlString,
            columnName,
            tableWithChanges.TableName
        );

        return columnSqlString;
    }
}
