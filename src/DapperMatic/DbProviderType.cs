using System.Collections.Concurrent;
using System.Data;

namespace DapperMatic;

public enum DbProviderType
{
    Sqlite,
    SqlServer,
    MySql,
    PostgreSql
}

public static class DbProviderTypeExtensions
{
    private static readonly ConcurrentDictionary<Type, DbProviderType> ProviderTypes = new();

    public static DbProviderType GetDbProviderType(this IDbConnection db)
    {
        var type = db.GetType();
        if (ProviderTypes.TryGetValue(type, out var dbType))
        {
            return dbType;
        }

        dbType = ToDbProviderType(type.FullName!);
        ProviderTypes.TryAdd(type, dbType);

        return dbType;
    }

    private static DbProviderType ToDbProviderType(string provider)
    {
        if (provider.Contains("sqlite", StringComparison.OrdinalIgnoreCase))
            return DbProviderType.Sqlite;

        if (
            provider.Contains("mysql", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("maria", StringComparison.OrdinalIgnoreCase)
        )
            return DbProviderType.MySql;

        if (
            provider.Contains("postgres", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("npgsql", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("pg", StringComparison.OrdinalIgnoreCase)
        )
            return DbProviderType.PostgreSql;

        if (
            provider.Contains("sqlserver", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("mssql", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("localdb", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("sqlclient", StringComparison.OrdinalIgnoreCase)
        )
            return DbProviderType.SqlServer;

        throw new NotSupportedException($"Db type {provider} is not supported.");
    }
}
