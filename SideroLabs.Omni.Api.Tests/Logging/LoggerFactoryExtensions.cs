using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Logging;

public static class LoggerFactoryExtensions
{
	public static ILoggerFactory AddXUnit(this ILoggerFactory factory, ITestOutputHelper output)
	{
		factory.AddProvider(new XunitLoggerProvider(output));
		return factory;
	}
}
