using System.Data;
using System.Text;
using DapperMatic.Models;

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
        await ExecuteAsync(db, alterTableSql, transaction: tx).ConfigureAwait(false);

        foreach (var index in additionalIndexes)
        {
            var indexName = NormalizeName(index.IndexName);
            var indexColumns = index.Columns.Select(c => c.ToString());
            var indexColumnNames = index.Columns.Select(c => c.ColumnName);
            // create index sql
            var createIndexSql =
                $"CREATE {(index.IsUnique ? "UNIQUE INDEX" : "INDEX")} ix_{tableName}_{string.Join('_', indexColumnNames)} ON {tableName} ({string.Join(", ", indexColumns)})";
            await ExecuteAsync(db, createIndexSql, transaction: tx).ConfigureAwait(false);
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
}
