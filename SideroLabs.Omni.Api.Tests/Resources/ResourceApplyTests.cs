using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for Apply operations on Omni resources
/// Tests declarative resource management (create-or-update)
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ResourceApplyTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task Apply_NewResource_CreatesResource()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("apply-test");
		
		var cluster = new Api.Builders.ClusterBuilder(clusterId)
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		try
		{
			// Act
			Logger.LogInformation("🔍 Applying new cluster: {ClusterId}", clusterId);
			var applied = await client.Resources.ApplyAsync(cluster, dryRun: false, cancellationToken: CancellationToken);

			// Assert
			applied.Should().NotBeNull();
			applied.Metadata.Id.Should().Be(clusterId);
			Logger.LogInformation("✅ Successfully applied (created) cluster: {ClusterId}", clusterId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupCluster(client, clusterId);
		}
	}

	[Fact]
	public async Task Apply_ExistingResource_UpdatesResource()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("apply-update-test");
		
		// Create initial cluster
		var cluster = new Api.Builders.ClusterBuilder(clusterId)
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		try
		{
			var created = await client.Resources.CreateAsync(cluster, CancellationToken);
			Logger.LogInformation("Created cluster for test: {ClusterId}", clusterId);

			// Act - Apply with modified version
			created.Metadata.Labels ??= new Dictionary<string, string>();
			created.Metadata.Labels["updated"] = "true";
			
			var applied = await client.Resources.ApplyAsync(created, dryRun: false, cancellationToken: CancellationToken);

			// Assert
			applied.Should().NotBeNull();
			applied.Metadata.Labels.Should().ContainKey("updated");
			applied.Metadata.Labels["updated"].Should().Be("true");
			applied.Metadata.Version.Should().NotBe(created.Metadata.Version, "Version should increment on update");
			
			Logger.LogInformation("✅ Successfully applied (updated) cluster: {ClusterId}", clusterId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupCluster(client, clusterId);
		}
	}

	[Fact]
	public async Task ApplyYaml_ValidYaml_CreatesResource()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("yaml-test");
		
		var yaml = $@"
metadata:
  id: {clusterId}
  namespace: default
  type: Clusters.omni.sidero.dev
  version: v1alpha1
spec:
  kubernetesVersion: v1.29.0
  talosVersion: v1.7.0
  installImage: ghcr.io/siderolabs/installer:v1.7.0
";

		try
		{
			// Act
			Logger.LogInformation("🔍 Applying cluster from YAML: {ClusterId}", clusterId);
			var applied = await client.Resources.ApplyYamlAsync<Api.Resources.Cluster>(yaml, dryRun: false, cancellationToken: CancellationToken);

			// Assert
			applied.Should().NotBeNull();
			applied.Metadata.Id.Should().Be(clusterId);
			Logger.LogInformation("✅ Successfully applied cluster from YAML: {ClusterId}", clusterId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to apply YAML");
			throw;
		}
		finally
		{
			// Cleanup
			await CleanupCluster(client, clusterId);
		}
	}

	[Fact]
	public async Task ApplyYaml_InvalidYaml_ThrowsException()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		
		var invalidYaml = @"
this is not: valid yaml
  - with invalid
    structure:
";

		// Act & Assert
		await Assert.ThrowsAsync<Exception>(async () =>
		{
			await client.Resources.ApplyYamlAsync<Api.Resources.Cluster>(invalidYaml, dryRun: false, cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ Correctly rejected invalid YAML");
	}

	[Fact]
	public async Task ApplyFile_ValidFile_CreatesResource()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("file-test");
		var tempFile = Path.GetTempFileName();
		
		var yaml = $@"
metadata:
  id: {clusterId}
  namespace: default
  type: Clusters.omni.sidero.dev
  version: v1alpha1
spec:
  kubernetesVersion: v1.29.0
  talosVersion: v1.7.0
  installImage: ghcr.io/siderolabs/installer:v1.7.0
";

		try
		{
			// Write YAML to temp file
			await File.WriteAllTextAsync(tempFile, yaml, CancellationToken);

			// Act
			Logger.LogInformation("🔍 Applying cluster from file: {ClusterId}", clusterId);
			var applied = await client.Resources.ApplyFileAsync<Api.Resources.Cluster>(tempFile, dryRun: false, cancellationToken: CancellationToken);

			// Assert
			applied.Should().NotBeNull();
			applied.Metadata.Id.Should().Be(clusterId);
			Logger.LogInformation("✅ Successfully applied cluster from file: {ClusterId}", clusterId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupCluster(client, clusterId);
			if (File.Exists(tempFile))
			{
				File.Delete(tempFile);
			}
		}
	}

	[Fact]
	public async Task ApplyFile_NonExistentFile_ThrowsException()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var nonExistentFile = Path.Combine(Path.GetTempPath(), $"non-existent-{Guid.NewGuid()}.yaml");

		// Act & Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () =>
		{
			await client.Resources.ApplyFileAsync<Api.Resources.Cluster>(nonExistentFile, dryRun: false, cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ Correctly rejected non-existent file");
	}

	[Fact]
	public async Task Apply_Idempotent_MultipleApplicationsSameResult()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("idempotent-test");
		
		var cluster = new Api.Builders.ClusterBuilder(clusterId)
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		try
		{
			// Act - Apply same resource multiple times
			Logger.LogInformation("🔍 Applying cluster multiple times: {ClusterId}", clusterId);
			var applied1 = await client.Resources.ApplyAsync(cluster, dryRun: false, cancellationToken: CancellationToken);
			var applied2 = await client.Resources.ApplyAsync(applied1, dryRun: false, cancellationToken: CancellationToken);
			var applied3 = await client.Resources.ApplyAsync(applied2, dryRun: false, cancellationToken: CancellationToken);

			// Assert - Should all succeed
			applied1.Should().NotBeNull();
			applied2.Should().NotBeNull();
			applied3.Should().NotBeNull();
			
			applied1.Metadata.Id.Should().Be(clusterId);
			applied2.Metadata.Id.Should().Be(clusterId);
			applied3.Metadata.Id.Should().Be(clusterId);
			
			Logger.LogInformation("✅ Apply is idempotent - multiple applications succeeded");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupCluster(client, clusterId);
		}
	}

	private OmniClientOptions GetClientOptions()
	{
		var configuration = Configuration;
		var omniSection = configuration.GetSection("Omni");
		var authToken = omniSection["AuthToken"] ?? throw new FormatException("Omni config section must contain AuthToken.");

		return new OmniClientOptions
		{
			BaseUrl = new(omniSection["BaseUrl"] ?? throw new InvalidOperationException("Omni:BaseUrl not configured")),
			AuthToken = authToken,
			TimeoutSeconds = int.Parse(omniSection["TimeoutSeconds"] ?? "30"),
			UseTls = bool.Parse(omniSection["UseTls"] ?? "true"),
			ValidateCertificate = bool.Parse(omniSection["ValidateCertificate"] ?? "true"),
			IsReadOnly = bool.Parse(omniSection["IsReadOnly"] ?? "false"),
			Logger = Logger
		};
	}

	private static string CreateUniqueId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

	private async Task CleanupCluster(OmniClient client, string clusterId)
	{
		try
		{
			await client.Resources.DeleteAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test cluster: {ClusterId}", clusterId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			// Already deleted, that's fine
			Logger.LogDebug("Test cluster already deleted: {ClusterId}", clusterId);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test cluster: {ClusterId}", clusterId);
		}
	}
}
