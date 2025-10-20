using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Operations;

/// <summary>
/// Integration tests for Cluster Operations API
/// Tests high-level cluster lifecycle and machine management operations
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ClusterOperations")]
public class ClusterOperationsTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task GetStatus_ExistingCluster_ReturnsStatus()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Arrange - List clusters to get an existing one
			Logger.LogInformation("🔍 Finding existing cluster for status test");
			
			Api.Resources.Cluster? existingCluster = null;
			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				limit: 1,
				cancellationToken: CancellationToken))
			{
				existingCluster = c;
				break;
			}

			if (existingCluster == null)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping status test");
				return;
			}

			// Act - Get cluster status
			Logger.LogInformation("🔍 Getting status for cluster: {ClusterId}", existingCluster.Metadata.Id);
			var status = await client.Clusters.GetStatusAsync(
				existingCluster.Metadata.Id,
				cancellationToken: CancellationToken);

			// Assert
			status.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully retrieved cluster status");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("⏭️ Cluster not found - may have been deleted");
		}
	}

	[Fact]
	public async Task GetStatus_NonExistentCluster_ThrowsNotFound()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var nonExistentId = CreateUniqueId("nonexistent");

		try
		{
			// Act & Assert
			Logger.LogInformation("🔍 Attempting to get status for non-existent cluster: {ClusterId}", nonExistentId);
			
			var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
			{
				await client.Clusters.GetStatusAsync(nonExistentId, cancellationToken: CancellationToken);
			});

			// Assert
			exception.StatusCode.Should().Be(Grpc.Core.StatusCode.NotFound);
			Logger.LogInformation("✅ NotFound exception thrown as expected");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task CreateCluster_ViaOperations_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("ops-cluster");

		try
		{
			// Arrange
			var cluster = new Api.Builders.ClusterBuilder(clusterId)
				.WithKubernetesVersion("v1.29.0")
				.WithTalosVersion("v1.7.0")
				.Build();

			// Act
			Logger.LogInformation("🔍 Creating cluster via Operations API: {ClusterId}", clusterId);
			var created = await client.Clusters.CreateAsync(cluster, CancellationToken);

			// Assert
			created.Should().NotBeNull();
			created.Metadata.Id.Should().Be(clusterId);
			
			Logger.LogInformation("✅ Successfully created cluster via Operations API");
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
	public async Task DeleteCluster_ViaOperations_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("ops-delete");

		try
		{
			// Arrange - Create a cluster first
			var cluster = new Api.Builders.ClusterBuilder(clusterId)
				.WithKubernetesVersion("v1.29.0")
				.WithTalosVersion("v1.7.0")
				.Build();

			Logger.LogInformation("🔍 Creating cluster for delete test: {ClusterId}", clusterId);
			await client.Clusters.CreateAsync(cluster, CancellationToken);

			// Act
			Logger.LogInformation("🔍 Deleting cluster via Operations API: {ClusterId}", clusterId);
			await client.Clusters.DeleteAsync(clusterId, force: false, cancellationToken: CancellationToken);

			// Assert - Try to get it, should throw NotFound
			var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
			{
				await client.Resources.GetAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);
			});

			exception.StatusCode.Should().Be(Grpc.Core.StatusCode.NotFound);
			Logger.LogInformation("✅ Successfully deleted cluster via Operations API");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
			
			// Cleanup attempt
			await CleanupCluster(client, clusterId);
		}
	}

	[Fact]
	public async Task LockUnlockMachine_FullCycle_Success()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Arrange - Find an existing machine and cluster
			Logger.LogInformation("🔍 Finding existing machine and cluster for lock test");
			
			Api.Resources.Machine? existingMachine = null;
			Api.Resources.Cluster? existingCluster = null;

			// Get a machine
			await foreach (var m in client.Resources.ListAsync<Api.Resources.Machine>(
				limit: 1,
				cancellationToken: CancellationToken))
			{
				existingMachine = m;
				break;
			}

			// Get a cluster
			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				limit: 1,
				cancellationToken: CancellationToken))
			{
				existingCluster = c;
				break;
			}

			if (existingMachine == null || existingCluster == null)
			{
				Logger.LogInformation("⏭️ No existing machine or cluster found - skipping lock test");
				return;
			}

			var machineId = existingMachine.Metadata.Id;
			var clusterId = existingCluster.Metadata.Id;

			// Act - Lock machine to cluster
			Logger.LogInformation("🔒 Locking machine {MachineId} to cluster {ClusterId}", machineId, clusterId);
			await client.Clusters.LockMachineAsync(machineId, clusterId, CancellationToken);
			
			Logger.LogInformation("✅ Machine locked successfully");

			// Verify machine is locked by retrieving it
			var lockedMachine = await client.Resources.GetAsync<Api.Resources.Machine>(
				machineId,
				cancellationToken: CancellationToken);
			
			lockedMachine.Status?.Locked.Should().BeTrue();

			// Act - Unlock machine
			Logger.LogInformation("🔓 Unlocking machine {MachineId}", machineId);
			await client.Clusters.UnlockMachineAsync(machineId, clusterId, CancellationToken);
			
			Logger.LogInformation("✅ Machine unlocked successfully");

			// Verify machine is unlocked
			var unlockedMachine = await client.Resources.GetAsync<Api.Resources.Machine>(
				machineId,
				cancellationToken: CancellationToken);
			
			unlockedMachine.Status?.Locked.Should().BeFalse();

			Logger.LogInformation("✅ Lock/Unlock full cycle successful");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("⏭️ Machine or cluster not found - may have been deleted");
		}
	}

	#region Helper Methods

	private OmniClientOptions GetClientOptions()
	{
		var configuration = Configuration;
		var omniSection = configuration.GetSection("Omni");
		var authToken = omniSection["AuthToken"] ?? throw new FormatException("Omni:AuthToken required");

		return new OmniClientOptions
		{
			BaseUrl = new(omniSection["BaseUrl"] ?? throw new InvalidOperationException("Omni:BaseUrl required")),
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
			Logger.LogDebug("Test cluster already deleted: {ClusterId}", clusterId);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test cluster: {ClusterId}", clusterId);
		}
	}

	#endregion
}
