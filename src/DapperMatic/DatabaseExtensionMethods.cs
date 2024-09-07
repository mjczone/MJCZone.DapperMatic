using System.Collections.Concurrent;
using System.Data;
using DapperMatic.Models;

namespace DapperMatic;

public static partial class DatabaseExtensionMethods
{
    public static string GetLastSql(this IDbConnection db)
    {
        return Database(db).GetLastSql(db);
    }

    public static (string sql, object? parameters) GetLastSqlWithParams(this IDbConnection db)
    {
        return Database(db).GetLastSqlWithParams(db);
    }

    public static async Task<string> GetDatabaseVersionAsync(
        this IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).GetDatabaseVersionAsync(db, tx, cancellationToken);
    }

    #region schemaName methods
    public static async Task<bool> SupportsSchemasAsync(
        this IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).SupportsSchemasAsync(db, tx, cancellationToken);
    }

    public static async Task<bool> SchemaExistsAsync(
        this IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return false;
        }

        return await Database(db).SchemaExistsAsync(db, schemaName, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetSchemasAsync(
        this IDbConnection db,
        string? nameFilter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return [];
        }

        return await Database(db).GetSchemasAsync(db, nameFilter, tx, cancellationToken);
    }

    public static async Task<bool> CreateSchemaIfNotExistsAsync(
        this IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return false;
        }

        return await Database(db)
            .CreateSchemaIfNotExistsAsync(db, schemaName, tx, cancellationToken);
    }

    public static async Task<bool> DropSchemaIfExistsAsync(
        this IDbConnection db,
        string schemaName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return false;
        }

        return await Database(db).DropSchemaIfExistsAsync(db, schemaName, tx, cancellationToken);
    }

    #endregion // schemaName methods

    #region table methods

    public static async Task<bool> TableExistsAsync(
        this IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .TableExistsAsync(db, tableName, schemaName, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetTablesAsync(
        this IDbConnection db,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).GetTablesAsync(db, nameFilter, schemaName, tx, cancellationToken);
    }

    public static async Task<bool> CreateTableIfNotExistsAsync(
        this IDbConnection db,
        string tableName,
        string? schemaName = null,
        string[]? primaryKeyColumnNames = null,
        Type[]? primaryKeyDotnetTypes = null,
        int?[]? primaryKeyColumnLengths = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateTableIfNotExistsAsync(
                db,
                tableName,
                schemaName,
                primaryKeyColumnNames,
                primaryKeyDotnetTypes,
                primaryKeyColumnLengths,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropTableIfExistsAsync(
        this IDbConnection db,
        string tableName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropTableIfExistsAsync(db, tableName, schemaName, tx, cancellationToken);
    }

    #endregion // table methods

    #region columnName methods

    public static async Task<bool> ColumnExistsAsync(
        this IDbConnection db,
        string tableName,
        string columnName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ColumnExistsAsync(db, tableName, columnName, schemaName, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetColumnsAsync(
        this IDbConnection db,
        string tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetColumnsAsync(db, tableName, nameFilter, schemaName, tx, cancellationToken);
    }

    public static async Task<bool> CreateColumnIfNotExistsAsync(
        this IDbConnection db,
        string tableName,
        string columnName,
        Type dotnetType,
        string? type = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? schemaName = null,
        string? defaultValue = null,
        bool nullable = true,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateColumnIfNotExistsAsync(
                db,
                tableName,
                columnName,
                dotnetType,
                type,
                length,
                precision,
                scale,
                schemaName,
                defaultValue,
                nullable,
                unique,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropColumnIfExistsAsync(
        this IDbConnection db,
        string tableName,
        string columnName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropColumnIfExistsAsync(db, tableName, columnName, schemaName, tx, cancellationToken);
    }

    #endregion // columnName methods

    #region indexName methods

    public static async Task<bool> IndexExistsAsync(
        this IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .IndexExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken);
    }

    public static async Task<IEnumerable<TableIndex>> GetIndexesAsync(
        this IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexesAsync(db, tableName, nameFilter, schemaName, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetIndexNamesAsync(
        this IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetIndexNamesAsync(db, tableName, nameFilter, schemaName, tx, cancellationToken);
    }

    public static async Task<bool> CreateIndexIfNotExistsAsync(
        this IDbConnection db,
        string tableName,
        string indexName,
        string[] columnNames,
        string? schemaName = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateIndexIfNotExistsAsync(
                db,
                tableName,
                indexName,
                columnNames,
                schemaName,
                unique,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropIndexIfExistsAsync(
        this IDbConnection db,
        string tableName,
        string indexName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropIndexIfExistsAsync(db, tableName, indexName, schemaName, tx, cancellationToken);
    }

    #endregion // index methods

    #region foreign key methods
    public static async Task<bool> SupportsNamedForeignKeysAsync(
        this IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).SupportsNamedForeignKeysAsync(db, tx, cancellationToken);
    }

    public static async Task<bool> ForeignKeyExistsAsync(
        this IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ForeignKeyExistsAsync(
                db,
                tableName,
                columnName,
                foreignKey,
                schemaName,
                tx,
                cancellationToken
            );
    }

    public static async Task<IEnumerable<string>> GetForeignKeysAsync(
        this IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeysAsync(db, tableName, nameFilter, schemaName, tx, cancellationToken);
    }

    public static async Task<bool> CreateForeignKeyIfNotExistsAsync(
        this IDbConnection db,
        string tableName,
        string columnName,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schemaName = null,
        string onDelete = "NO ACTION",
        string onUpdate = "NO ACTION",
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateForeignKeyIfNotExistsAsync(
                db,
                tableName,
                columnName,
                foreignKey,
                referenceTable,
                referenceColumn,
                schemaName,
                onDelete,
                onUpdate,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropForeignKeyIfExistsAsync(
        this IDbConnection db,
        string tableName,
        string columnName,
        string? foreignKey = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropForeignKeyIfExistsAsync(
                db,
                tableName,
                columnName,
                foreignKey,
                schemaName,
                tx,
                cancellationToken
            );
    }

    #endregion // foreign key methods

    #region unique constraint methods

    public static async Task<bool> UniqueConstraintExistsAsync(
        this IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .UniqueConstraintExistsAsync(
                db,
                tableName,
                uniqueConstraintName,
                schemaName,
                tx,
                cancellationToken
            );
    }

    public static async Task<IEnumerable<string>> GetUniqueConstraintsAsync(
        this IDbConnection db,
        string? tableName,
        string? nameFilter = null,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintsAsync(
                db,
                tableName,
                nameFilter,
                schemaName,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        this IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string[] columnNames,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateUniqueConstraintIfNotExistsAsync(
                db,
                tableName,
                uniqueConstraintName,
                columnNames,
                schemaName,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropUniqueConstraintIfExistsAsync(
        this IDbConnection db,
        string tableName,
        string uniqueConstraintName,
        string? schemaName = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropUniqueConstraintIfExistsAsync(
                db,
                tableName,
                uniqueConstraintName,
                schemaName,
                tx,
                cancellationToken
            );
    }

    #endregion // unique constraint methods

    #region Private static methods
    private static readonly ConcurrentDictionary<Type, DatabaseTypes> _providerTypes = new();
    private static readonly ConcurrentDictionary<DatabaseTypes, IDatabaseExtensions> _extensions =
        new();

    private static IDatabaseExtensions Database(IDbConnection db)
    {
        return GetDatabaseExtensions(GetDatabaseType(db));
    }

    public static DatabaseTypes GetDatabaseType(this IDbConnection db)
    {
        var dbType = db.GetType();
        if (_providerTypes.TryGetValue(dbType, out var provider))
        {
            return provider;
        }
        return dbType.FullName!.ToDatabaseType();
    }

    private static IDatabaseExtensions GetDatabaseExtensions(DatabaseTypes provider)
    {
        return _extensions.GetOrAdd(
            provider,
            provider switch
            {
                DatabaseTypes.Sqlite => new Providers.Sqlite.SqliteExtensions(),
                DatabaseTypes.SqlServer => new Providers.SqlServer.SqlServerExtensions(),
                DatabaseTypes.MySql => new Providers.MySql.MySqlExtensions(),
                DatabaseTypes.PostgreSql => new Providers.PostgreSql.PostgreSqlExtensions(),
                _ => throw new NotSupportedException($"Provider {provider} is not supported.")
            }
        );
    }
    #endregion // Private static methods
}
