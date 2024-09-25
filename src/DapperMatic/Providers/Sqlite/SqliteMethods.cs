using System.Data;
using System.Data.Common;
using System.Text;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods : DatabaseMethodsBase, IDatabaseMethods
{
    protected override string DefaultSchema => "";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DbProviderType.Sqlite);

    internal SqliteMethods() { }

    public override async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"select sqlite_version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return SqliteSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    private async Task<bool> AlterTableUsingRecreateTableStrategyAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        Func<DxTable, bool>? validateTable,
        Func<DxTable, DxTable> updateTable,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken)
            .ConfigureAwait(false);

        if (table == null)
            return false;

        if (validateTable != null && !validateTable(table))
            return false;

        // create a temporary table with the updated schema
        var tmpTable = new DxTable(
            table.SchemaName,
            table.TableName,
            [.. table.Columns],
            table.PrimaryKeyConstraint,
            [.. table.CheckConstraints],
            [.. table.DefaultConstraints],
            [.. table.UniqueConstraints],
            [.. table.ForeignKeyConstraints],
            [.. table.Indexes]
        );
        var newTable = updateTable(tmpTable);

        await AlterTableUsingRecreateTableStrategyAsync(
            db,
            schemaName,
            table,
            newTable,
            tx,
            cancellationToken
        );

        return true;
    }

    private async Task AlterTableUsingRecreateTableStrategyAsync(
        IDbConnection db,
        string? schemaName,
        DxTable existingTable,
        DxTable updatedTable,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    )
    {
        var tableName = existingTable.TableName;
        var newTableName = $"{tableName}_temp";
        updatedTable.TableName = newTableName;

        // get the create index sql statements for the existing table
        var createIndexStatements = await GetCreateIndexSqlStatementsForTable(
                db,
                schemaName,
                tableName,
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        // disable foreign key constraints temporarily
        await ExecuteAsync(db, "PRAGMA foreign_keys = 0", tx).ConfigureAwait(false);

        var innerTx = (DbTransaction)(
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false)
        );
        try
        {
            var created = await CreateTableIfNotExistsAsync(db, updatedTable, tx, cancellationToken)
                .ConfigureAwait(false);

            if (created)
            {
                // populate the new table with the data from the old table
                var columnsToCopy = existingTable.Columns.Select(c => c.ColumnName);

                // make sure all these columns exist in the new table
                var commonColumnsBetweenBothTables = columnsToCopy.Where(c =>
                    updatedTable.Columns.Any(x =>
                        x.ColumnName.Equals(c, StringComparison.OrdinalIgnoreCase)
                    )
                );

                if (commonColumnsBetweenBothTables.Count() > 0)
                {
                    var columnsToCopyString = string.Join(", ", commonColumnsBetweenBothTables);
                    await ExecuteAsync(
                            db,
                            $@"INSERT INTO {updatedTable.TableName} ({columnsToCopyString}) SELECT {columnsToCopyString} FROM {tableName}",
                            transaction: tx
                        )
                        .ConfigureAwait(false);
                }

                // drop the old table
                await ExecuteAsync(db, $@"DROP TABLE {tableName}", transaction: tx)
                    .ConfigureAwait(false);

                // rename the new table to the old table name
                await ExecuteAsync(
                        db,
                        $@"ALTER TABLE {updatedTable.TableName} RENAME TO {tableName}",
                        transaction: tx
                    )
                    .ConfigureAwait(false);

                // add back the indexes to the new table
                foreach (var createIndexStatement in createIndexStatements)
                {
                    await ExecuteAsync(db, createIndexStatement, null, transaction: innerTx)
                        .ConfigureAwait(false);
                }

                //TODO: add back the triggers to the new table

                //TODO: add back the views to the new table

                // commit the transaction
                if (tx == null)
                {
                    await innerTx.CommitAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            if (tx == null)
            {
                await innerTx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            throw;
        }
        finally
        {
            if (tx == null)
            {
                await innerTx.DisposeAsync();
            }
            // re-enable foreign key constraints
            await ExecuteAsync(db, "PRAGMA foreign_keys = 1", tx).ConfigureAwait(false);
        }
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

        // only add the primary key here if a existing primary key is not defined
        if (isPrimaryKey && existingPrimaryKeyConstraint == null)
        {
            columnSql.Append($" CONSTRAINT pk_{tableName}_{columnName}  PRIMARY KEY");
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
            columnSql.Append($" CONSTRAINT uc_{tableName}_{columnName}  UNIQUE");
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
                    $"ix_{tableName}_{columnName}",
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
                    $" CONSTRAINT df_{tableName}_{columnName} DEFAULT {(defaultExpression.Contains(' ') ? $"({defaultExpression})" : defaultExpression)}"
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
            columnSql.Append($" CONSTRAINT ck_{tableName}_{columnName} CHECK ({checkExpression})");
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

            columnSql.Append(
                $" CONSTRAINT fk_{tableName}_{columnName}_{referencedTableName}_{referencedColumnName} FOREIGN KEY ({columnName}) REFERENCES {referencedTableName} ({referencedColumnName})"
            );
            if (onDelete.HasValue)
                columnSql.Append($" ON DELETE {onDelete.Value.ToSql()}");
            if (onUpdate.HasValue)
                columnSql.Append($" ON UPDATE {onUpdate.Value.ToSql()}");
        }

        return columnSql.ToString();
    }
}
