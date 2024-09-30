using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods
{
    public override async Task<bool> DoesTableExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var sql =
            $@"
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = @schemaName
            AND TABLE_NAME = @tableName
            ";

        var result = await ExecuteScalarAsync<int>(
            db,
            sql,
            new { schemaName, tableName },
            transaction: tx
        );

        return result > 0;
    }

    public override async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        DxColumn[]? columns = null,
        DxPrimaryKeyConstraint? primaryKey = null,
        DxCheckConstraint[]? checkConstraints = null,
        DxDefaultConstraint[]? defaultConstraints = null,
        DxUniqueConstraint[]? uniqueConstraints = null,
        DxForeignKeyConstraint[]? foreignKeyConstraints = null,
        DxIndex[]? indexes = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        if (await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken))
        {
            return false;
        }

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        return true;
    }

    public override async Task<List<string>> GetTableNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToAlphaNumericString(tableNameFilter).Replace('*', '%');

        return await QueryAsync<string>(
                db,
                $@"
                SELECT TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND TABLE_NAME LIKE @where")}
                ORDER BY TABLE_NAME",
                new { schemaName, where },
                transaction: tx
            )
            .ConfigureAwait(false);
    }

    public override async Task<List<DxTable>> GetTablesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        var where = string.IsNullOrWhiteSpace(tableNameFilter)
            ? null
            : ToAlphaNumericString(tableNameFilter).Replace('*', '%');

        // columns
        var columnsSql =
            @$"
            SELECT
                t.TABLE_SCHEMA AS SchemaName,
                t.TABLE_NAME AS TableName,
                c.COLUMN_NAME AS ColumnName,
                c.ORDINAL_POSITION AS OrdinalPosition,
                c.COLUMN_DEFAULT AS ColumnDefault,
                IIF(LEN(ISNULL(pk.CONSTRAINT_NAME, '')) > 0, 1, 0) AS IsPrimaryKey,
                pk.CONSTRAINT_NAME AS PrimaryKeyConstraintName,
                c.IS_NULLABLE AS IsNullable,
                c.DATA_TYPE AS DataType,
                c.CHARACTER_MAXIMUM_LENGTH AS CharacterMaximumLength,
                c.CHARACTER_OCTET_LENGTH AS CharacterOctetLength,
                c.NUMERIC_PRECISION AS NumericPrecision,
                c.NUMERIC_PRECISION_RADIX AS NumericPrecisionRadix,
                c.NUMERIC_SCALE AS NumericScale,
                c.DATETIME_PRECISION AS DatetimePrecision,
                c.CHARACTER_SET_NAME AS CharacterSetName,
                c.COLLATION_NAME AS CollationName

            FROM INFORMATION_SCHEMA.TABLES t
                LEFT OUTER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_SCHEMA = c.TABLE_SCHEMA and t.TABLE_NAME = c.TABLE_NAME
                LEFT OUTER JOIN (
                    SELECT tc.TABLE_SCHEMA, tc.TABLE_NAME, ccu.COLUMN_NAME, ccu.CONSTRAINT_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                        INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ccu
                            ON tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
                    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) pk ON t.TABLE_SCHEMA = pk.TABLE_SCHEMA and t.TABLE_NAME = pk.TABLE_NAME and c.COLUMN_NAME = pk.COLUMN_NAME

            WHERE t.TABLE_TYPE = 'BASE TABLE'
                AND t.TABLE_SCHEMA = @schemaName
                {(string.IsNullOrWhiteSpace(where) ? null : " AND t.TABLE_NAME LIKE @where")}
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION
        ";
        var columnResults = await QueryAsync<(
            string SchemaName,
            string TableName,
            string ColumnName,
            int OrdinalPosition,
            string ColumnDefault,
            bool IsPrimaryKey,
            string? PrimaryKeyConstraintName,
            string IsNullable,
            string DataType,
            int? CharacterMaximumLength,
            int? CharacterOctetLength,
            int? NumericPrecision,
            int? NumericPrecisionRadix,
            int? NumericScale,
            int? DatetimePrecision,
            string? CharacterSetName,
            string? CollationName
        )>(db, columnsSql, new { schemaName, where }, transaction: tx)
            .ConfigureAwait(false);

        var tables = new List<DxTable>();

        List<DxCheckConstraint>? checkConstraints = null;
        List<DxDefaultConstraint>? defaultConstraints = null;
        List<DxUniqueConstraint>? uniqueConstraints = null;
        List<DxForeignKeyConstraint>? foreignKeyConstraints = null;
        List<DxIndex>? indexes = null;

        foreach (var tableColumns in columnResults.GroupBy(r => new { r.SchemaName, r.TableName }))
        {
            var tableName = tableColumns.Key.TableName;
            string? primaryKeyConstraintName = null;
            var primaryKeyColumnNames = new List<string>();

            var columns = new List<DxColumn>();
            foreach (var tableColumn in tableColumns)
            {
                var column = new DxColumn(
                    tableColumn.SchemaName,
                    tableColumn.TableName,
                    tableColumn.ColumnName,
                    GetDotnetTypeFromSqlType(tableColumn.DataType),
                    tableColumn.DataType,
                    tableColumn.CharacterMaximumLength,
                    tableColumn.NumericPrecision,
                    tableColumn.NumericScale,
                    // checkexpression
                    null,
                    tableColumn.ColumnDefault,
                    tableColumn.IsNullable == "YES",
                    tableColumn.IsPrimaryKey,
                    // autoincrement
                    false,
                    // isunique
                    false,
                    // isindexed
                    false,
                    // isforeignkey
                    false,
                    null,
                    null,
                    null,
                    null
                );

                columns.Add(column);
                if (column.IsPrimaryKey)
                {
                    primaryKeyColumnNames.Add(column.ColumnName);
                    if (!string.IsNullOrWhiteSpace(tableColumn.PrimaryKeyConstraintName))
                        primaryKeyConstraintName = tableColumn.PrimaryKeyConstraintName;
                }
            }

            var primaryKey =
                (
                    !string.IsNullOrWhiteSpace(primaryKeyConstraintName)
                    && primaryKeyColumnNames.Any()
                )
                    ? new DxPrimaryKeyConstraint(
                        schemaName,
                        tableName,
                        primaryKeyConstraintName,
                        primaryKeyColumnNames.Select(pkc => new DxOrderedColumn(pkc)).ToArray()
                    )
                    : null;

            var table = new DxTable(
                schemaName,
                tableName,
                [.. columns],
                primaryKey,
                checkConstraints
                    ?.Where(t =>
                        (t.SchemaName ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                        && t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray(),
                defaultConstraints
                    ?.Where(t =>
                        (t.SchemaName ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                        && t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray(),
                uniqueConstraints
                    ?.Where(t =>
                        (t.SchemaName ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                        && t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray(),
                foreignKeyConstraints
                    ?.Where(t =>
                        (t.SchemaName ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                        && t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray(),
                indexes
                    ?.Where(t =>
                        (t.SchemaName ?? "").Equals(schemaName, StringComparison.OrdinalIgnoreCase)
                        && t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToArray()
            );
            tables.Add(table);
        }

        return tables;
    }

    public override async Task<bool> RenameTableIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string newTableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(newTableName))
        {
            throw new ArgumentException("New table name is required.", nameof(newTableName));
        }

        if (
            !await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
        {
            return false;
        }

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"EXEC sp_rename '{schemaQualifiedTableName}', '{newTableName}'",
                new
                {
                    schemaName,
                    tableName,
                    newTableName
                },
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
