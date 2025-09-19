using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using SideroLabs.Omni.Api.Tests.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for writable mode functionality to contrast with read-only mode
/// </summary>
public class WritableModeTests
{
	private readonly ILogger _logger;

	public WritableModeTests(ITestOutputHelper testOutputHelper)
	{
		var loggerFactory = new LoggerFactory()
			.AddXUnit(testOutputHelper);
		_logger = loggerFactory.CreateLogger<WritableModeTests>();
	}

	[Fact]
	public void OmniClient_WhenConfiguredWithWritableMode_ShouldNotBeReadOnly()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "test-key",
			IsReadOnly = false,
			Logger = _logger
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.IsReadOnly.Should().BeFalse("OmniClient should not be in read-only mode when IsReadOnly is false");
	}

	[Fact]
	public async Task WriteOperations_WhenInWritableMode_ShouldNotThrowReadOnlyModeException()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "test-key",
			IsReadOnly = false,
			Logger = _logger
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1)); // Short timeout for testing

		// Test data
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";
		const string testAccountName = "test-account";
		var testExtensions = new[] { "test-extension" };

		// Act & Assert - These should not throw ReadOnlyModeException
		// They may throw other exceptions (network, auth, etc.) but not ReadOnlyModeException

		await AssertNotReadOnlyException(() => 
			client.Management.CreateServiceAccountAsync(testPgpKey, cts.Token));

		await AssertNotReadOnlyException(() => 
			client.Management.RenewServiceAccountAsync(testAccountName, testPgpKey, cts.Token));

		await AssertNotReadOnlyException(() => 
			client.Management.DestroyServiceAccountAsync(testAccountName, cts.Token));

		await AssertNotReadOnlyException(() => 
			client.Management.CreateSchematicAsync(testExtensions, cts.Token));

		await AssertNotReadOnlyException(() => 
			client.Management.GetKubeConfigAsync(serviceAccount: true, cts.Token));

		// Test streaming operation (will throw on enumeration due to network, but not ReadOnlyModeException)
		var stream = client.Management.StreamKubernetesSyncManifestsAsync(dryRun: false, cts.Token);
		await AssertNotReadOnlyExceptionOnStream(stream);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void OmniClientOptions_IsReadOnlyProperty_ShouldReflectConfiguration(bool isReadOnly)
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "test-key",
			IsReadOnly = isReadOnly,
			Logger = _logger
		};

		// Act
		using var client = new OmniClient(options);

		// Assert
		client.IsReadOnly.Should().Be(isReadOnly);
	}

	[Fact]
	public void OmniClientOptions_DefaultIsReadOnly_ShouldBeFalse()
	{
		// Arrange
		var options = new OmniClientOptions
		{
			Endpoint = "https://test.example.com",
			Identity = "test-user",
			PgpPrivateKey = "test-key",
			Logger = _logger
		};

		// Act & Assert
		options.IsReadOnly.Should().BeFalse("IsReadOnly should default to false");

		using var client = new OmniClient(options);
		client.IsReadOnly.Should().BeFalse("Client IsReadOnly should default to false");
	}

	/// <summary>
	/// Helper method to verify that an operation does not throw ReadOnlyModeException
	/// It may throw other exceptions (network, auth, etc.) which is expected in tests
	/// </summary>
	private static async Task AssertNotReadOnlyException(Func<Task> operation)
	{
		try
		{
			await operation();
		}
		catch (ReadOnlyModeException ex)
		{
			false.Should().BeTrue($"Operation should not throw ReadOnlyModeException in writable mode. Exception: {ex.Message}");
		}
		catch
		{
			// Any other exception is expected (network errors, authentication errors, etc.)
			// We just want to ensure it's not a ReadOnlyModeException
		}
	}

	/// <summary>
	/// Helper method for testing streaming operations
	/// </summary>
	private static async Task AssertNotReadOnlyExceptionOnStream<T>(IAsyncEnumerable<T> stream)
	{
		try
		{
			await foreach (var _ in stream)
			{
				// We don't expect to actually get any items due to network issues
				break;
			}
		}
		catch (ReadOnlyModeException ex)
		{
			false.Should().BeTrue($"Streaming operation should not throw ReadOnlyModeException in writable mode. Exception: {ex.Message}");
		}
		catch
		{
			// Any other exception is expected (network errors, authentication errors, etc.)
			// We just want to ensure it's not a ReadOnlyModeException
		}
	}
}
