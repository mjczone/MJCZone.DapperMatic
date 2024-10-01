using System.Data;
using Dapper;
using DapperMatic.Models;
using Microsoft.VisualBasic;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract class DatabaseTests : TestBase
{
    public DatabaseTests(ITestOutputHelper output)
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

    public virtual void Dispose()
    {
        /* do nothing */
    }
}
