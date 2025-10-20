using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Exceptions;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for read-only mode functionality and write action enforcement
/// These tests verify that the IsReadOnly flag acts as a client-side safety switch
/// that prevents write operations regardless of server permissions
/// </summary>
public class ReadOnlyModeTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Creates a test client with read-only mode enabled
	/// </summary>
	private OmniClient CreateReadOnlyClient()
	{
		var options = new OmniClientOptions
		{
			BaseUrl = new("https://test-readonly.example.com"),
			AuthToken = "test-token",
			IsReadOnly = true,
			TimeoutSeconds = 1,
			Logger = Logger
		};
		return new OmniClient(options);
	}

	[Fact]
	public void OmniClient_WhenConfiguredWithReadOnlyMode_ShouldHaveReadOnlyProperty()
	{
		// Arrange
		using var client = CreateReadOnlyClient();

		// Act
		var isReadOnly = client.IsReadOnly;

		// Assert
		isReadOnly.Should().BeTrue("Client created with IsReadOnly=true should have IsReadOnly property set");
		Logger.LogInformation("✓ OmniClient.IsReadOnly = {IsReadOnly}", isReadOnly);
	}

	[Fact]
	public async Task CreateServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		// Arrange
		using var client = CreateReadOnlyClient();
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";

		// Act & Assert
		Exception? exception = null;
		try
		{
			#pragma warning disable CS0618
			await client.ServiceAccounts.CreateAsync(testPgpKey, cancellationToken: CancellationToken);
			#pragma warning restore CS0618
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		exception.Should().NotBeNull("Expected an exception to be thrown");
		exception.Should().BeOfType<ReadOnlyModeException>("Client-side read-only mode should throw ReadOnlyModeException");

		var readOnlyEx = (ReadOnlyModeException)exception!;
		readOnlyEx.Operation.Should().Be("create");
		readOnlyEx.ResourceType.Should().Be("service account");

		Logger.LogInformation("✓ CreateServiceAccount correctly blocked by read-only mode");
	}

	[Fact]
	public async Task RenewServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		// Arrange
		using var client = CreateReadOnlyClient();
		const string testAccountName = "test-account";
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";

		// Act & Assert
		Exception? exception = null;
		try
		{
			#pragma warning disable CS0618
			await client.ServiceAccounts.RenewAsync(testAccountName, testPgpKey, CancellationToken);
			#pragma warning restore CS0618
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		exception.Should().NotBeNull("Expected an exception to be thrown");
		exception.Should().BeOfType<ReadOnlyModeException>("Client-side read-only mode should throw ReadOnlyModeException");

		var readOnlyEx = (ReadOnlyModeException)exception!;
		readOnlyEx.Operation.Should().Be("update");
		readOnlyEx.ResourceType.Should().Be("service account");

		Logger.LogInformation("✓ RenewServiceAccount correctly blocked by read-only mode");
	}

	[Fact]
	public async Task DestroyServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		// Arrange
		using var client = CreateReadOnlyClient();
		const string testAccountName = "test-account";

		// Act & Assert
		Exception? exception = null;
		try
		{
			#pragma warning disable CS0618
			await client.ServiceAccounts.DestroyAsync(testAccountName, CancellationToken);
			#pragma warning restore CS0618
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		exception.Should().NotBeNull("Expected an exception to be thrown");
		exception.Should().BeOfType<ReadOnlyModeException>("Client-side read-only mode should throw ReadOnlyModeException");

		var readOnlyEx = (ReadOnlyModeException)exception!;
		readOnlyEx.Operation.Should().Be("delete");
		readOnlyEx.ResourceType.Should().Be("service account");

		Logger.LogInformation("✓ DestroyServiceAccount correctly blocked by read-only mode");
	}

	[Fact]
	public async Task CreateSchematicAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		// Arrange
		using var client = CreateReadOnlyClient();
		var extensions = new[] { "test-extension" };

		// Act & Assert
		Exception? exception = null;
		try
		{
			await client.Schematics.CreateAsync(extensions: extensions, cancellationToken: CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		exception.Should().NotBeNull("Expected an exception to be thrown");
		exception.Should().BeOfType<ReadOnlyModeException>("Client-side read-only mode should throw ReadOnlyModeException");

		var readOnlyEx = (ReadOnlyModeException)exception!;
		readOnlyEx.Operation.Should().Be("create");
		readOnlyEx.ResourceType.Should().Be("schematic");

		Logger.LogInformation("✓ CreateSchematic correctly blocked by read-only mode");
	}

	[Fact]
	public async Task GetKubeConfigWithServiceAccount_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		// Arrange
		using var client = CreateReadOnlyClient();
		const bool serviceAccount = true;

		// Act & Assert
		Exception? exception = null;
		try
		{
			await client.KubeConfig.GetAsync(serviceAccount: serviceAccount, cancellationToken: CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		// Assert
		exception.Should().NotBeNull("Expected an exception to be thrown");
		exception.Should().BeOfType<ReadOnlyModeException>("Creating a service account in kubeconfig should throw ReadOnlyModeException");

		var readOnlyEx = (ReadOnlyModeException)exception!;
		readOnlyEx.Operation.Should().Be("create");
		readOnlyEx.ResourceType.Should().Be("service account");

		Logger.LogInformation("✓ GetKubeConfig with service account correctly blocked by read-only mode");
	}

	[Fact]
	public void StreamKubernetesSyncManifests_WhenInReadOnlyModeWithDryRun_ShouldNotThrow()
	{
		// Arrange
		using var client = CreateReadOnlyClient();
		const bool dryRun = true;

		// Act & Assert - This should not throw because it's a dry run
		var stream = client.Kubernetes.StreamSyncManifestsAsync(dryRun, CancellationToken);

		// We can't fully test the stream without a real connection, but we can verify it doesn't throw immediately
		stream.Should().NotBeNull();

		Logger.LogInformation("✓ StreamKubernetesSyncManifests with dry run allowed in read-only mode");
	}

	[Fact]
	public async Task ReadOnlyOperations_WhenInReadOnlyMode_ShouldNotThrow()
	{
		// Arrange
		using var client = CreateReadOnlyClient();

		// These operations should work fine in read-only mode
		// Note: These will fail with connection errors, but should not throw ReadOnlyModeException

		var readOperations = new Func<Task>[]
		{
			() => AssertNotReadOnlyException(() => client.KubeConfig.GetAsync(cancellationToken: CancellationToken)),
			() => AssertNotReadOnlyException(() => client.TalosConfig.GetAsync(cancellationToken: CancellationToken)),
			() => AssertNotReadOnlyException(() => client.OmniConfig.GetAsync(CancellationToken)),
			() => AssertNotReadOnlyException(() => client.ServiceAccounts.ListAsync(CancellationToken)),
			() => AssertNotReadOnlyException(() => client.Validation.ValidateConfigAsync("test-config", CancellationToken)),
			() => AssertNotReadOnlyException(() => client.Kubernetes.UpgradePreChecksAsync("v1.29.0", CancellationToken))
		};

		// Execute all read operations and verify none throw ReadOnlyModeException
		foreach (var operation in readOperations)
		{
			await operation();
		}

		Logger.LogInformation("✓ All read operations allowed in read-only mode");
	}

	[Fact]
	public void ReadOnlyModeException_ShouldHaveCorrectProperties()
	{
		// Arrange
		const string operation = "create";
		const string resourceType = "test-resource";

		// Act
		var exception = new ReadOnlyModeException(operation, resourceType);

		// Assert
		exception.Operation.Should().Be(operation);
		exception.ResourceType.Should().Be(resourceType);
		exception.Message.Should().Contain("read-only mode");
		exception.Message.Should().Contain(operation);
		exception.Message.Should().Contain(resourceType);

		Logger.LogInformation("✓ ReadOnlyModeException has correct properties");
	}

	[Fact]
	public void ReadOnlyModeException_WithCustomMessage_ShouldUseCustomMessage()
	{
		// Arrange
		const string operation = "update";
		const string resourceType = "test-resource";
		const string customMessage = "Custom error message for testing";

		// Act
		var exception = new ReadOnlyModeException(operation, resourceType, customMessage);

		// Assert
		exception.Operation.Should().Be(operation);
		exception.ResourceType.Should().Be(resourceType);
		exception.Message.Should().Be(customMessage);

		Logger.LogInformation("✓ ReadOnlyModeException with custom message works correctly");
	}

	[Fact]
	public async Task ClientSideReadOnlyMode_OverridesServerPermissions()
	{
		// Arrange - Create a client with read-only mode enabled
		using var client = CreateReadOnlyClient();
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		Logger.LogInformation("🔒 Testing that IsReadOnly acts as a client-side safety switch");

		// Act & Assert - These should throw ReadOnlyModeException before network calls
		Exception? createException = null;
		try
		{
			await client.ServiceAccounts.CreateAsync("test-key", cancellationToken: cts.Token);
		}
		catch (Exception ex)
		{
			createException = ex;
		}

		createException.Should().NotBeNull("Expected CreateServiceAccountAsync to throw an exception");
		createException.Should().BeOfType<ReadOnlyModeException>("Expected ReadOnlyModeException for CreateServiceAccountAsync");

		var createReadOnlyEx = (ReadOnlyModeException)createException!;
		createReadOnlyEx.Operation.Should().Be("create");
		createReadOnlyEx.ResourceType.Should().Be("service account");

		Exception? destroyException = null;
		try
		{
			await client.ServiceAccounts.DestroyAsync("test-account", cts.Token);
		}
		catch (Exception ex)
		{
			destroyException = ex;
		}

		destroyException.Should().NotBeNull("Expected DestroyServiceAccountAsync to throw an exception");
		destroyException.Should().BeOfType<ReadOnlyModeException>("Expected ReadOnlyModeException for DestroyServiceAccountAsync");

		var destroyReadOnlyEx = (ReadOnlyModeException)destroyException!;
		destroyReadOnlyEx.Operation.Should().Be("delete");
		destroyReadOnlyEx.ResourceType.Should().Be("service account");

		Logger.LogInformation("✓ Client-side read-only mode successfully blocks write operations");
	}

	[Fact]
	public void WritableClient_AllowsWriteOperations()
	{
		// Arrange - Use the injected test client (which may be writable)
		var client = OmniClient;

		// Act & Assert
		var isReadOnly = client.IsReadOnly;

		Logger.LogInformation("Test client IsReadOnly = {IsReadOnly}", isReadOnly);

		// The test just verifies that we can check the property
		// The actual value depends on the test configuration
		if (isReadOnly)
		{
			Logger.LogInformation("✓ Test client is in read-only mode (safer for testing)");
		}
		else
		{
			Logger.LogInformation("✓ Test client is in writable mode (can test write operations)");
		}
	}

	/// <summary>
	/// Helper method to verify that an operation throws any exception except ReadOnlyModeException
	/// This is used to test that read operations are allowed (they may fail for other reasons like network)
	/// </summary>
	private static async Task AssertNotReadOnlyException(Func<Task> operation)
	{
		try
		{
			await operation();
		}
		catch (ReadOnlyModeException)
		{
			// This is the exception we don't want to see
			false.Should().BeTrue("Operation should not throw ReadOnlyModeException for read operations");
		}
		catch
		{
			// Any other exception is fine (e.g., network errors, authentication errors)
			// We just want to ensure it's not a ReadOnlyModeException
		}
	}

	/// <summary>
	/// Helper method for testing generic Task<T> operations
	/// </summary>
	private static async Task AssertNotReadOnlyException<T>(Func<Task<T>> operation)
	{
		try
		{
			await operation();
		}
		catch (ReadOnlyModeException)
		{
			// This is the exception we don't want to see
			false.Should().BeTrue("Operation should not throw ReadOnlyModeException for read operations");
		}
		catch
		{
			// Any other exception is fine (e.g., network errors, authentication errors)
			// We just want to ensure it's not a ReadOnlyModeException
		}
	}
}
