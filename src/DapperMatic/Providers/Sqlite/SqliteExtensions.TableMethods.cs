using System.Data;
using System.Text;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> TableExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @tableName",
                    new { tableName },
                    tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        string[]? primaryKeyColumnNames = null,
        Type[]? primaryKeyDotnetTypes = null,
        int?[]? primaryKeyColumnLengths = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await TableExistsAsync(db, tableName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        if (primaryKeyColumnNames == null || primaryKeyColumnNames.Length == 0)
        {
            // sqlite automatically sets the rowid as the primary key and autoincrements it
            await ExecuteAsync(
                    db,
                    @$"CREATE TABLE ""{tableName}"" (
                        id INTEGER NOT NULL,
                        CONSTRAINT ""pk_{tableName}_id"" PRIMARY KEY (id)
                    )
                    ",
                    transaction: tx
                )
                .ConfigureAwait(false);

            return true;
        }

        var sql = new StringBuilder();
        sql.AppendLine(@$"CREATE TABLE ""{tableName}"" (");
        var columnNamesWithOrder = new List<string>();
        for (var i = 0; i < primaryKeyColumnNames.Length; i++)
        {
            var columnArr = primaryKeyColumnNames[i].Split(' ');
            var (_, _, columnName) = NormalizeNames(schemaName, tableName, columnArr[0]);
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            columnNamesWithOrder.Add(
                columnName + (columnArr.Length > 1 ? $" {columnArr[1]}" : " ASC")
            );

            if (primaryKeyDotnetTypes != null && primaryKeyDotnetTypes.Length > i)
            {
                sql.AppendLine(
                    $"{columnName} {GetSqlTypeString(primaryKeyDotnetTypes[i], (primaryKeyColumnLengths != null && primaryKeyColumnLengths.Length > i) ? primaryKeyColumnLengths[i] : null)} NOT NULL,"
                );
            }
            else
            {
                sql.AppendLine($"[{columnName}] INTEGER NOT NULL,");
            }
        }
        sql.AppendLine(
            $"CONSTRAINT [pk_{tableName}_id] PRIMARY KEY ({string.Join(", ", columnNamesWithOrder)})"
        );
        sql.AppendLine(")");

        await ExecuteAsync(db, sql.ToString(), transaction: tx).ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<string>> GetTablesAsync(
        IDbConnection db,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return await QueryAsync<string>(
                    db,
                    "SELECT distinct name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%' ORDER BY name",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    "SELECT distinct name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%' AND name LIKE @where ORDER BY name",
                    new { where },
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
    }

    public async Task<bool> DropTableIfExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !await TableExistsAsync(db, tableName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        (_, tableName, _) = NormalizeNames(schemaName, tableName, null);

        await ExecuteAsync(db, @$"DROP TABLE ""{tableName}""", transaction: tx)
            .ConfigureAwait(false);

        return true;
    }
}
