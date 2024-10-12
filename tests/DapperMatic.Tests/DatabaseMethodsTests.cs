using System.Data;
using Dapper;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests : TestBase
{
    protected DatabaseMethodsTests(ITestOutputHelper output)
        : base(output) { }

    public abstract Task<IDbConnection> OpenConnectionAsync();

    [Fact]
    protected virtual async Task Database_Can_RunArbitraryQueriesAsync()
    {
        using var db = await OpenConnectionAsync();
        const int expected = 1;
        var actual = await db.QueryFirstAsync<int>("SELECT 1");
        Assert.Equal(expected, actual);

        // run a statement with many sql statements at the same time
        await db.ExecuteAsync(
            @"
            CREATE TABLE test (id INT PRIMARY KEY);
            INSERT INTO test VALUES (1);
            INSERT INTO test VALUES (2);
            INSERT INTO test VALUES (3);
            "
        );
        var values = await db.QueryAsync<int>("SELECT id FROM test");
        Assert.Equal(3, values.Count());

        // run multiple select statements and read multiple result sets
        var result = await db.QueryMultipleAsync(
            @"
            SELECT id FROM test WHERE id = 1;
            SELECT id FROM test WHERE id = 2;
            SELECT id FROM test;
            -- this statement is ignored by the grid reader
            -- because it doesn't return any results
            INSERT INTO test VALUES (4);
            SELECT id FROM test WHERE id = 4;
            "
        );
        var id1 = result.Read<int>().Single();
        var id2 = result.Read<int>().Single();
        var allIds = result.Read<int>().ToArray();
        var id4 = result.Read<int>().Single();
        Assert.Equal(1, id1);
        Assert.Equal(2, id2);
        Assert.Equal(3, allIds.Length);
        Assert.Equal(4, id4);
    }

    [Fact]
    protected virtual async Task GetDatabaseVersionAsync_ReturnsVersion()
    {
        using var db = await OpenConnectionAsync();

        var version = await db.GetDatabaseVersionAsync();
        Assert.True(version.Major > 0);

        Output.WriteLine("Database version: {0}", version);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task GetLastSqlWithParamsAsync_ReturnsLastSqlWithParams(
        string? schemaName
    )
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var tableNames = await db.GetTableNamesAsync(schemaName, "testing*");

        var (lastSql, lastParams) = db.GetLastSqlWithParams();
        Assert.NotEmpty(lastSql);
        Assert.NotNull(lastParams);

        Output.WriteLine("Last SQL: {0}", lastSql);
        Output.WriteLine("Last Parameters: {0}", JsonConvert.SerializeObject(lastParams));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task GetLastSqlAsync_ReturnsLastSql(string? schemaName)
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var tableNames = await db.GetTableNamesAsync(schemaName, "testing*");

        var lastSql = db.GetLastSql();
        Assert.NotEmpty(lastSql);

        Output.WriteLine("Last SQL: {0}", lastSql);
    }
}
