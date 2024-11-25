using System.Data;
using DapperMatic.Tests.ProviderFixtures;
using MySql.Data.MySqlClient;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing MariaDb 11.2
/// </summary>
public class MariaDb_11_1_DatabaseMethodsTests(
    MariaDb_11_1_DatabaseFixture fixture,
    ITestOutputHelper output
) : MariaDbDatabaseMethodsTests<MariaDb_11_1_DatabaseFixture>(fixture, output) { }

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
    where TDatabaseFixture : MariaDbDatabaseFixture
{
    static MariaDbDatabaseMethodsTests()
    {
        Providers.DatabaseMethodsProvider.RegisterFactory(
            nameof(ProfiledMariaDbMethodsFactory),
            new ProfiledMariaDbMethodsFactory()
        );
    }

    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var connectionString = fixture.ConnectionString;
        // Disable SSL for local testing and CI environments
        if (!connectionString.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase))
        {
            connectionString += ";SSL Mode=None";
        }
        var db = new DbQueryLogging.LoggedDbConnection(
            new MySqlConnection(connectionString),
            new Logging.TestLogger(Output, nameof(MySqlConnection))
        );
        await db.OpenAsync();
        return db;
    }

    public override bool IgnoreSqlType(string sqlType)
    {
        return fixture.IgnoreSqlType(sqlType);
    }
}

public class ProfiledMariaDbMethodsFactory : Providers.MySql.MySqlMethodsFactory
{
    public override bool SupportsConnectionCustom(IDbConnection db) =>
        db is DbQueryLogging.LoggedDbConnection loggedDb && loggedDb.Inner is MySqlConnection;
}
