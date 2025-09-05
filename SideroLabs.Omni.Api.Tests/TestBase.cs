using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SideroLabs.Omni.Api.Extensions;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Base class for all unit tests providing common setup and shared resources
/// </summary>
public abstract class TestBase : IDisposable
{
	/// <summary>
	/// Shared cancellation token source for all tests
	/// </summary>
	public static readonly CancellationTokenSource SharedCancellationTokenSource = new();

	/// <summary>
	/// Cancellation token for test operations
	/// </summary>
	protected static CancellationToken CancellationToken => SharedCancellationTokenSource.Token;

	private readonly ServiceProvider _serviceProvider;
	private bool _disposed;

	/// <summary>
	/// Initializes the test base with configuration and dependency injection
	/// </summary>
	protected TestBase()
	{
		// Build configuration from multiple sources
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddEnvironmentVariables("OMNI_")
			.AddUserSecrets<TestBase>()
			.Build();

		// Set up dependency injection
		var services = new ServiceCollection();

		// Register configuration
		services.AddSingleton<IConfiguration>(configuration);

		// Configure OmniClientOptions from configuration
		services.Configure<OmniClientOptions>(configuration.GetSection("Omni"));

		// Add the Omni client
		services.AddOmniClient(options =>
		{
			configuration.GetSection("Omni").Bind(options);
		});

		_serviceProvider = services.BuildServiceProvider();
	}

	/// <summary>
	/// Gets a service from the dependency injection container
	/// </summary>
	/// <typeparam name="T">The service type</typeparam>
	/// <returns>The service instance</returns>
	protected T GetService<T>() where T : notnull
	{
		return _serviceProvider.GetRequiredService<T>();
	}

	/// <summary>
	/// Gets the configured OmniClient instance
	/// </summary>
	protected OmniClient OmniClient => GetService<OmniClient>();

	/// <summary>
	/// Gets the configuration instance
	/// </summary>
	protected IConfiguration Configuration => GetService<IConfiguration>();

	/// <summary>
	/// Disposes the test base and releases resources
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_serviceProvider?.Dispose();
			_disposed = true;
		}

		GC.SuppressFinalize(this);
	}
}
