using System.Data;
using DapperMatic.Logging;
using DapperMatic.Tests.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly ITestOutputHelper Output;

    protected TestBase(ITestOutputHelper output)
    {
        Output = output;

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(output));
        });
        DxLogger.SetLoggerFactory(loggerFactory);
    }

    protected async Task InitFreshSchemaAsync(IDbConnection db, string? schemaName)
    {
        if (db.SupportsSchemas())
        {
            foreach (var view in await db.GetViewsAsync(schemaName))
            {
                try
                {
                    await db.DropViewIfExistsAsync(schemaName, view.ViewName);
                }
                catch { }
            }
            foreach (var table in await db.GetTablesAsync(schemaName))
            {
                await db.DropTableIfExistsAsync(schemaName, table.TableName);
            }
            // await db.DropSchemaIfExistsAsync(schemaName);
        }
        if (!string.IsNullOrEmpty(schemaName))
        {
            await db.CreateSchemaIfNotExistsAsync(schemaName);
        }
    }

    public virtual void Dispose()
    {
        DxLogger.SetLoggerFactory(LoggerFactory.Create(builder => builder.ClearProviders()));
    }

    protected void Log(string message)
    {
        Output.WriteLine(message);
    }

    public virtual bool IgnoreSqlType(string sqlType) => false;
}
