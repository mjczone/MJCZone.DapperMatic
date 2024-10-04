using System.Data;
using System.Text;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods
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

        var additionalIndexes = new List<DxIndex>();
        var columnSql = BuildColumnDefinitionSql(
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

        var sql = new StringBuilder();
        sql.Append(
            $"ALTER TABLE {GetSchemaQualifiedTableName(schemaName, tableName)} ADD {columnSql}"
        );

        await ExecuteAsync(db, sql.ToString(), tx).ConfigureAwait(false);

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
                    columnSql.Append(" GENERATED BY DEFAULT AS IDENTITY");
            }
        }
        else if (isPrimaryKey)
        {
            columnSql.Append(
                $" CONSTRAINT {ProviderUtils.GetPrimaryKeyConstraintName(tableName, columnName)}  PRIMARY KEY"
            );
            if (isAutoIncrement)
                columnSql.Append(" GENERATED BY DEFAULT AS IDENTITY");
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
                $" CONSTRAINT {ProviderUtils.GetUniqueConstraintName(tableName, columnName)} UNIQUE"
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
                    ProviderUtils.GetIndexName(tableName, columnName),
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
                    $" CONSTRAINT {ProviderUtils.GetDefaultConstraintName(tableName, columnName)} DEFAULT {(defaultExpression.Contains(' ') ? $"({defaultExpression})" : defaultExpression)}"
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
                $" CONSTRAINT {ProviderUtils.GetCheckConstraintName(tableName, columnName)} CHECK ({checkExpression})"
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
            referencedTableName = NormalizeName(referencedTableName);
            referencedColumnName = NormalizeName(referencedColumnName);

            var foreignKeyConstraintName = ProviderUtils.GetForeignKeyConstraintName(
                tableName,
                columnName,
                referencedTableName,
                referencedColumnName
            );
            columnSql.Append(
                $" CONSTRAINT {foreignKeyConstraintName} REFERENCES {referencedTableName} ({referencedColumnName})"
            );
            if (onDelete.HasValue)
                columnSql.Append($" ON DELETE {onDelete.Value.ToSql()}");
            if (onUpdate.HasValue)
                columnSql.Append($" ON UPDATE {onUpdate.Value.ToSql()}");
        }

        var columnSqlString = columnSql.ToString();

        Logger.LogDebug(
            "Column Definition SQL: \n{sql}\n for column '{columnName}' in table '{tableName}'",
            columnSqlString,
            columnName,
            tableName
        );

        return columnSqlString;
    }
}
