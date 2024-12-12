using System.Data;
using System.Data.SQLite;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.Tests.ProviderTests;

public class SQLiteDatabaseMethodsTests(ITestOutputHelper output)
    : DatabaseMethodsTests(output),
        IDisposable
{
    static SQLiteDatabaseMethodsTests()
    {
        Providers.DatabaseMethodsProvider.RegisterFactory(
            nameof(ProfiledSqLiteMethodsFactory),
            new ProfiledSqLiteMethodsFactory()
        );
    }

    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        if (File.Exists("sqlite_tests.sqlite"))
        {
            File.Delete("sqlite_tests.sqlite");
        }

        var db = new DbQueryLogging.LoggedDbConnection(
            new SQLiteConnection("Data Source=sqlite_tests.sqlite;Version=3;BinaryGuid=False;"),
            new Logging.TestLogger(Output, nameof(SQLiteConnection))
        );
        await db.OpenAsync();
        return db;
    }

    public override void Dispose()
    {
        if (File.Exists("sqlite_tests.sqlite"))
        {
            File.Delete("sqlite_tests.sqlite");
        }

        base.Dispose();
    }
}

public class ProfiledSqLiteMethodsFactory : Providers.Sqlite.SqliteMethodsFactory
{
    public override bool SupportsConnectionCustom(IDbConnection db) =>
        db is DbQueryLogging.LoggedDbConnection loggedDb && loggedDb.Inner is SQLiteConnection;
}
