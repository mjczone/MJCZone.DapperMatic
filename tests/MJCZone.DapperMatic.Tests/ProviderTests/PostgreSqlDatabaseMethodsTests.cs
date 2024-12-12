using System.Data;
using Dapper;
using MJCZone.DapperMatic.Providers;
using MJCZone.DapperMatic.Tests.ProviderFixtures;
using Npgsql;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.Tests.ProviderTests;

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
    static PostgreSqlDatabaseMethodsTests()
    {
        DatabaseMethodsProvider.RegisterFactory(
            nameof(ProfiledPostgreSqlMethodsFactory),
            new ProfiledPostgreSqlMethodsFactory()
        );
    }

    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var db = new DbQueryLogging.LoggedDbConnection(
            new NpgsqlConnection(fixture.ConnectionString),
            new Logging.TestLogger(Output, nameof(NpgsqlConnection))
        );
        await db.OpenAsync();
        await db.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
        await db.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"hstore\";");
        if (
            await db.ExecuteScalarAsync<int>(
                @"select count(*) from pg_extension where extname = 'postgis'"
            ) > 0
        )
        {
            await db.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"postgis\";");
            await db.ExecuteAsync("CREATE EXTENSION IF NOT EXISTS \"postgis_topology\";");
        }
        return db;
    }
}

public class ProfiledPostgreSqlMethodsFactory : Providers.PostgreSql.PostgreSqlMethodsFactory
{
    public override bool SupportsConnectionCustom(IDbConnection db) =>
        db is DbQueryLogging.LoggedDbConnection loggedDb && loggedDb.Inner is NpgsqlConnection;
}
