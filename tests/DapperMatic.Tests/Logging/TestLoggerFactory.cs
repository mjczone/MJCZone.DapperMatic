namespace DapperMatic.Tests.Logging;

using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public TestLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(_output, categoryName);
    }

    public void Dispose() { }
}
