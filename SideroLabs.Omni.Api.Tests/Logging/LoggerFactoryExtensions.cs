using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Logging;

/// <summary>
/// Extension methods for ILoggerFactory to add xUnit test output logging.
/// </summary>
public static class LoggerFactoryExtensions
{
	/// <summary>
	/// Registers an xUnit test output helper as a logging provider on the logger factory.
	/// </summary>
	public static ILoggerFactory AddXUnit(this ILoggerFactory factory, ITestOutputHelper output)
	{
		factory.AddProvider(new XunitLoggerProvider(output));
		return factory;
	}
}
