using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Logging;

/// <summary>
/// An ILoggerProvider that writes log output to an xUnit ITestOutputHelper.
/// </summary>
public class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
	/// <summary>
	/// Creates a logger that writes messages to the xUnit test output helper.
	/// </summary>
	public ILogger CreateLogger(string categoryName) => new XunitLogger(output, categoryName);

	/// <summary>
	/// Releases resources held by the provider.
	/// </summary>
	public void Dispose() => GC.SuppressFinalize(this);
}
