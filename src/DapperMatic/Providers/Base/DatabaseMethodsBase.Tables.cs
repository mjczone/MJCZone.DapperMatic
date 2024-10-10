using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers;

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

    public virtual async Task<bool> CreateTableIfNotExistsAsync(
        IDbConnection db,
        DxTable table,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await CreateTableIfNotExistsAsync(
            db,
            table.SchemaName,
            table.TableName,
            table.Columns.ToArray(),
            table.PrimaryKeyConstraint,
            table.CheckConstraints.ToArray(),
            table.DefaultConstraints.ToArray(),
            table.UniqueConstraints.ToArray(),
            table.ForeignKeyConstraints.ToArray(),
            table.Indexes.ToArray()
        );
    }

    public abstract Task<bool> CreateTableIfNotExistsAsync(
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
    );

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
            throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
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
