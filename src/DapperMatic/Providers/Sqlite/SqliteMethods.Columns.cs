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
        DxColumn column,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(column.TableName))
            throw new ArgumentException("Table name is required", nameof(column.TableName));

        if (string.IsNullOrWhiteSpace(column.ColumnName))
            throw new ArgumentException("Column name is required", nameof(column.ColumnName));

        var (_, tableName, columnName) = NormalizeNames(
            column.SchemaName,
            column.TableName,
            column.ColumnName
        );

        return await AlterTableUsingRecreateTableStrategyAsync(
                db,
                DefaultSchema,
                tableName,
                table =>
                {
                    return table.Columns.All(x =>
                        !x.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                    );
                },
                table =>
                {
                    table.Columns.Add(column);
                    return table;
                },
                tx: tx,
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
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
