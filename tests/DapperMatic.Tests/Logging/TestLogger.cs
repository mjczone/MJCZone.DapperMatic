using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DapperMatic.Tests.Logging;

public class TestLogger : ILogger, IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private LogLevel _minLogLevel = LogLevel.Debug;
    private ITestOutputHelper _output;
    private string _categoryName;

    public TestLogger(ITestOutputHelper output, string categoryName)
    {
        _output = output;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _minLogLevel;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (IsEnabled(logLevel))
        {
            _output.WriteLine(
                "[DapperMatic {0:hh\\:mm\\:ss\\.ff}] {1}",
                _stopwatch.Elapsed,
                formatter.Invoke(state, exception)
            );
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // The default console logger does not support scopes. We return itself as IDisposable implementation.
    }
}
