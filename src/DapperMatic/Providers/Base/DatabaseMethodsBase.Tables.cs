using System.Data;
using System.Text;
using DapperMatic.Interfaces;
using DapperMatic.Models;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase : IDatabaseTableMethods
{
    public virtual async Task<bool> DoesTableExistAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (sql, parameters) = SqlDoesTableExist(schemaName, tableName);

        var result = await ExecuteScalarAsync<int>(db, sql, parameters, tx: tx)
            .ConfigureAwait(false);

        return result > 0;
    }

    public virtual async Task<bool> CreateTablesIfNotExistsAsync(
        IDbConnection db,
        DxTable[] tables,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var afterAllTablesConstraints = new List<DxTable>();

        foreach (var table in tables)
        {
            var created = await CreateTableIfNotExistsAsync(
                    db,
                    table,
                    afterAllTablesConstraints,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);

            if (!created)
                return false;
        }

        // Add foreign keys AFTER all tables are created
        foreach (
            var foreignKeyConstraint in afterAllTablesConstraints.SelectMany(x =>
                x.ForeignKeyConstraints
            )
        )
        {
            await CreateForeignKeyConstraintIfNotExistsAsync(
                db,
                foreignKeyConstraint,
                tx: tx,
                cancellationToken: cancellationToken
            );
        }

        return true;
    }

    public virtual async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        DxTable table,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateTableIfNotExistsAsync(db, table, null, tx, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="db"></param>
    /// <param name="table"></param>
    /// <param name="afterAllTablesConstraints">If NULL, then the foreign keys will get added inline, or as table constraints, otherwise, if a list is passed, they'll get processed outside this function.</param>
    /// <param name="tx"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        DxTable table,
        List<DxTable>? afterAllTablesConstraints,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(table.TableName))
        {
            throw new ArgumentException("Table name is required.", nameof(table.TableName));
        }

        if (table.Columns == null || table.Columns.Count == 0)
        {
            throw new ArgumentException("At least one column is required.", nameof(table.Columns));
        }

        if (
            await DoesTableExistAsync(db, table.SchemaName, table.TableName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var dbVersion = await GetDatabaseVersionAsync(db, tx, cancellationToken)
            .ConfigureAwait(false);

        var supportsOrderedKeysInConstraints = await SupportsOrderedKeysInConstraintsAsync(
                db,
                tx: tx,
                cancellationToken
            )
            .ConfigureAwait(false);

        var (schemaName, tableName, _) = NormalizeNames(table.SchemaName, table.TableName);

        var sql = new StringBuilder();
        sql.Append(
            $"CREATE TABLE {GetSchemaQualifiedIdentifierName(table.SchemaName, table.TableName)} ("
        );

        var tableConstraints = new DxTable(
            schemaName,
            tableName,
            [],
            table.PrimaryKeyConstraint,
            [.. table.CheckConstraints],
            [.. table.DefaultConstraints],
            [.. table.UniqueConstraints],
            [.. table.ForeignKeyConstraints],
            [.. table.Indexes]
        );

        afterAllTablesConstraints?.Add(tableConstraints);

        // if there are multiple columns with a primary key,
        // we need to add the primary key as a constraint, and not inline
        // with the column definition.
        if (table.PrimaryKeyConstraint == null && table.Columns.Count(c => c.IsPrimaryKey) > 1)
        {
            var pkColumns = table.Columns.Where(c => c.IsPrimaryKey).ToArray();
            var pkConstraintName = ProviderUtils.GeneratePrimaryKeyConstraintName(
                table.TableName,
                pkColumns.Select(c => c.ColumnName).ToArray()
            );

            // The column definition builder will detect the primary key constraint is
            // already added and disregard adding it again.
            tableConstraints.PrimaryKeyConstraint = new DxPrimaryKeyConstraint(
                table.SchemaName,
                table.TableName,
                pkConstraintName,
                [.. pkColumns.Select(c => new DxOrderedColumn(c.ColumnName))]
            );
        }

        for (var i = 0; i < table.Columns.Count; i++)
        {
            sql.AppendLine();
            sql.Append(i == 0 ? "  " : "  , ");

            var column = table.Columns[i];

            if (afterAllTablesConstraints != null)
            {
                // the caller of this function wants to process the foreign keys
                // outside this function.
                if (
                    column.IsForeignKey
                    && !string.IsNullOrWhiteSpace(column.ReferencedTableName)
                    && !string.IsNullOrWhiteSpace(column.ReferencedColumnName)
                    && tableConstraints.ForeignKeyConstraints.All(fk =>
                        !fk.SourceColumns.Any(c =>
                            c.ColumnName.Equals(
                                column.ColumnName,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                )
                {
                    var fkConstraintName = ProviderUtils.GenerateForeignKeyConstraintName(
                        tableName,
                        column.ColumnName,
                        column.ReferencedTableName,
                        column.ReferencedColumnName
                    );
                    tableConstraints.ForeignKeyConstraints.Add(
                        new DxForeignKeyConstraint(
                            table.SchemaName,
                            table.TableName,
                            NormalizeName(fkConstraintName),
                            [new DxOrderedColumn(column.ColumnName)],
                            column.ReferencedTableName,
                            [new DxOrderedColumn(column.ReferencedColumnName)]
                        )
                    );
                }
            }

            var columnDefinitionSql = SqlInlineColumnDefinition(
                table,
                column,
                tableConstraints,
                dbVersion
            );
            sql.Append(columnDefinitionSql);
        }

        if (tableConstraints.PrimaryKeyConstraint != null)
        {
            sql.AppendLine();
            sql.Append("  ,");
            sql.Append(
                SqlInlinePrimaryKeyTableConstraint(table, tableConstraints.PrimaryKeyConstraint)
            );
        }

        foreach (var check in tableConstraints.CheckConstraints)
        {
            sql.AppendLine();
            sql.Append("  ,");
            sql.Append(SqlInlineCheckTableConstraint(table, check));
        }

        // Default constraints are added inline with the column definition always during CREATE TABLE and ADD COLUMN
        // foreach (var def in tableConstraints.DefaultConstraints)
        // {
        //     sql.AppendLine();
        //     sql.Append("  ,");
        //     sql.Append(SqlInlineDefaultTableConstraint(table, def));
        // }

        foreach (var uc in tableConstraints.UniqueConstraints)
        {
            sql.AppendLine();
            sql.Append("  ,");
            sql.Append(SqlInlineUniqueTableConstraint(table, uc, supportsOrderedKeysInConstraints));
        }

        // When creating a single table, we can add the foreign keys inline.
        // We assume that the referenced table already exists.
        if (
            afterAllTablesConstraints == null
            && table.ForeignKeyConstraints != null
            && table.ForeignKeyConstraints.Count > 0
        )
        {
            foreach (var fk in table.ForeignKeyConstraints)
            {
                sql.AppendLine();
                sql.Append("  ,");
                sql.Append(SqlInlineForeignKeyTableConstraint(table, fk));
            }
        }

        sql.AppendLine();
        sql.Append(");");

        var sqlStatement = sql.ToString();

        await ExecuteAsync(db, sqlStatement, tx: tx).ConfigureAwait(false);

        // Add indexes AFTER the table is created
        foreach (var index in tableConstraints.Indexes)
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

    public virtual async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        DxColumn[] columns,
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

        if (columns == null || columns.Length == 0)
        {
            throw new ArgumentException("At least one column is required.", nameof(columns));
        }

        return await CreateTableIfNotExistsAsync(
                db,
                new DxTable(
                    schemaName,
                    tableName,
                    columns,
                    primaryKey,
                    checkConstraints,
                    defaultConstraints,
                    uniqueConstraints,
                    foreignKeyConstraints,
                    indexes
                ),
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<DxTable?> GetTableAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        return (
            await GetTablesAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false)
        ).SingleOrDefault();
    }

    public virtual async Task<List<string>> GetTableNamesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var (sql, parameters) = SqlGetTableNames(schemaName, tableNameFilter);
        return await QueryAsync<string>(db, sql, parameters, tx: tx).ConfigureAwait(false);
    }

    public abstract Task<List<DxTable>> GetTablesAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    public virtual async Task<bool> DropTableIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var table = await GetTableAsync(db, schemaName, tableName, tx, cancellationToken);

        if (string.IsNullOrWhiteSpace(table?.TableName))
            return false;

        schemaName = table.SchemaName;
        tableName = table.TableName;

        // drop all related objects
        foreach (var index in table.Indexes)
        {
            await DropIndexIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    index.IndexName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var fk in table.ForeignKeyConstraints)
        {
            await DropForeignKeyConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    fk.ConstraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var uc in table.UniqueConstraints)
        {
            await DropUniqueConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    uc.ConstraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var dc in table.DefaultConstraints)
        {
            await DropDefaultConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    dc.ConstraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        foreach (var cc in table.CheckConstraints)
        {
            await DropCheckConstraintIfExistsAsync(
                    db,
                    schemaName,
                    tableName,
                    cc.ConstraintName,
                    tx,
                    cancellationToken
                )
                .ConfigureAwait(false);
        }

        // USUALLY, this is done by the database provider, and
        // it's not necessary to do it here.
        // await DropPrimaryKeyConstraintIfExistsAsync(
        //         db,
        //         schemaName,
        //         tableName,
        //         tx,
        //         cancellationToken
        //     )
        //     .ConfigureAwait(false);

        var sql = SqlDropTable(schemaName, tableName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> RenameTableIfExistsAsync(
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
            return false;

        var sql = SqlRenameTable(schemaName, tableName, newTableName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    public virtual async Task<bool> TruncateTableIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        if (
            !await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            return false;

        var sql = SqlTruncateTable(schemaName, tableName);

        await ExecuteAsync(db, sql, tx: tx).ConfigureAwait(false);

        return true;
    }

    protected abstract Task<List<DxIndex>> GetIndexesInternalAsync(
        IDbConnection db,
        string? schemaName,
        string? tableNameFilter,
        string? indexNameFilter,
        IDbTransaction? tx,
        CancellationToken cancellationToken
    );
}
