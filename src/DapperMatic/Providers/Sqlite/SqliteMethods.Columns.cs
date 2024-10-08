using System.Data;
using System.Diagnostics;
using System.Text;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    /// <summary>
    /// The restrictions on creating a column in a SQLite database are too many.
    /// Unfortunately, we have to re-create the table in SQLite to avoid these limitations.
    /// See: https://www.sqlite.org/lang_altertable.html
    /// </summary>
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
        return await AlterTableUsingRecreateTableStrategyAsync(
                db,
                schemaName,
                tableName,
                table =>
                {
                    return table.Columns.All(x =>
                        !x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    );
                },
                table =>
                {
                    table.Columns.Add(
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
                        )
                    );
                    return table;
                },
                tx: tx,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
    }

    public async Task<bool> CreateColumnIfNotExistsAsyncAlternate(
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

        var additionalIndexes = new List<DxIndex>();

        var sql = new StringBuilder();

        sql.AppendLine($"ALTER TABLE {tableName} (");
        sql.Append($"  ADD COLUMN ");

        var colSql = BuildColumnDefinitionSql(
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
            table.PrimaryKeyConstraint,
            table.CheckConstraints?.ToArray(),
            table.DefaultConstraints?.ToArray(),
            table.UniqueConstraints?.ToArray(),
            table.ForeignKeyConstraints?.ToArray(),
            table.Indexes?.ToArray(),
            additionalIndexes
        );

        sql.Append(colSql.ToString());

        sql.AppendLine(")");
        var alterTableSql = sql.ToString();
        await ExecuteAsync(db, alterTableSql, tx: tx).ConfigureAwait(false);

        foreach (var index in additionalIndexes)
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
        return await AlterTableUsingRecreateTableStrategyAsync(
                db,
                schemaName,
                tableName,
                table =>
                {
                    return table.Columns.Any(x =>
                        x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    );
                },
                table =>
                {
                    table.Columns.RemoveAll(c =>
                        c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    );
                    return table;
                },
                tx: tx,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
    }

    private string BuildColumnDefinitionSql(
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
        // existing constraints and indexes to minimize collisions
        // ignore anything that already exists
        DxPrimaryKeyConstraint? existingPrimaryKeyConstraint = null,
        DxCheckConstraint[]? existingCheckConstraints = null,
        DxDefaultConstraint[]? existingDefaultConstraints = null,
        DxUniqueConstraint[]? existingUniqueConstraints = null,
        DxForeignKeyConstraint[]? existingForeignKeyConstraints = null,
        DxIndex[]? existingIndexes = null,
        List<DxIndex>? populateNewIndexes = null
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

        // only add the primary key here if the primary key is a single column key
        if (existingPrimaryKeyConstraint != null)
        {
            var pkColumns = existingPrimaryKeyConstraint.Columns.Select(c => c.ToString());
            var pkColumnNames = existingPrimaryKeyConstraint
                .Columns.Select(c => c.ColumnName)
                .ToArray();
            if (
                pkColumnNames.Length == 1
                && pkColumnNames.First().Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
            {
                columnSql.Append(
                    $" CONSTRAINT {existingPrimaryKeyConstraint.ConstraintName} PRIMARY KEY"
                );
                if (isAutoIncrement)
                    columnSql.Append(" AUTOINCREMENT");
            }
        }
        else if (isPrimaryKey)
        {
            columnSql.Append(
                $" CONSTRAINT {ProviderUtils.GeneratePrimaryKeyConstraintName(tableName, columnName)}  PRIMARY KEY"
            );
            if (isAutoIncrement)
                columnSql.Append(" AUTOINCREMENT");
        }

        // only add unique constraints here if column is not part of an existing unique constraint
        if (
            isUnique
            && !isIndexed
            && (existingUniqueConstraints ?? []).All(uc =>
                !uc.Columns.Any(c =>
                    c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            columnSql.Append(
                $" CONSTRAINT {ProviderUtils.GenerateUniqueConstraintName(tableName, columnName)} UNIQUE"
            );
        }

        // only add indexes here if column is not part of an existing existing index
        if (
            isIndexed
            && (existingIndexes ?? []).All(uc =>
                uc.Columns.Length > 1
                || !uc.Columns.Any(c =>
                    c.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
            )
        )
        {
            populateNewIndexes?.Add(
                new DxIndex(
                    null,
                    tableName,
                    ProviderUtils.GenerateIndexName(tableName, columnName),
                    [new DxOrderedColumn(columnName)],
                    isUnique
                )
            );
        }

        // only add default constraint here if column doesn't already have a default constraint
        if (!string.IsNullOrWhiteSpace(defaultExpression))
        {
            if (
                (existingDefaultConstraints ?? []).All(dc =>
                    !dc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                columnSql.Append(
                    $" CONSTRAINT {ProviderUtils.GenerateDefaultConstraintName(tableName, columnName)} DEFAULT {(defaultExpression.Contains(' ') ? $"({defaultExpression})" : defaultExpression)}"
                );
            }
        }

        // when using CREATE method, we need to merge default constraints into column definition sql
        // since this is the only place sqlite allows them to be added
        var defaultConstraint = (existingDefaultConstraints ?? []).FirstOrDefault(dc =>
            dc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
        );
        if (defaultConstraint != null)
        {
            columnSql.Append(
                $" CONSTRAINT {defaultConstraint.ConstraintName} DEFAULT {(defaultConstraint.Expression.Contains(' ') ? $"({defaultConstraint.Expression})" : defaultConstraint.Expression)}"
            );
        }

        // only add check constraints here if column doesn't already have a check constraint
        if (
            !string.IsNullOrWhiteSpace(checkExpression)
            && (existingCheckConstraints ?? []).All(ck =>
                string.IsNullOrWhiteSpace(ck.ColumnName)
                || !ck.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            columnSql.Append(
                $" CONSTRAINT {ProviderUtils.GenerateCheckConstraintName(tableName, columnName)} CHECK ({checkExpression})"
            );
        }

        // only add foreign key constraints here if separate foreign key constraints are not defined
        if (
            isForeignKey
            && !string.IsNullOrWhiteSpace(referencedTableName)
            && !string.IsNullOrWhiteSpace(referencedColumnName)
            && (
                (existingForeignKeyConstraints ?? []).All(fk =>
                    fk.SourceColumns.All(sc =>
                        !sc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    )
                )
            )
        )
        {
            var foreignKeyConstraintName = ProviderUtils.GenerateForeignKeyConstraintName(
                NormalizeName(tableName),
                NormalizeName(columnName),
                NormalizeName(referencedTableName),
                NormalizeName(referencedColumnName)
            );

            var foreignKeyConstraintSql = SqlInlineAddForeignKeyConstraint(
                DefaultSchema,
                foreignKeyConstraintName,
                referencedTableName,
                new DxOrderedColumn(referencedColumnName),
                onDelete,
                onUpdate
            );

            columnSql.Append($" {foreignKeyConstraintSql}");
        }

        return columnSql.ToString();
    }
}
