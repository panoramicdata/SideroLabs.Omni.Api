using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;
using SideroLabs.Omni.Api.Tests.Infrastructure;
using SideroLabs.Omni.Api.Tests.Logging;
using Xunit;

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

	protected ILogger Logger { get; }

	/// <summary>
	/// Initializes the test base with configuration and dependency injection
	/// </summary>
	protected TestBase(ITestOutputHelper testOutputHelper)
	{
		Logger = new LoggerFactory()
				.AddXUnit(testOutputHelper)
				.CreateLogger<TestBase>();

		var serviceProviderFactory = new TestServiceProviderFactory(Logger);
		_serviceProvider = serviceProviderFactory.CreateServiceProvider();
	}

	/// <summary>
	/// Gets a service from the dependency injection container
	/// </summary>
	/// <typeparam name="T">The service type</typeparam>
	/// <returns>The service instance</returns>
	protected T GetService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

	/// <summary>
	/// Gets the configured OmniClient instance (safe for testing)
	/// </summary>
	protected IOmniClient OmniClient => GetService<IOmniClient>();

	/// <summary>
	/// Gets the configuration instance
	/// </summary>
	protected IConfiguration Configuration => GetService<IConfiguration>();

	/// <summary>
	/// Gets the test expectations from configuration
	/// </summary>
	protected TestExpectations TestExpectations => GetService<TestExpectations>();

	/// <summary>
	/// Creates an OmniAuthenticator from the test PGP key file for testing
	/// </summary>
	/// <returns>OmniAuthenticator instance</returns>
	protected static async Task<OmniAuthenticator> CreateTestAuthenticatorAsync()
	{
		var testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
		var testKeyFile = new FileInfo(Path.Combine(testDataDirectory, "pgp-key-test.txt"));

		var loggerFactory = new LoggerFactory();
		var logger = loggerFactory.CreateLogger<OmniAuthenticator>();
		return await OmniAuthenticator.FromFileAsync(testKeyFile, logger, CancellationToken);
	}

	#region Test Data Generators

	/// <summary>
	/// Generates a unique resource ID with optional prefix
	/// </summary>
	/// <param name="prefix">Prefix for the ID (default: "test")</param>
	/// <returns>Unique resource ID</returns>
	protected static string GenerateUniqueId(string prefix = "test")
		=> $"{prefix}-{Guid.NewGuid():N}";

	/// <summary>
	/// Generates a unique test email address
	/// </summary>
	/// <returns>Unique test email</returns>
	protected static string GenerateTestEmail()
		=> $"test-{Guid.NewGuid():N}@test.panoramicdata.com";

	/// <summary>
	/// Gets the current timestamp for test operations
	/// </summary>
	/// <returns>Current UTC timestamp</returns>
	protected static DateTimeOffset GetTestTimestamp()
		=> DateTimeOffset.UtcNow;

	#endregion

	#region Resource Assertions

	/// <summary>
	/// Asserts that a resource was created successfully
	/// </summary>
	/// <typeparam name="T">Resource type</typeparam>
	/// <param name="resource">The created resource</param>
	protected static void AssertResourceCreated<T>(T resource) where T : SideroLabs.Omni.Api.Resources.IOmniResource
	{
		Assert.NotNull(resource);
		Assert.NotNull(resource.Metadata);
		Assert.NotEmpty(resource.Metadata.Id);
		Assert.NotEmpty(resource.Metadata.Version);
		Assert.NotEmpty(resource.Metadata.Namespace);
	}

	/// <summary>
	/// Asserts that a resource was updated successfully
	/// </summary>
	/// <typeparam name="T">Resource type</typeparam>
	/// <param name="original">Original resource</param>
	/// <param name="updated">Updated resource</param>
	protected static void AssertResourceUpdated<T>(T original, T updated)
		where T : SideroLabs.Omni.Api.Resources.IOmniResource
	{
		Assert.NotNull(updated);
		Assert.Equal(original.Metadata.Id, updated.Metadata.Id);
		Assert.Equal(original.Metadata.Namespace, updated.Metadata.Namespace);
		Assert.NotEqual(original.Metadata.Version, updated.Metadata.Version);
	}

	/// <summary>
	/// Asserts that two resources are equal (same ID, namespace)
	/// </summary>
	/// <typeparam name="T">Resource type</typeparam>
	/// <param name="expected">Expected resource</param>
	/// <param name="actual">Actual resource</param>
	protected static void AssertResourceEquals<T>(T expected, T actual)
		where T : SideroLabs.Omni.Api.Resources.IOmniResource
	{
		Assert.NotNull(actual);
		Assert.Equal(expected.Metadata.Id, actual.Metadata.Id);
		Assert.Equal(expected.Metadata.Namespace, actual.Metadata.Namespace);
	}

	#endregion

	#region Safe Cleanup Helpers

	/// <summary>
	/// Safely deletes a resource, ignoring errors if it doesn't exist
	/// </summary>
	/// <typeparam name="T">Resource type</typeparam>
	/// <param name="id">Resource ID</param>
	/// <param name="ns">Resource namespace (default: "default")</param>
	protected async Task SafeDeleteResourceAsync<T>(string id, string? ns = "default")
		where T : SideroLabs.Omni.Api.Resources.IOmniResource, new()
	{
		try
		{
			await OmniClient.Resources.DeleteAsync<T>(id, ns, CancellationToken);
			Logger.LogInformation("✓ Cleaned up {Type}: {Id}", typeof(T).Name, id);
		}
		catch (Exception ex)
		{
			Logger.LogDebug(ex, "Safe delete: {Type} {Id} (may not exist)", typeof(T).Name, id);
		}
	}

	/// <summary>
	/// Safely deletes a user by email, ignoring errors if it doesn't exist
	/// </summary>
	/// <param name="email">User email</param>
	protected async Task SafeDeleteUserAsync(string email)
	{
		try
		{
			await OmniClient.Users.DeleteAsync(email, CancellationToken);
			Logger.LogInformation("✓ Cleaned up user: {Email}", email);
		}
		catch (Exception ex)
		{
			Logger.LogDebug(ex, "Safe delete: User {Email} (may not exist)", email);
		}
	}

	#endregion

	#region Integration Test Guards

	/// <summary>
	/// Determines if integration tests should run based on configuration
	/// </summary>
	/// <returns>True if integration tests should run</returns>
	protected bool ShouldRunIntegrationTests()
	{
		var omniSection = Configuration.GetSection("Omni");
		var endpoint = omniSection["BaseUrl"];
		var authToken = omniSection["AuthToken"];

		var shouldRun = !string.IsNullOrEmpty(endpoint) &&
					   !string.IsNullOrEmpty(authToken) &&
					   !endpoint.Contains("test.example.com") &&
					   !endpoint.Contains("localhost");

		if (!shouldRun)
		{
			Logger.LogWarning("Skipping integration tests - no valid Omni endpoint and credentials configured");
		}

		return shouldRun;
	}

	/// <summary>
	/// Determines if destructive tests should be skipped
	/// </summary>
	/// <returns>True if destructive tests should be skipped</returns>
	protected bool ShouldSkipDestructiveTests()
	{
		var testConfig = Configuration.GetSection("TestConfiguration");
		var skipDestructive = testConfig.GetValue<bool>("SkipDestructiveTests");

		if (skipDestructive)
		{
			Logger.LogInformation("Skipping destructive test - SkipDestructiveTests is true");
		}

		return skipDestructive;
	}

	#endregion

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
