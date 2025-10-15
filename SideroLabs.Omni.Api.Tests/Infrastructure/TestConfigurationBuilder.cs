using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SideroLabs.Omni.Api.Tests.Infrastructure;

/// <summary>
/// Builder for test configurations
/// </summary>
/// <param name="logger">Logger instance</param>
public class TestConfigurationBuilder(ILogger logger)
{
	private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

	/// <summary>
	/// Builds configuration from multiple sources
	/// </summary>
	/// <returns>Built configuration</returns>
	public IConfiguration BuildConfiguration()
	{
		_logger.LogInformation("Building test configuration");

		return new ConfigurationBuilder()
		.SetBasePath(Directory.GetCurrentDirectory())
		.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
		.AddEnvironmentVariables("OMNI_")
		.AddUserSecrets<TestConfigurationBuilder>()
		.Build();
	}

	/// <summary>
	/// Gets test expectations from configuration
	/// </summary>
	/// <param name="configuration">Configuration instance</param>
	/// <returns>Test expectations</returns>
	public TestExpectations GetTestExpectations(IConfiguration configuration)
	{
		_logger.LogInformation("Loading test expectations from configuration");

		return configuration.GetSection("TestExpectations").Get<TestExpectations>()
			?? throw new InvalidOperationException("Failed to load TestExpectations from configuration");
	}

	/// <summary>
	/// Extracts PGP key from test data file for safe testing
	/// </summary>
	/// <returns>PGP key information if available</returns>
	public (string Identity, string PgpKey)? ExtractTestPgpKey()
	{
		try
		{
			var testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
			var testKeyFile = Path.Combine(testDataDirectory, "pgp-key-test.txt");

			if (!File.Exists(testKeyFile))
			{
				_logger.LogWarning("Test PGP key file not found: {TestKeyFile}", testKeyFile);
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
					_logger.LogInformation("Extracted test PGP key for identity: {Identity}", identity);
					return (identity, pgpKey);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to extract test PGP key, tests may not have authentication");
		}

		return null;
	}

	/// <summary>
	/// Configures OmniClientOptions with test settings
	/// </summary>
	/// <param name="configuration">Configuration instance</param>
	/// <param name="options">Options to configure</param>
	public void ConfigureOmniClientOptions(IConfiguration configuration, OmniClientOptions options)
	{
		configuration.GetSection("Omni").Bind(options);

		// Extract PGP key from test file for non-destructive testing
		var testPgpKey = ExtractTestPgpKey();
		if (testPgpKey.HasValue)
		{
			options.Identity = testPgpKey.Value.Identity;
			options.PgpPrivateKey = testPgpKey.Value.PgpKey;
			// Clear other auth methods since we're using direct key content
			options.PgpKeyFilePath = null;
			options.AuthToken = null;
		}

		// Set logger for OmniClient
		options.Logger = _logger;
	}
}
