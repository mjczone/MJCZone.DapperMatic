using System.Data;
using Dapper;
using DapperMatic.Tests.ProviderFixtures;
using Npgsql;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing Postgres 15
/// </summary>
public class PostgreSql_Postgres15_DatabaseMethodsTests(
    PostgreSql_Postgres15_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseMethodsTests<PostgreSql_Postgres15_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing Postgres 16
/// </summary>
public class PostgreSql_Postgres16_DatabaseMethodsTests(
    PostgreSql_Postgres16_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseMethodsTests<PostgreSql_Postgres16_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing Postgres 156 with Postgis
/// </summary>
public class PostgreSql_Postgis15_DatabaseMethodsTests(
    PostgreSql_Postgis15_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseMethodsTests<PostgreSql_Postgis15_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing Postgres 16 with Postgis
/// </summary>
public class PostgreSql_Postgis16_DatabaseMethodsTests(
    PostgreSql_Postgis16_DatabaseFixture fixture,
    ITestOutputHelper output
) : PostgreSqlDatabaseMethodsTests<PostgreSql_Postgis16_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for Postgres database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class PostgreSqlDatabaseMethodsTests<TDatabaseFixture>(
    TDatabaseFixture fixture,
    ITestOutputHelper output
) : DatabaseMethodsTests(output), IClassFixture<TDatabaseFixture>, IDisposable
    where TDatabaseFixture : PostgreSqlDatabaseFixture
{
    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var db = new NpgsqlConnection(fixture.ConnectionString);
        await db.OpenAsync();
        await db.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
        return db;
    }
}
