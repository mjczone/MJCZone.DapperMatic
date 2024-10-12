using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DapperMatic.Logging;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class DxLogger
{
    private static ILoggerFactory _loggerFactory = new NullLoggerFactory();

    public static void SetLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public static ILogger CreateLogger<T>()
    {
        return _loggerFactory.CreateLogger<T>();
    }

    public static ILogger CreateLogger(Type type)
    {
        return _loggerFactory.CreateLogger(type);
    }
}
