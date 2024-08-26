using System.Data;
using System.Data.SqlClient;
using DapperMatic.Tests.ProviderFixtures;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

/// <summary>
/// Testing SqlServer 2022 Linux (CU image)
/// </summary>
public class SqlServer_2022_CU13_Ubuntu_DatabaseTests(
    SqlServer_2022_CU13_Ubuntu_DatabaseFixture fixture,
    ITestOutputHelper output
) : SqlServerDatabaseTests<SqlServer_2022_CU13_Ubuntu_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing SqlServer 2019
/// </summary>
public class SqlServer_2019_CU27_DatabaseTests(
    SqlServer_2019_CU27_DatabaseFixture fixture,
    ITestOutputHelper output
) : SqlServerDatabaseTests<SqlServer_2019_CU27_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Testing SqlServer 2017
/// </summary>
public class SqlServer_2017_CU29_DatabaseTests(
    SqlServer_2017_CU29_DatabaseFixture fixture,
    ITestOutputHelper output
) : SqlServerDatabaseTests<SqlServer_2017_CU29_DatabaseFixture>(fixture, output) { }

/// <summary>
/// Abstract class for Postgres database tests
/// </summary>
/// <typeparam name="TDatabaseFixture"></typeparam>
public abstract class SqlServerDatabaseTests<TDatabaseFixture>(
    TDatabaseFixture fixture,
    ITestOutputHelper output
) : DatabaseTests(output), IClassFixture<TDatabaseFixture>, IDisposable
    where TDatabaseFixture : SqlServerDatabaseFixture
{
    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
