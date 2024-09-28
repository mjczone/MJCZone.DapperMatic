using System.Data;
using Dapper;
using DapperMatic.Tests.ProviderFixtures;
using MySql.Data.MySqlClient;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing MySql 90
/// </summary>
public class MySql_90_DatabaseMethodsTests(
    MySql_90_DatabaseFixture fixture,
    ITestOutputHelper output
) : MySqlDatabaseMethodsTests<MySql_90_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing MySql 84
/// </summary>
public class MySql_84_DatabaseMethodsTests(
    MySql_84_DatabaseFixture fixture,
    ITestOutputHelper output
) : MySqlDatabaseMethodsTests<MySql_84_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing MySql 57
/// </summary>
public class MySql_57_DatabaseMethodsTests(
    MySql_57_DatabaseFixture fixture,
    ITestOutputHelper output
) : MySqlDatabaseMethodsTests<MySql_57_DatabaseFixture>(fixture, output) { }

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
public class MariaDb_10_11_DatabaseMethodsTests(
    MariaDb_10_11_DatabaseFixture fixture,
    ITestOutputHelper output
) : MySqlDatabaseMethodsTests<MariaDb_10_11_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for MySql database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class MySqlDatabaseMethodsTests<TDatabaseFixture>(
    TDatabaseFixture fixture,
    ITestOutputHelper output
) : DatabaseMethodsTests(output), IClassFixture<TDatabaseFixture>, IDisposable
    where TDatabaseFixture : MySqlDatabaseFixture
{
    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var connectionString = fixture.ConnectionString;
        // Disable SSL for local testing and CI environments
        if (!connectionString.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";SSL Mode=None";
        }
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
