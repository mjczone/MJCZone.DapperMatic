using System.Collections.Concurrent;
using System.Data;
using DapperMatic.Interfaces;
using DapperMatic.Providers.MySql;
using DapperMatic.Providers.PostgreSql;
using DapperMatic.Providers.Sqlite;
using DapperMatic.Providers.SqlServer;

namespace DapperMatic.Providers;

internal static class DatabaseMethodsFactory
{
    private static readonly ConcurrentDictionary<DbProviderType, IDatabaseMethods> MethodsCache =
        new();

    public static IDatabaseMethods GetDatabaseMethods(IDbConnection db)
    {
        return GetDatabaseMethods(db.GetDbProviderType());
    }

    private static IDatabaseMethods GetDatabaseMethods(DbProviderType providerType)
    {
        // Try to get the DxTable from the cache
        if (MethodsCache.TryGetValue(providerType, out var databaseMethods))
        {
            return databaseMethods;
        }

        databaseMethods = providerType switch
        {
            DbProviderType.Sqlite => new SqliteMethods(),
            DbProviderType.SqlServer => new SqlServerMethods(),
            DbProviderType.MySql => new MySqlMethods(),
            DbProviderType.PostgreSql => new PostgreSqlMethods(),
            _ => throw new NotSupportedException($"Provider {providerType} is not supported.")
        };

        MethodsCache.TryAdd(providerType, databaseMethods);

        return databaseMethods;
    }
}
