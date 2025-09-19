using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Extensions;
using SideroLabs.Omni.Api.Interfaces;
using SideroLabs.Omni.Api.Security;
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

		// Build configuration from multiple sources
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables("OMNI_")
			.AddUserSecrets<TestBase>()
			.Build();

		// Set up dependency injection
		var services = new ServiceCollection();

		// Register configuration
		services.AddSingleton<IConfiguration>(configuration);

		// Configure OmniClientOptions from configuration and test PGP key
		services.Configure<OmniClientOptions>(options =>
		{
			configuration.GetSection("Omni").Bind(options);

			// Extract PGP key from test file for non-destructive testing
			var testPgpKey = ExtractTestPgpKey();
			if (testPgpKey.HasValue)
			{
				options.Identity = testPgpKey.Value.Identity;
				options.PgpPrivateKey = testPgpKey.Value.PgpKey;
				// Clear file path since we're using direct key content
				options.PgpKeyFilePath = null;
			}

			// Set logger for OmniClient
			options.Logger = Logger;
		});

		// Add the Omni client with the configured options
		services.AddOmniClient(opts =>
		{
			configuration.GetSection("Omni").Bind(opts);

			// Extract PGP key from test file for non-destructive testing
			var testPgpKey = ExtractTestPgpKey();
			if (testPgpKey.HasValue)
			{
				opts.Identity = testPgpKey.Value.Identity;
				opts.PgpPrivateKey = testPgpKey.Value.PgpKey;
				// Clear file path since we're using direct key content
				opts.PgpKeyFilePath = null;
			}

			// Set logger for OmniClient
			opts.Logger = Logger;
		});

		_serviceProvider = services.BuildServiceProvider();
	}

	/// <summary>
	/// Extracts PGP key from test data file for safe testing
	/// </summary>
	private (string Identity, string PgpKey)? ExtractTestPgpKey()
	{
		try
		{
			var testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
			var testKeyFile = Path.Combine(testDataDirectory, "pgp-key-test.txt");

			if (!File.Exists(testKeyFile))
			{
				Logger.LogWarning("Test PGP key file not found: {TestKeyFile}", testKeyFile);
				return null;
			}

			var fileContents = File.ReadAllText(testKeyFile);
			var decodedBytes = Convert.FromBase64String(fileContents);
			var decodedString = Encoding.UTF8.GetString(decodedBytes);

			using var jsonDoc = JsonDocument.Parse(decodedString);
			var root = jsonDoc.RootElement;

			if (root.TryGetProperty("name", out var nameElement) &&
				root.TryGetProperty("pgp_key", out var pgpKeyElement))
			{
				var identity = nameElement.GetString();
				var pgpKey = pgpKeyElement.GetString();

				if (!string.IsNullOrEmpty(identity) && !string.IsNullOrEmpty(pgpKey))
				{
					Logger.LogInformation("Extracted test PGP key for identity: {Identity}", identity);
					return (identity, pgpKey);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to extract test PGP key, tests may not have authentication");
		}

		return null;
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
