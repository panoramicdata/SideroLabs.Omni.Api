using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Extensions;

namespace SideroLabs.Omni.Api.Tests.Infrastructure;

/// <summary>
/// Factory for creating test service providers
/// </summary>
/// <param name="logger">Logger instance</param>
public class TestServiceProviderFactory(ILogger logger)
{
	private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>
	/// Creates a service provider configured for testing
	/// </summary>
	/// <returns>Configured service provider</returns>
	public ServiceProvider CreateServiceProvider()
	{
		var configBuilder = new TestConfigurationBuilder(_logger);
		var configuration = configBuilder.BuildConfiguration();
		var services = new ServiceCollection();

		// Register configuration
		services.AddSingleton(configuration);

		// Register logger factory that uses our test logger - CRITICAL for test output!
		services.AddSingleton<ILoggerFactory>(sp =>
		{
			var factory = new LoggerFactory();
			factory.AddProvider(new TestLoggerProvider(_logger));
			return factory;
		});

		// Register test expectations
		var testExpectations = configBuilder.GetTestExpectations(configuration);
		services.AddSingleton(testExpectations);

		// Configure OmniClientOptions from configuration and test PGP key
		services.Configure<OmniClientOptions>(options =>
			configBuilder.ConfigureOmniClientOptions(configuration, options));

		// Add the Omni client with the configured options
		services.AddOmniClient(opts =>
			configBuilder.ConfigureOmniClientOptions(configuration, opts));

		return services.BuildServiceProvider();
	}
}
