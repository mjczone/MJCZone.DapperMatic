using System.Data;
using System.Text;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> TableExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName, null);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = @schemaName",
                    new { schemaName, tableName },
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
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName, null);

        if (
            await TableExistsAsync(db, tableName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        if (primaryKeyColumnNames == null || primaryKeyColumnNames.Length == 0)
        {
            await ExecuteAsync(
                    db,
                    @$"CREATE TABLE [{schemaName}].[{tableName}] (
                        id INT NOT NULL IDENTITY(1,1),
                        CONSTRAINT [pk_{schemaName}_{tableName}_id] PRIMARY KEY CLUSTERED ([id] ASC)
                    )
                    ",
                    transaction: tx
                )
                .ConfigureAwait(false);
            return true;
        }

        var sql = new StringBuilder();
        sql.AppendLine($"CREATE TABLE [{schemaName}].[{tableName}] (");
        var columnNamesWithOrder = new List<string>();
        for (var i = 0; i < primaryKeyColumnNames.Length; i++)
        {
            var columnArr = primaryKeyColumnNames[i].Split(' ');
            var (_, _, columnName) = NormalizeNames(schemaName, tableName, columnArr[0]);
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            columnNamesWithOrder.Add(
                '[' + columnName + ']' + (columnArr.Length > 1 ? $" {columnArr[1]}" : " ASC")
            );

            if (primaryKeyDotnetTypes != null && primaryKeyDotnetTypes.Length > i)
            {
                sql.AppendLine(
                    $"[{columnName}] {GetSqlTypeString(primaryKeyDotnetTypes[i], (primaryKeyColumnLengths != null && primaryKeyColumnLengths.Length > i) ? primaryKeyColumnLengths[i] : null)} NOT NULL,"
                );
            }
            else
            {
                sql.AppendLine($"[{columnName}] INT NOT NULL,");
            }
        }
        sql.AppendLine(
            $"CONSTRAINT [pk_{schemaName}_{tableName}_id] PRIMARY KEY CLUSTERED ({string.Join(", ", columnNamesWithOrder)})"
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
        (schemaName, _, _) = NormalizeNames(schemaName, null, null);

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schemaName ORDER BY TABLE_NAME",
                    new { schemaName },
                    tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @schemaName AND TABLE_NAME LIKE @where ORDER BY TABLE_NAME",
                    new { schemaName, where },
                    tx
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
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName, null);

        if (
            !await TableExistsAsync(db, tableName, schemaName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        // drop the constraints on the tableName first
        var constraints = await QueryAsync<string>(
                db,
                "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = @schemaName",
                new { tableName, schemaName },
                tx
            )
            .ConfigureAwait(false);

        foreach (var constraint in constraints)
        {
            await ExecuteAsync(
                    db,
                    $"ALTER TABLE [{schemaName}].[{tableName}] DROP CONSTRAINT [{constraint}]",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }

        await ExecuteAsync(db, $"DROP TABLE [{schemaName}].[{tableName}]", transaction: tx)
            .ConfigureAwait(false);
        return true;
    }
}
