using Microsoft.Extensions.Logging;

namespace SmtOrderManager.Tests.Application.TestHelpers;

internal static class TestLoggerFactory
{
    public static ILoggerFactory Create() =>
        LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

    public static ILogger<T> CreateLogger<T>() =>
        Create().CreateLogger<T>();
}
