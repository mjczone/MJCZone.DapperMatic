using System.Data;
using Dapper;
using DapperMatic.Tests.ProviderFixtures;
using MySql.Data.MySqlClient;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing MariaDb 10.11
/// </summary>
public class MariaDb_10_11_DatabaseMethodsTests(
    MariaDb_10_11_DatabaseFixture fixture,
    ITestOutputHelper output
) : MariaDbDatabaseMethodsTests<MariaDb_10_11_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for MySql database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class MariaDbDatabaseMethodsTests<TDatabaseFixture>(
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
        var db = new MySqlConnection(connectionString);
        await db.OpenAsync();
        return db;
    }
}