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
        return await GetTableAsync(db, schemaName, tableName, tx, cancellationToken) != null;
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
        return (
            await GetTablesAsync(db, schemaName, tableNameFilter, tx, cancellationToken)
                .ConfigureAwait(false)
        )
            .Select(x => x.TableName)
            .ToList();
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
        if (
            !(
                await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                    .ConfigureAwait(false)
            )
        )
            return false;

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        // drop table
        await ExecuteAsync(db, $@"DROP TABLE {schemaQualifiedTableName}", tx: tx)
            .ConfigureAwait(false);

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
        if (
            !(
                await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                    .ConfigureAwait(false)
            )
        )
            return false;

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} RENAME TO {newTableName}",
                tx: tx
            )
            .ConfigureAwait(false);

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
        if (
            !(
                await DoesTableExistAsync(db, schemaName, tableName, tx, cancellationToken)
                    .ConfigureAwait(false)
            )
        )
            return false;

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        await ExecuteAsync(db, $@"TRUNCATE TABLE {schemaQualifiedTableName}", tx: tx)
            .ConfigureAwait(false);

        return true;
    }
}
