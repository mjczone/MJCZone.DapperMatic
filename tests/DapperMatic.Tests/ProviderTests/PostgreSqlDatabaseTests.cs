using System.Data;
using Dapper;
using DapperMatic.Tests.ProviderFixtures;
using Npgsql;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing Postgres 15
/// </summary>
public class PostgreSql_Postgres15_DatabaseTests(
    PostgreSql_Postgres15_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseTests<PostgreSql_Postgres15_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing Postgres 16
/// </summary>
public class PostgreSql_Postgres16_DatabaseTests(
    PostgreSql_Postgres16_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseTests<PostgreSql_Postgres16_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing Postgres 156 with Postgis
/// </summary>
public class PostgreSql_Postgis15_DatabaseTests(
    PostgreSql_Postgis15_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseTests<PostgreSql_Postgis15_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing Postgres 16 with Postgis
/// </summary>
public class PostgreSql_Postgis16_DatabaseTests(
    PostgreSql_Postgis16_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseTests<PostgreSql_Postgis16_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for Postgres database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class PostgreSqlDatabaseTests<TDatabaseFixture>(
    TDatabaseFixture fixture,
    ITestOutputHelper output
) : DatabaseTests(output), IClassFixture<TDatabaseFixture>, IDisposable
    where TDatabaseFixture : PostgreSqlDatabaseFixture
{
    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
        return connection;
    }
}
