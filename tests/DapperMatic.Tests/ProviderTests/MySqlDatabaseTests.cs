using System.Data;
using DapperMatic.Tests.ProviderFixtures;
using MySql.Data.MySqlClient;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing MySql 90
/// </summary>
public class MySql_90_DatabaseTests(MySql_90_DatabaseFixture fixture, ITestOutputHelper output)
    : MySqlDatabaseTests<MySql_90_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing MySql 84
/// </summary>
public class MySql_84_DatabaseTests(MySql_84_DatabaseFixture fixture, ITestOutputHelper output)
    : MySqlDatabaseTests<MySql_84_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing MySql 57
/// </summary>
public class MySql_57_DatabaseTests(MySql_57_DatabaseFixture fixture, ITestOutputHelper output)
    : MySqlDatabaseTests<MySql_57_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing MariaDb 11.2 (short-term release, not LTS)
/// </summary>
// public class MariaDb_11_2_DatabaseTests(
//     MariaDb_11_2_DatabaseFixture fixture,
//     ITestOutputHelper output
// ) : MySqlDatabaseTests<MariaDb_11_2_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing MariaDb 10.11
/// </summary>
public class MariaDb_10_11_DatabaseTests(
    MariaDb_10_11_DatabaseFixture fixture,
    ITestOutputHelper output
) : MySqlDatabaseTests<MariaDb_10_11_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for Postgres database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class MySqlDatabaseTests<TDatabaseFixture>(
    TDatabaseFixture fixture,
    ITestOutputHelper output
) : DatabaseTests(output), IClassFixture<TDatabaseFixture>, IDisposable
    where TDatabaseFixture : MySqlDatabaseFixture
{
    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var connection = new MySqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
