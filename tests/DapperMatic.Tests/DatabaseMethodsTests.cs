using System.Data;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    private readonly ITestOutputHelper output;

    protected DatabaseMethodsTests(ITestOutputHelper output)
    {
        Console.WriteLine($"Initializing tests for {GetType().Name}");
        output.WriteLine($"Initializing tests for {GetType().Name}");
        this.output = output;
    }

    public abstract Task<IDbConnection> OpenConnectionAsync();

    [Fact]
    protected virtual async Task GetDatabaseVersionAsync_ReturnsVersion()
    {
        using var connection = await OpenConnectionAsync();

        var version = await connection.GetDatabaseVersionAsync();
        Assert.NotEmpty(version);

        output.WriteLine($"Database version: {version}");
    }

    [Fact]
    protected virtual async Task GetLastSqlWithParamsAsync_ReturnsLastSqlWithParams()
    {
        using var connection = await OpenConnectionAsync();

        var tableNames = await connection.GetTableNamesAsync(null, "testing*");

        var (lastSql, lastParams) = connection.GetLastSqlWithParams();
        Assert.NotEmpty(lastSql);
        Assert.NotNull(lastParams);

        output.WriteLine($"Last SQL: {lastSql}");
        output.WriteLine($"Last Parameters: {JsonConvert.SerializeObject(lastParams)}");
    }

    [Fact]
    protected virtual async Task GetLastSqlAsync_ReturnsLastSql()
    {
        using var connection = await OpenConnectionAsync();

        var tableNames = await connection.GetTableNamesAsync(null, "testing*");

        var lastSql = connection.GetLastSql();
        Assert.NotEmpty(lastSql);

        output.WriteLine($"Last SQL: {lastSql}");
    }

    public virtual void Dispose() => output.WriteLine(GetType().Name);
}
