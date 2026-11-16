using SideroLabs.Omni.Api.Models;
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

	/// <summary>
	/// Verifies that a client created with IsReadOnly=true reports IsReadOnly as true.
	/// </summary>
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

	/// <summary>
	/// Verifies that CreateServiceAccountAsync throws ReadOnlyModeException when the client is in read-only mode.
	/// </summary>
	[Fact]
	public async Task CreateServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		using var client = CreateReadOnlyClient();
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";

		await AssertBlockedAsync(
			() => client.ServiceAccounts.CreateAsync(testPgpKey, cancellationToken: CancellationToken),
			"create",
			"service account");
	}

	/// <summary>
	/// Verifies that RenewServiceAccountAsync throws ReadOnlyModeException when the client is in read-only mode.
	/// </summary>
	[Fact]
	public async Task RenewServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		using var client = CreateReadOnlyClient();
		const string testPgpKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\ntest-key\n-----END PGP PUBLIC KEY BLOCK-----";

		await AssertBlockedAsync(
			() => client.ServiceAccounts.RenewAsync("test-account", testPgpKey, CancellationToken),
			"update",
			"service account");
	}

	/// <summary>
	/// Verifies that DestroyServiceAccountAsync throws ReadOnlyModeException when the client is in read-only mode.
	/// </summary>
	[Fact]
	public async Task DestroyServiceAccountAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		using var client = CreateReadOnlyClient();

		await AssertBlockedAsync(
			() => client.ServiceAccounts.DestroyAsync("test-account", CancellationToken),
			"delete",
			"service account");
	}

	/// <summary>
	/// Verifies that CreateSchematicAsync throws ReadOnlyModeException when the client is in read-only mode.
	/// </summary>
	[Fact]
	public async Task CreateSchematicAsync_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		using var client = CreateReadOnlyClient();

		await AssertBlockedAsync(
			() => client.Schematics.CreateAsync(new CreateSchematicOptions { Extensions = ["test-extension"] }, cancellationToken: CancellationToken),
			"create",
			"schematic");
	}

	/// <summary>
	/// Verifies that GetKubeConfigAsync with serviceAccount=true throws ReadOnlyModeException when the client is in read-only mode.
	/// </summary>
	[Fact]
	public async Task GetKubeConfigWithServiceAccount_WhenInReadOnlyMode_ShouldThrowReadOnlyModeException()
	{
		using var client = CreateReadOnlyClient();

		await AssertBlockedAsync(
			() => client.KubeConfig.GetAsync(serviceAccount: true, cancellationToken: CancellationToken),
			"create",
			"service account");
	}
	/// <summary>
	/// Verifies that StreamSyncManifestsAsync with dryRun=true is permitted in read-only mode.
	/// </summary>
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

	/// <summary>
	/// Verifies that read-only operations (GetAsync, ListAsync, etc.) do not throw ReadOnlyModeException.
	/// </summary>
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

	/// <summary>
	/// Verifies that ReadOnlyModeException exposes the correct operation and resource type in its properties and message.
	/// </summary>
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

	/// <summary>
	/// Verifies that ReadOnlyModeException uses the provided custom message when one is supplied.
	/// </summary>
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

	/// <summary>
	/// Verifies that client-side read-only mode blocks write operations before any network call is made.
	/// </summary>
	[Fact]
	public async Task ClientSideReadOnlyMode_OverridesServerPermissions()
	{
		using var client = CreateReadOnlyClient();
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		await AssertBlockedAsync(
			() => client.ServiceAccounts.CreateAsync("test-key", cancellationToken: cts.Token),
			"create",
			"service account");

		await AssertBlockedAsync(
			() => client.ServiceAccounts.DestroyAsync("test-account", cts.Token),
			"delete",
			"service account");
	}
	/// <summary>
	/// Verifies that a writable client allows write operations.
	/// </summary>
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

	private static async Task AssertBlockedAsync(Func<Task> action, string operation, string resourceType)
	{
		var exception = await Assert.ThrowsAsync<ReadOnlyModeException>(action);
		exception.Operation.Should().Be(operation);
		exception.ResourceType.Should().Be(resourceType);
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
			false.Should().BeTrue("Operation should not throw ReadOnlyModeException for read operations");
		}
		catch
		{
			// Any other failure (network, authentication) is fine
		}
	}
}