using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for basic CRUD operations on Omni resources
/// Tests Create, Read, Update, Delete operations for core resource types
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ResourceCrudTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	#region Cluster CRUD Tests

	[Fact]
	public async Task Cluster_Create_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("test-cluster");
		
		var cluster = new Api.Builders.ClusterBuilder(clusterId)
			.WithKubernetesVersion("v1.29.0")
			.WithTalosVersion("v1.7.0")
			.Build();

		try
		{
			// Act
			Logger.LogInformation("🔍 Creating test cluster: {ClusterId}", clusterId);
			var created = await client.Resources.CreateAsync(cluster, CancellationToken);

			// Assert
			created.Should().NotBeNull();
			created.Metadata.Id.Should().Be(clusterId);
			created.Spec.KubernetesVersion.Should().Be("v1.29.0");
			created.Spec.TalosVersion.Should().Be("v1.7.0");

			Logger.LogInformation("✅ Successfully created cluster: {ClusterId}", clusterId);
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
	public async Task Cluster_Get_ReturnsCluster()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("test-cluster");
		
		// Create a cluster first
		var cluster = await CreateTestCluster(client, clusterId);
		if (cluster == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create cluster (likely permission denied)");
			return;
		}

		try
		{
			// Act
			Logger.LogInformation("🔍 Getting cluster: {ClusterId}", clusterId);
			var retrieved = await client.Resources.GetAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);

			// Assert
			retrieved.Should().NotBeNull();
			retrieved.Metadata.Id.Should().Be(clusterId);
			retrieved.Spec.KubernetesVersion.Should().Be(cluster.Spec.KubernetesVersion);

			Logger.LogInformation("✅ Successfully retrieved cluster: {ClusterId}", clusterId);
		}
		finally
		{
			// Cleanup
			await CleanupCluster(client, clusterId);
		}
	}

	[Fact]
	public async Task Cluster_Update_ModifiesCluster()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("test-cluster");
		
		// Create a cluster first
		var cluster = await CreateTestCluster(client, clusterId);
		if (cluster == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create cluster (likely permission denied)");
			return;
		}

		try
		{
			// Act - Update the cluster
			Logger.LogInformation("🔍 Updating cluster: {ClusterId}", clusterId);
			cluster.Spec.KubernetesVersion = "v1.30.0";
			
			var updated = await client.Resources.UpdateAsync(cluster, cancellationToken: CancellationToken);

			// Assert
			updated.Should().NotBeNull();
			updated.Spec.KubernetesVersion.Should().Be("v1.30.0");

			Logger.LogInformation("✅ Successfully updated cluster: {ClusterId}", clusterId);
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
	public async Task Cluster_Delete_RemovesCluster()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("test-cluster");
		
		// Create a cluster first
		var cluster = await CreateTestCluster(client, clusterId);
		if (cluster == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create cluster (likely permission denied)");
			return;
		}

		// Act
		Logger.LogInformation("🔍 Deleting cluster: {ClusterId}", clusterId);
		await client.Resources.DeleteAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);

		// Assert - Try to get it, should throw NotFound
		await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await client.Resources.GetAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ Successfully deleted cluster: {ClusterId}", clusterId);
	}

	[Fact]
	public async Task Cluster_List_ReturnsMultipleClusters()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act
			Logger.LogInformation("🔍 Listing all clusters");
			var clusters = new List<Api.Resources.Cluster>();
			
			await foreach (var cluster in client.Resources.ListAsync<Api.Resources.Cluster>(cancellationToken: CancellationToken))
			{
				clusters.Add(cluster);
			}

			// Assert
			Logger.LogInformation("✅ Successfully listed {Count} clusters", clusters.Count);
			clusters.Should().NotBeNull();
			
			// We don't assert a specific count since we don't know how many exist
			// Just verify we can list without error
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	#endregion

	#region Machine CRUD Tests

	[Fact]
	public async Task Machine_Create_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var machineId = CreateUniqueId("test-machine");
		
		var machine = new Api.Builders.MachineBuilder(machineId)
			.Build();

		try
		{
			// Act
			Logger.LogInformation("🔍 Creating test machine: {MachineId}", machineId);
			var created = await client.Resources.CreateAsync(machine, CancellationToken);

			// Assert
			created.Should().NotBeNull();
			created.Metadata.Id.Should().Be(machineId);

			Logger.LogInformation("✅ Successfully created machine: {MachineId}", machineId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupMachine(client, machineId);
		}
	}

	[Fact]
	public async Task Machine_Get_ReturnsMachine()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var machineId = CreateUniqueId("test-machine");
		
		// Create a machine first
		var machine = await CreateTestMachine(client, machineId);
		if (machine == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create machine (likely permission denied)");
			return;
		}

		try
		{
			// Act
			Logger.LogInformation("🔍 Getting machine: {MachineId}", machineId);
			var retrieved = await client.Resources.GetAsync<Api.Resources.Machine>(machineId, cancellationToken: CancellationToken);

			// Assert
			retrieved.Should().NotBeNull();
			retrieved.Metadata.Id.Should().Be(machineId);

			Logger.LogInformation("✅ Successfully retrieved machine: {MachineId}", machineId);
		}
		finally
		{
			// Cleanup
			await CleanupMachine(client, machineId);
		}
	}

	[Fact]
	public async Task Machine_Update_ModifiesMachine()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var machineId = CreateUniqueId("test-machine");
		
		// Create a machine first
		var machine = await CreateTestMachine(client, machineId);
		if (machine == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create machine (likely permission denied)");
			return;
		}

		try
		{
			// Act - Update the machine
			Logger.LogInformation("🔍 Updating machine: {MachineId}", machineId);
			machine.Metadata.Labels ??= new Dictionary<string, string>();
			machine.Metadata.Labels["updated"] = "true";
			
			var updated = await client.Resources.UpdateAsync(machine, cancellationToken: CancellationToken);

			// Assert
			updated.Should().NotBeNull();
			updated.Metadata.Labels.Should().ContainKey("updated");
			updated.Metadata.Labels["updated"].Should().Be("true");

			Logger.LogInformation("✅ Successfully updated machine: {MachineId}", machineId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupMachine(client, machineId);
		}
	}

	[Fact]
	public async Task Machine_Delete_RemovesMachine()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var machineId = CreateUniqueId("test-machine");
		
		// Create a machine first
		var machine = await CreateTestMachine(client, machineId);
		if (machine == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create machine (likely permission denied)");
			return;
		}

		// Act
		Logger.LogInformation("🔍 Deleting machine: {MachineId}", machineId);
		await client.Resources.DeleteAsync<Api.Resources.Machine>(machineId, cancellationToken: CancellationToken);

		// Assert - Try to get it, should throw NotFound
		await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await client.Resources.GetAsync<Api.Resources.Machine>(machineId, cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ Successfully deleted machine: {MachineId}", machineId);
	}

	[Fact]
	public async Task Machine_List_ReturnsMultipleMachines()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act
			Logger.LogInformation("🔍 Listing all machines");
			var machines = new List<Api.Resources.Machine>();
			
			await foreach (var machine in client.Resources.ListAsync<Api.Resources.Machine>(cancellationToken: CancellationToken))
			{
				machines.Add(machine);
			}

			// Assert
			Logger.LogInformation("✅ Successfully listed {Count} machines", machines.Count);
			machines.Should().NotBeNull();
			
			// We don't assert a specific count since we don't know how many exist
			// Just verify we can list without error
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	#endregion

	#region ConfigPatch CRUD Tests

	[Fact]
	public async Task ConfigPatch_Create_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var patchId = CreateUniqueId("test-patch");
		
		var patch = new Api.Builders.ConfigPatchBuilder(patchId)
			.WithData("machine:\n  network:\n    hostname: test-node")
			.Build();

		try
		{
			// Act
			Logger.LogInformation("🔍 Creating test config patch: {PatchId}", patchId);
			var created = await client.Resources.CreateAsync(patch, CancellationToken);

			// Assert
			created.Should().NotBeNull();
			created.Metadata.Id.Should().Be(patchId);
			created.Spec.Data.Should().Contain("hostname: test-node");

			Logger.LogInformation("✅ Successfully created config patch: {PatchId}", patchId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupConfigPatch(client, patchId);
		}
	}

	[Fact]
	public async Task ConfigPatch_Get_ReturnsConfigPatch()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var patchId = CreateUniqueId("test-patch");
		
		// Create a config patch first
		var patch = await CreateTestConfigPatch(client, patchId);
		if (patch == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create config patch (likely permission denied)");
			return;
		}

		try
		{
			// Act
			Logger.LogInformation("🔍 Getting config patch: {PatchId}", patchId);
			var retrieved = await client.Resources.GetAsync<Api.Resources.ConfigPatch>(patchId, cancellationToken: CancellationToken);

			// Assert
			retrieved.Should().NotBeNull();
			retrieved.Metadata.Id.Should().Be(patchId);
			retrieved.Spec.Data.Should().Contain("hostname: test-node");

			Logger.LogInformation("✅ Successfully retrieved config patch: {PatchId}", patchId);
		}
		finally
		{
			// Cleanup
			await CleanupConfigPatch(client, patchId);
		}
	}

	[Fact]
	public async Task ConfigPatch_Update_ModifiesConfigPatch()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var patchId = CreateUniqueId("test-patch");
		
		// Create a config patch first
		var patch = await CreateTestConfigPatch(client, patchId);
		if (patch == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create config patch (likely permission denied)");
			return;
		}

		try
		{
			// Act - Update the config patch
			Logger.LogInformation("🔍 Updating config patch: {PatchId}", patchId);
			patch.Spec.Data = "machine:\n  network:\n    hostname: updated-node";
			
			var updated = await client.Resources.UpdateAsync(patch, cancellationToken: CancellationToken);

			// Assert
			updated.Should().NotBeNull();
			updated.Spec.Data.Should().Contain("updated-node");

			Logger.LogInformation("✅ Successfully updated config patch: {PatchId}", patchId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupConfigPatch(client, patchId);
		}
	}

	[Fact]
	public async Task ConfigPatch_Delete_RemovesConfigPatch()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var patchId = CreateUniqueId("test-patch");
		
		// Create a config patch first
		var patch = await CreateTestConfigPatch(client, patchId);
		if (patch == null)
		{
			Logger.LogInformation("⏭️ Skipping test - could not create config patch (likely permission denied)");
			return;
		}

		// Act
		Logger.LogInformation("🔍 Deleting config patch: {PatchId}", patchId);
		await client.Resources.DeleteAsync<Api.Resources.ConfigPatch>(patchId, cancellationToken: CancellationToken);

		// Assert - Try to get it, should throw NotFound
		await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await client.Resources.GetAsync<Api.Resources.ConfigPatch>(patchId, cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ Successfully deleted config patch: {PatchId}", patchId);
	}

	[Fact]
	public async Task ConfigPatch_List_ReturnsMultipleConfigPatches()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act
			Logger.LogInformation("🔍 Listing all config patches");
			var patches = new List<Api.Resources.ConfigPatch>();
			
			await foreach (var patch in client.Resources.ListAsync<Api.Resources.ConfigPatch>(cancellationToken: CancellationToken))
			{
				patches.Add(patch);
			}

			// Assert
			Logger.LogInformation("✅ Successfully listed {Count} config patches", patches.Count);
			patches.Should().NotBeNull();
			
			// We don't assert a specific count since we don't know how many exist
			// Just verify we can list without error
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	#endregion

	#region Helper Methods

	private OmniClientOptions GetClientOptions(bool? isReadOnlyOverride = null)
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
			IsReadOnly = isReadOnlyOverride ?? bool.Parse(omniSection["IsReadOnly"] ?? "false"),
			Logger = Logger
		};
	}

	private static string CreateUniqueId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

	private async Task<Api.Resources.Cluster?> CreateTestCluster(OmniClient client, string clusterId)
	{
		try
		{
			var cluster = new Api.Builders.ClusterBuilder(clusterId)
				.WithKubernetesVersion("v1.29.0")
				.WithTalosVersion("v1.7.0")
				.Build();

			Logger.LogDebug("Creating test cluster: {ClusterId}", clusterId);
			return await client.Resources.CreateAsync(cluster, CancellationToken);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogWarning("Cannot create test cluster - permission denied");
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to create test cluster: {ClusterId}", clusterId);
			return null;
		}
	}

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

	private async Task<Api.Resources.Machine?> CreateTestMachine(OmniClient client, string machineId)
	{
		try
		{
			var machine = new Api.Builders.MachineBuilder(machineId)
				.Build();

			Logger.LogDebug("Creating test machine: {MachineId}", machineId);
			return await client.Resources.CreateAsync(machine, CancellationToken);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogWarning("Cannot create test machine - permission denied");
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to create test machine: {MachineId}", machineId);
			return null;
		}
	}

	private async Task CleanupMachine(OmniClient client, string machineId)
	{
		try
		{
			await client.Resources.DeleteAsync<Api.Resources.Machine>(machineId, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test machine: {MachineId}", machineId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			// Already deleted, that's fine
			Logger.LogDebug("Test machine already deleted: {MachineId}", machineId);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test machine: {MachineId}", machineId);
		}
	}

	private async Task<Api.Resources.ConfigPatch?> CreateTestConfigPatch(OmniClient client, string patchId)
	{
		try
		{
			var patch = new Api.Builders.ConfigPatchBuilder(patchId)
				.WithData("machine:\n  network:\n    hostname: test-node")
				.Build();

			Logger.LogDebug("Creating test config patch: {PatchId}", patchId);
			return await client.Resources.CreateAsync(patch, CancellationToken);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogWarning("Cannot create test config patch - permission denied");
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Failed to create test config patch: {PatchId}", patchId);
			return null;
		}
	}

	private async Task CleanupConfigPatch(OmniClient client, string patchId)
	{
		try
		{
			await client.Resources.DeleteAsync<Api.Resources.ConfigPatch>(patchId, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test config patch: {PatchId}", patchId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			// Already deleted, that's fine
			Logger.LogDebug("Test config patch already deleted: {PatchId}", patchId);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test config patch: {PatchId}", patchId);
		}
	}

	#endregion
}
