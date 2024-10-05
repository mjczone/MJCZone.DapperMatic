using System.Collections.Concurrent;
using System.Data;

namespace DapperMatic.Providers;

public static class DatabaseMethodsFactory
{
    private static readonly ConcurrentDictionary<DbProviderType, IDatabaseMethods> _methodsCache =
        new();

    public static IDatabaseMethods GetDatabaseMethods(IDbConnection db)
    {
        return GetDatabaseMethods(db.GetDbProviderType());
    }

    public static IDatabaseMethods GetDatabaseMethods(DbProviderType providerType)
    {
        // Try to get the DxTable from the cache
        if (_methodsCache.TryGetValue(providerType, out var databaseMethods))
        {
            return databaseMethods;
        }

        databaseMethods = providerType switch
        {
            DbProviderType.Sqlite => new Sqlite.SqliteMethods(),
            DbProviderType.SqlServer => new SqlServer.SqlServerMethods(),
            DbProviderType.MySql => new MySql.MySqlMethods(),
            DbProviderType.PostgreSql => new PostgreSql.PostgreSqlMethods(),
            _ => throw new NotSupportedException($"Provider {providerType} is not supported.")
        };

        _methodsCache.TryAdd(providerType, databaseMethods);

        return databaseMethods;
    }
}
