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
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required", nameof(tableName));

        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException("Column name is required", nameof(columnName));

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
            $"ALTER TABLE {GetSchemaQualifiedIdentifierName(schemaName, tableName)} ADD {columnSql}"
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

    private string BuildColumnDefinitionSql(
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
                $" CONSTRAINT {ProviderUtils.GeneratePrimaryKeyConstraintName(tableName, columnName)}  PRIMARY KEY"
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
                    schemaName,
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

            var foreignKeyConstraintSql = SqlInlineForeignKeyColumnConstraint(
                schemaName,
                foreignKeyConstraintName,
                referencedTableName,
                new DxOrderedColumn(referencedColumnName),
                onDelete,
                onUpdate,
                out _
            );

            columnSql.Append($" {foreignKeyConstraintSql}");
        }

        return columnSql.ToString();
    }
}
