namespace DapperMatic;

public enum DatabaseTypes
{
    Sqlite,
    SqlServer,
    MySql,
    PostgreSql,
}

public static class DatabaseTypeExtensions
{
    public static DatabaseTypes ToDatabaseType(this string provider)
    {
        if (
            string.IsNullOrWhiteSpace(provider)
            || provider.Contains("sqlite", StringComparison.OrdinalIgnoreCase)
        )
            return DatabaseTypes.Sqlite;

        if (
            provider.Contains("mysql", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("mariadb", StringComparison.OrdinalIgnoreCase)
        )
            return DatabaseTypes.MySql;

        if (
            provider.Contains("postgres", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("npgsql", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("pg", StringComparison.OrdinalIgnoreCase)
        )
            return DatabaseTypes.PostgreSql;

        if (
            provider.Contains("sqlserver", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("mssql", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("localdb", StringComparison.OrdinalIgnoreCase)
            || provider.Contains("sqlclient", StringComparison.OrdinalIgnoreCase)
        )
            return DatabaseTypes.SqlServer;

        throw new NotSupportedException($"Cache type {provider} is not supported.");
    }
}
