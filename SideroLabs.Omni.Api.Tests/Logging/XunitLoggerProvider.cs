using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Logging;

public class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName) => new XunitLogger(output, categoryName);

	public void Dispose() => GC.SuppressFinalize(this);
}
