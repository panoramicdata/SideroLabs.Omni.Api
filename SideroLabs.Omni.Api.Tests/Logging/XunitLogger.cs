using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Logging;

/// <summary>
/// An ILogger implementation that writes formatted log messages to xUnit's ITestOutputHelper.
/// </summary>
public class XunitLogger(ITestOutputHelper output, string categoryName) : ILogger
{
	/// <summary>
	/// Begins a logical operation scope; returns null as scopes are not tracked.
	/// </summary>
	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	/// <summary>
	/// Always returns true; all log levels are enabled.
	/// </summary>
	public bool IsEnabled(LogLevel logLevel) => true;

	/// <summary>
	/// Formats and writes the log entry to the xUnit test output, including any exception details.
	/// </summary>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		output.WriteLine($"[{logLevel}] {categoryName}: {formatter(state, exception)}");
		if (exception != null)
		{
			output.WriteLine(exception.ToString());
		}
	}
}
