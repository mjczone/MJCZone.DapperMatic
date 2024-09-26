using System.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests : TestBase, IDisposable
{
    private bool disposedValue;

    protected DatabaseMethodsTests(ITestOutputHelper output)
        : base(output) { }

    public abstract Task<IDbConnection> OpenConnectionAsync();

    [Fact]
    protected virtual async Task GetDatabaseVersionAsync_ReturnsVersion()
    {
        using var connection = await OpenConnectionAsync();

        var version = await connection.GetDatabaseVersionAsync();
        Assert.NotEmpty(version);

        Logger.LogInformation("Database version: {version}", version);
    }

    [Fact]
    protected virtual async Task GetLastSqlWithParamsAsync_ReturnsLastSqlWithParams()
    {
        using var connection = await OpenConnectionAsync();

        var tableNames = await connection.GetTableNamesAsync(null, "testing*");

        var (lastSql, lastParams) = connection.GetLastSqlWithParams();
        Assert.NotEmpty(lastSql);
        Assert.NotNull(lastParams);

        Logger.LogInformation("Last SQL: {sql}", lastSql);
        Logger.LogInformation(
            "Last Parameters: {parameters}",
            JsonConvert.SerializeObject(lastParams)
        );
    }

    [Fact]
    protected virtual async Task GetLastSqlAsync_ReturnsLastSql()
    {
        using var connection = await OpenConnectionAsync();

        var tableNames = await connection.GetTableNamesAsync(null, "testing*");

        var lastSql = connection.GetLastSql();
        Assert.NotEmpty(lastSql);

        Logger.LogInformation("Last SQL: {sql}", lastSql);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public virtual void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
