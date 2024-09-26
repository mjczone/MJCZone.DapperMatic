using DapperMatic.Logging;
using DapperMatic.Tests.Logging;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract class TestBase
{
    private readonly ITestOutputHelper output;
    protected ILogger Logger { get; }

    protected TestBase(ITestOutputHelper output)
    {
        this.output = output;
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(output));
        });
        DxLogger.SetLoggerFactory(loggerFactory);
        Logger = loggerFactory.CreateLogger(GetType());
        Logger.LogInformation("Initializing tests for {test}", GetType().Name);
    }
}
