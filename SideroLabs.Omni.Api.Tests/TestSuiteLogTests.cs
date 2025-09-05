using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for logging functionality within the test suite
/// </summary>
public class TestSuiteLogTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Theory]
	[InlineData(LogLevel.Trace)]
	[InlineData(LogLevel.Debug)]
	[InlineData(LogLevel.Information)]
	[InlineData(LogLevel.Warning)]
	[InlineData(LogLevel.Error)]
	[InlineData(LogLevel.Critical)]
	public void LoggingEndpointsTest_Succeeds(LogLevel logLevel)
	{
		const string TestMessageTemplate = "Testing {logLevel} logging endpoint";

		((Action)(logLevel switch
		{
			LogLevel.Trace => () => Logger.LogTrace(TestMessageTemplate, logLevel),
			LogLevel.Debug => () => Logger.LogDebug(TestMessageTemplate, logLevel),
			LogLevel.Information => () => Logger.LogInformation(TestMessageTemplate, logLevel),
			LogLevel.Warning => () => Logger.LogWarning(TestMessageTemplate, logLevel),
			LogLevel.Error => () => Logger.LogError(TestMessageTemplate, logLevel),
			LogLevel.Critical => () => Logger.LogCritical(TestMessageTemplate, logLevel),
			_ => throw new ArgumentException($"Unsupported log level: {logLevel}", nameof(logLevel))
		}))();
	}
}
