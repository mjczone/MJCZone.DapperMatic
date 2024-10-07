using DapperMatic.Logging;
using DapperMatic.Tests.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly ITestOutputHelper output;

    protected TestBase(ITestOutputHelper output)
    {
        this.output = output;

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(output));
        });
        DxLogger.SetLoggerFactory(loggerFactory);
    }

    public virtual void Dispose()
    {
        DxLogger.SetLoggerFactory(LoggerFactory.Create(builder => builder.ClearProviders()));
    }

    protected void Log(string message)
    {
        output.WriteLine(message);
    }
}
