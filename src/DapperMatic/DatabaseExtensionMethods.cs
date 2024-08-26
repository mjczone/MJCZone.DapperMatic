using System.Collections.Concurrent;
using System.Data;

namespace DapperMatic;

public static partial class DatabaseExtensionMethods
{
    #region schema methods
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
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return false;
        }

        return await Database(db).SchemaExistsAsync(db, schema, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetSchemasAsync(
        this IDbConnection db,
        string? filter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return [];
        }

        return await Database(db).GetSchemasAsync(db, filter, tx, cancellationToken);
    }

    public static async Task<bool> CreateSchemaIfNotExistsAsync(
        this IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return false;
        }

        return await Database(db).CreateSchemaIfNotExistsAsync(db, schema, tx, cancellationToken);
    }

    public static async Task<bool> DropSchemaIfExistsAsync(
        this IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await Database(db).SupportsSchemasAsync(db, tx, cancellationToken))
        {
            return false;
        }

        return await Database(db).DropSchemaIfExistsAsync(db, schema, tx, cancellationToken);
    }

    #endregion // schema methods

    #region table methods

    public static async Task<bool> TableExistsAsync(
        this IDbConnection db,
        string table,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).TableExistsAsync(db, table, schema, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetTablesAsync(
        this IDbConnection db,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).GetTablesAsync(db, filter, schema, tx, cancellationToken);
    }

    public static async Task<bool> CreateTableIfNotExistsAsync(
        this IDbConnection db,
        string table,
        string? schema = null,
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
                table,
                schema,
                primaryKeyColumnNames,
                primaryKeyDotnetTypes,
                primaryKeyColumnLengths,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropTableIfExistsAsync(
        this IDbConnection db,
        string table,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).DropTableIfExistsAsync(db, table, schema, tx, cancellationToken);
    }

    #endregion // table methods

    #region column methods

    public static async Task<bool> ColumnExistsAsync(
        this IDbConnection db,
        string table,
        string column,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ColumnExistsAsync(db, table, column, schema, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetColumnsAsync(
        this IDbConnection db,
        string table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).GetColumnsAsync(db, table, filter, schema, tx, cancellationToken);
    }

    public static async Task<bool> CreateColumnIfNotExistsAsync(
        this IDbConnection db,
        string table,
        string column,
        Type dotnetType,
        string? type = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? schema = null,
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
                table,
                column,
                dotnetType,
                type,
                length,
                precision,
                scale,
                schema,
                defaultValue,
                nullable,
                unique,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropColumnIfExistsAsync(
        this IDbConnection db,
        string table,
        string column,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropColumnIfExistsAsync(db, table, column, schema, tx, cancellationToken);
    }

    #endregion // column methods

    #region index methods

    public static async Task<bool> IndexExistsAsync(
        this IDbConnection db,
        string table,
        string index,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).IndexExistsAsync(db, table, index, schema, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetIndexesAsync(
        this IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db).GetIndexesAsync(db, table, filter, schema, tx, cancellationToken);
    }

    public static async Task<bool> CreateIndexIfNotExistsAsync(
        this IDbConnection db,
        string table,
        string index,
        string[] columns,
        string? schema = null,
        bool unique = false,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateIndexIfNotExistsAsync(
                db,
                table,
                index,
                columns,
                schema,
                unique,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropIndexIfExistsAsync(
        this IDbConnection db,
        string table,
        string index,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropIndexIfExistsAsync(db, table, index, schema, tx, cancellationToken);
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
        string table,
        string column,
        string? foreignKey = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .ForeignKeyExistsAsync(db, table, column, foreignKey, schema, tx, cancellationToken);
    }

    public static async Task<IEnumerable<string>> GetForeignKeysAsync(
        this IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetForeignKeysAsync(db, table, filter, schema, tx, cancellationToken);
    }

    public static async Task<bool> CreateForeignKeyIfNotExistsAsync(
        this IDbConnection db,
        string table,
        string column,
        string foreignKey,
        string referenceTable,
        string referenceColumn,
        string? schema = null,
        string onDelete = "NO ACTION",
        string onUpdate = "NO ACTION",
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateForeignKeyIfNotExistsAsync(
                db,
                table,
                column,
                foreignKey,
                referenceTable,
                referenceColumn,
                schema,
                onDelete,
                onUpdate,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropForeignKeyIfExistsAsync(
        this IDbConnection db,
        string table,
        string column,
        string? foreignKey = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropForeignKeyIfExistsAsync(
                db,
                table,
                column,
                foreignKey,
                schema,
                tx,
                cancellationToken
            );
    }

    #endregion // foreign key methods

    #region unique constraint methods

    public static async Task<bool> UniqueConstraintExistsAsync(
        this IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .UniqueConstraintExistsAsync(
                db,
                table,
                uniqueConstraint,
                schema,
                tx,
                cancellationToken
            );
    }

    public static async Task<IEnumerable<string>> GetUniqueConstraintsAsync(
        this IDbConnection db,
        string? table,
        string? filter = null,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .GetUniqueConstraintsAsync(db, table, filter, schema, tx, cancellationToken);
    }

    public static async Task<bool> CreateUniqueConstraintIfNotExistsAsync(
        this IDbConnection db,
        string table,
        string uniqueConstraint,
        string[] columns,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .CreateUniqueConstraintIfNotExistsAsync(
                db,
                table,
                uniqueConstraint,
                columns,
                schema,
                tx,
                cancellationToken
            );
    }

    public static async Task<bool> DropUniqueConstraintIfExistsAsync(
        this IDbConnection db,
        string table,
        string uniqueConstraint,
        string? schema = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await Database(db)
            .DropUniqueConstraintIfExistsAsync(
                db,
                table,
                uniqueConstraint,
                schema,
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
