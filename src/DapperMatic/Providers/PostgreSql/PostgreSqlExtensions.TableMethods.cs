using System.Data;
using System.Text;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> TableExistsAsync(
        IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT COUNT(*) FROM pg_class JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace WHERE relname = @tableName AND relkind = 'r' AND nspname = @schemaName",
                    new { schemaName, tableName }
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
                    @$"CREATE TABLE {schemaName}.{tableName} (
                    id serial NOT NULL,
                    CONSTRAINT pk_{schemaName}_{tableName}_id PRIMARY KEY (id)
                )
                "
                )
                .ConfigureAwait(false);
            return true;
        }

        var sql = new StringBuilder();
        sql.AppendLine($@"CREATE TABLE {schemaName}.{tableName} (");
        var columnNamesWithOrder = new List<string>();
        for (var i = 0; i < primaryKeyColumnNames.Length; i++)
        {
            var columnArr = primaryKeyColumnNames[i].Split(' ');
            var (_, _, columnName) = NormalizeNames(schemaName, tableName, columnArr[0]);
            if (string.IsNullOrWhiteSpace(columnName))
                continue;

            columnNamesWithOrder.Add(
                '"' + columnName + '"' // + (columnArr.Length > 1 ? $" {columnArr[1]}" : " ASC")
            );

            if (primaryKeyDotnetTypes != null && primaryKeyDotnetTypes.Length > i)
            {
                sql.AppendLine(
                    $@"{columnName} {GetSqlTypeString(primaryKeyDotnetTypes[i], (primaryKeyColumnLengths != null && primaryKeyColumnLengths.Length > i) ? primaryKeyColumnLengths[i] : null)} NOT NULL,"
                );
            }
            else
            {
                sql.AppendLine($@"{columnName} serial NOT NULL,");
            }
        }
        sql.AppendLine(
            $@"CONSTRAINT pk_{schemaName}_{tableName}_id PRIMARY KEY ({string.Join(", ", columnNamesWithOrder)})"
        );
        sql.AppendLine(")");

        await ExecuteAsync(db, sql.ToString(), transaction: tx).ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<string>> GetTableNamesAsync(
        IDbConnection db,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        if (string.IsNullOrWhiteSpace(nameFilter))
        {
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = @schemaName ORDER BY TABLE_NAME",
                    new { schemaName }
                )
                .ConfigureAwait(false);
        }
        else
        {
            var where = $"{ToAlphaNumericString(nameFilter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = @schemaName AND TABLE_NAME LIKE @where ORDER BY TABLE_NAME",
                    new { schemaName, where }
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

        await ExecuteAsync(
                db,
                $"DROP TABLE IF EXISTS \"{schemaName}\".\"{tableName}\" CASCADE",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
