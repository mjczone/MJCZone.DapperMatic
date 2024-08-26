using System.Data;
using System.Data.SQLite;
using Xunit.Abstractions;

namespace DapperMatic.Tests.ProviderTests;

public class SQLiteDatabaseTests(ITestOutputHelper output) : DatabaseTests(output), IDisposable
{
    public override async Task<IDbConnection> OpenConnectionAsync()
    {
        if (File.Exists("sqlite_tests.sqlite"))
            File.Delete("sqlite_tests.sqlite");

        var connection = new SQLiteConnection(
            "Data Source=sqlite_tests.sqlite;Version=3;BinaryGuid=False;"
        );
        await connection.OpenAsync();
        return connection;
    }

    public override void Dispose()
    {
        if (File.Exists("sqlite_tests.sqlite"))
            File.Delete("sqlite_tests.sqlite");

        base.Dispose();
    }
}
