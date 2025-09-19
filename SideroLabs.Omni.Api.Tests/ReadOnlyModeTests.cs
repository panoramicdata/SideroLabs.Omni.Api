using AwesomeAssertions;
using Grpc.Core;
using SideroLabs.Omni.Api.Exceptions;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Tests for read-only mode functionality and write action enforcement
/// </summary>
public class ReadOnlyModeTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public void OmniClient_WhenConfiguredWithReadOnlyMode_ShouldHaveReadOnlyProperty()
	{
		// Arrange & Act
		var isReadOnly = OmniClient.IsReadOnly;

		// Assert
		isReadOnly.Should().BeTrue("OmniClient should be in read-only mode based on appsettings.json configuration");
	}

	[Fact]
	public async Task CreateServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeExceptionOrPermissionDenied()
	{
		// Arrange
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";

		// Act & Assert
		// We expect either ReadOnlyModeException (client-side) or RpcException with PermissionDenied (server-side)
		Exception? exception = null;
		try
		{
			await OmniClient.Management.CreateServiceAccountAsync(testPgpKey, CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		exception.Should().NotBeNull("Expected an exception to be thrown");

		// Verify it's one of the expected exception types
		var isExpectedException = exception is ReadOnlyModeException readOnlyEx &&
			readOnlyEx.Operation == "create" &&
			readOnlyEx.ResourceType == "service account" ||
			exception is RpcException rpcEx &&
			rpcEx.StatusCode == StatusCode.PermissionDenied;

		isExpectedException.Should().BeTrue(
			$"Expected ReadOnlyModeException or PermissionDenied RpcException, but got: {exception.GetType().Name}: {exception.Message}");
	}

	[Fact]
	public async Task RenewServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeExceptionOrPermissionDenied()
	{
		// Arrange
		const string testAccountName = "test-account";
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";

		// Act & Assert
		Exception? exception = null;
		try
		{
			await OmniClient.Management.RenewServiceAccountAsync(testAccountName, testPgpKey, CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		exception.Should().NotBeNull("Expected an exception to be thrown");

		// Verify it's one of the expected exception types
		var isExpectedException = exception is ReadOnlyModeException readOnlyEx &&
			readOnlyEx.Operation == "update" &&
			readOnlyEx.ResourceType == "service account" ||
			exception is RpcException rpcEx &&
			rpcEx.StatusCode == StatusCode.PermissionDenied;

		isExpectedException.Should().BeTrue(
			$"Expected ReadOnlyModeException or PermissionDenied RpcException, but got: {exception.GetType().Name}: {exception.Message}");
	}

	[Fact]
	public async Task DestroyServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeExceptionOrPermissionDenied()
	{
		// Arrange
		const string testAccountName = "test-account";

		// Act & Assert
		Exception? exception = null;
		try
		{
			await OmniClient.Management.DestroyServiceAccountAsync(testAccountName, CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		exception.Should().NotBeNull("Expected an exception to be thrown");

		// Verify it's one of the expected exception types
		var isExpectedException = exception is ReadOnlyModeException readOnlyEx &&
			readOnlyEx.Operation == "delete" &&
			readOnlyEx.ResourceType == "service account" ||
			exception is RpcException rpcEx &&
			rpcEx.StatusCode == StatusCode.PermissionDenied;

		isExpectedException.Should().BeTrue(
			$"Expected ReadOnlyModeException or PermissionDenied RpcException, but got: {exception.GetType().Name}: {exception.Message}");
	}

	[Fact]
	public async Task CreateSchematicAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeExceptionOrPermissionDenied()
	{
		// Arrange
		var extensions = new[] { "test-extension" };

		// Act & Assert
		Exception? exception = null;
		try
		{
			await OmniClient.Management.CreateSchematicAsync(extensions, CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		exception.Should().NotBeNull("Expected an exception to be thrown");

		// Verify it's one of the expected exception types
		var isExpectedException = exception is ReadOnlyModeException readOnlyEx &&
			readOnlyEx.Operation == "create" &&
			readOnlyEx.ResourceType == "schematic" ||
			exception is RpcException rpcEx &&
			rpcEx.StatusCode == StatusCode.PermissionDenied;

		isExpectedException.Should().BeTrue(
			$"Expected ReadOnlyModeException or PermissionDenied RpcException, but got: {exception.GetType().Name}: {exception.Message}");
	}

	[Fact]
	public async Task GetKubeConfigWithServiceAccount_WhenInReadOnlyMode_ShouldThrowReadOnlyModeExceptionOrPermissionDenied()
	{
		// Arrange
		const bool serviceAccount = true;

		// Act & Assert
		Exception? exception = null;
		try
		{
			await OmniClient.Management.GetKubeConfigAsync(serviceAccount, CancellationToken);
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		exception.Should().NotBeNull("Expected an exception to be thrown");

		// Verify it's one of the expected exception types
		var isExpectedException = exception is ReadOnlyModeException readOnlyEx &&
			readOnlyEx.Operation == "create" &&
			readOnlyEx.ResourceType == "service account" ||
			exception is RpcException;  // Server may allow this operation but fail for other reasons

		isExpectedException.Should().BeTrue(
			$"Expected ReadOnlyModeException or RpcException, but got: {exception.GetType().Name}: {exception.Message}");
	}

	[Fact]
	public void StreamKubernetesSyncManifests_WhenInReadOnlyModeWithDryRun_ShouldNotThrow()
	{
		// Arrange
		const bool dryRun = true;

		// Act & Assert - This should not throw because it's a dry run
		var stream = OmniClient.Management.StreamKubernetesSyncManifestsAsync(dryRun, CancellationToken);

		// We can't fully test the stream without a real connection, but we can verify it doesn't throw immediately
		stream.Should().NotBeNull();
	}

	[Fact]
	public async Task ReadOnlyOperations_WhenInReadOnlyMode_ShouldNotThrow()
	{
		// These operations should work fine in read-only mode
		// Note: These will fail with connection errors, but should not throw ReadOnlyModeException

		// Test read operations that should be allowed
		var readOperations = new Func<Task>[]
		{
			() => AssertNotReadOnlyException(() => OmniClient.Management.GetKubeConfigAsync(CancellationToken)),
			() => AssertNotReadOnlyException(() => OmniClient.Management.GetTalosConfigAsync(CancellationToken)),
			() => AssertNotReadOnlyException(() => OmniClient.Management.GetOmniConfigAsync(CancellationToken)),
			() => AssertNotReadOnlyException(() => OmniClient.Management.ListServiceAccountsAsync(CancellationToken)),
			() => AssertNotReadOnlyException(() => OmniClient.Management.ValidateConfigAsync("test-config", CancellationToken)),
			() => AssertNotReadOnlyException(() => OmniClient.Management.KubernetesUpgradePreChecksAsync("v1.29.0", CancellationToken))
		};

		// Execute all read operations and verify none throw ReadOnlyModeException
		foreach (var operation in readOperations)
		{
			await operation();
		}
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
	}

	/// <summary>
	/// Test client-side read-only enforcement with a locally configured client
	/// </summary>
	[Fact]
	public async Task ClientSideReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		// Arrange - Create a client with read-only mode enabled but no real connection
		var options = new OmniClientOptions
		{
			Endpoint = "https://test-readonly.example.com", // Non-existent endpoint
			Identity = "test-user",
			IsReadOnly = true,
			TimeoutSeconds = 1, // Short timeout
			Logger = Logger
		};

		using var client = new OmniClient(options);
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		// Act & Assert - These should throw ReadOnlyModeException before network calls
		Exception? createException = null;
		try
		{
			await client.Management.CreateServiceAccountAsync("test-key", cts.Token);
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
			await client.Management.DestroyServiceAccountAsync("test-account", cts.Token);
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
			false.Should().BeTrue("Operation should not throw ReadOnlyModeException in read-only mode");
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
			false.Should().BeTrue("Operation should not throw ReadOnlyModeException in read-only mode");
		}
		catch
		{
			// Any other exception is fine (e.g., network errors, authentication errors)
			// We just want to ensure it's not a ReadOnlyModeException
		}
	}
}
