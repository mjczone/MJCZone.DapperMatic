using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
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
        using var connection = await OpenConnectionAsync();
        const int expected = 1;
        var actual = await connection.QueryFirstAsync<int>("SELECT 1");
        Assert.Equal(expected, actual);

        // run a statement with many sql statements at the same time
        await connection.ExecuteAsync(
            @"
            CREATE TABLE test (id INT PRIMARY KEY);
            INSERT INTO test VALUES (1);
            INSERT INTO test VALUES (2);
            INSERT INTO test VALUES (3);
            "
        );
        var values = await connection.QueryAsync<int>("SELECT id FROM test");
        Assert.Equal(3, values.Count());

        // run multiple select statements and read multiple result sets
        var result = await connection.QueryMultipleAsync(
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
        using var connection = await OpenConnectionAsync();

        var version = await connection.GetDatabaseVersionAsync();
        Assert.True(version.Major > 0);

        output.WriteLine("Database version: {0}", version);
    }

    [Fact]
    protected virtual async Task GetLastSqlWithParamsAsync_ReturnsLastSqlWithParams()
    {
        using var connection = await OpenConnectionAsync();

        var tableNames = await connection.GetTableNamesAsync(null, "testing*");

        var (lastSql, lastParams) = connection.GetLastSqlWithParams();
        Assert.NotEmpty(lastSql);
        Assert.NotNull(lastParams);

        output.WriteLine("Last SQL: {0}", lastSql);
        output.WriteLine("Last Parameters: {0}", JsonConvert.SerializeObject(lastParams));
    }

    [Fact]
    protected virtual async Task GetLastSqlAsync_ReturnsLastSql()
    {
        using var connection = await OpenConnectionAsync();

        var tableNames = await connection.GetTableNamesAsync(null, "testing*");

        var lastSql = connection.GetLastSql();
        Assert.NotEmpty(lastSql);

        output.WriteLine("Last SQL: {0}", lastSql);
    }
}
