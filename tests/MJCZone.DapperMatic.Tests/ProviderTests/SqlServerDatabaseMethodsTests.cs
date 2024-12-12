using System.Data;
using System.Data.SqlClient;
using MJCZone.DapperMatic.Tests.ProviderFixtures;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing SqlServer 2022 Linux (CU image)
/// </summary>
public class SqlServer_2022_CU13_Ubuntu_DatabaseMethodsTests(
    SqlServer_2022_CU13_Ubuntu_DatabaseFixture fixture,
    ITestOutputHelper output
) : SqlServerDatabaseMethodsests<SqlServer_2022_CU13_Ubuntu_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing SqlServer 2019
/// </summary>
public class SqlServer_2019_CU27_DatabaseMethodsTests(
    SqlServer_2019_CU27_DatabaseFixture fixture,
    ITestOutputHelper output
) : SqlServerDatabaseMethodsests<SqlServer_2019_CU27_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing SqlServer 2017
/// </summary>
public class SqlServer_2017_CU29_DatabaseMethodsTests(
    SqlServer_2017_CU29_DatabaseFixture fixture,
    ITestOutputHelper output
) : SqlServerDatabaseMethodsests<SqlServer_2017_CU29_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for Postgres database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class SqlServerDatabaseMethodsests<TDatabaseFixture>(
    TDatabaseFixture fixture,
    ITestOutputHelper output
) : DatabaseMethodsTests(output), IClassFixture<TDatabaseFixture>, IDisposable
    where TDatabaseFixture : SqlServerDatabaseFixture
{
    static SqlServerDatabaseMethodsests()
    {
        Providers.DatabaseMethodsProvider.RegisterFactory(
            nameof(ProfiledSqlServerMethodsFactory),
            new ProfiledSqlServerMethodsFactory()
        );
    }

    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var db = new DbQueryLogging.LoggedDbConnection(
            new SqlConnection(fixture.ConnectionString),
            new Logging.TestLogger(Output, nameof(SqlConnection))
        );
        await db.OpenAsync();
        return db;
    }
}

public class ProfiledSqlServerMethodsFactory : Providers.SqlServer.SqlServerMethodsFactory
{
    public override bool SupportsConnectionCustom(IDbConnection db) =>
        db is DbQueryLogging.LoggedDbConnection loggedDb && loggedDb.Inner is SqlConnection;
}
