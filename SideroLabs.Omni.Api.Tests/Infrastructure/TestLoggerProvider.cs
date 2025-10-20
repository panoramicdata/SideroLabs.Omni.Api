using Microsoft.Extensions.Logging;

namespace SideroLabs.Omni.Api.Tests.Infrastructure;

/// <summary>
/// Logger provider that wraps a test logger for use in DI
/// </summary>
internal class TestLoggerProvider(ILogger testLogger) : ILoggerProvider
{
	private readonly ILogger _testLogger = testLogger ?? throw new ArgumentNullException(nameof(testLogger));

	public ILogger CreateLogger(string categoryName)
	{
		// Return a wrapper that delegates to the test logger
		return new TestLoggerWrapper(_testLogger, categoryName);
	}

	public void Dispose()
	{
		// Nothing to dispose
	}

	private class TestLoggerWrapper(ILogger innerLogger, string categoryName) : ILogger
	{
		private readonly ILogger _innerLogger = innerLogger;
		private readonly string _categoryName = categoryName;

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
			=> _innerLogger.BeginScope(state);

		public bool IsEnabled(LogLevel logLevel)
			=> _innerLogger.IsEnabled(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			// Prepend category name to help identify the source
			var message = formatter(state, exception);
			var formattedMessage = $"[{_categoryName}] {message}";
			
			_innerLogger.Log(logLevel, eventId, formattedMessage, exception, (msg, ex) => msg);
		}
	}
}
