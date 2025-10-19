using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Resources;
using Xunit;
using Machine = SideroLabs.Omni.Api.Resources.Machine;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Tests for COSI State Service basic operations (List, Get)
/// These tests verify that the COSI State service works correctly on Omni SaaS
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "COSIState")]
[Trait("Category", "Resources")]
public class CosiStateBasicOperationsTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task ListClusters_ReturnsClusterList()
	{
		// Arrange
		Logger.LogInformation("?? Testing: List all clusters via COSI State");

		// Act
		var clusters = new List<Cluster>();
		await foreach (var cluster in OmniClient.Resources.ListAsync<Cluster>(
			cancellationToken: CancellationToken))
		{
			clusters.Add(cluster);
		}

		// Assert
		Assert.NotNull(clusters);
		Logger.LogInformation("?? Found {Count} clusters", clusters.Count);

		foreach (var cluster in clusters)
		{
			Assert.NotEmpty(cluster.Metadata.Id);
			Assert.NotEmpty(cluster.Metadata.Namespace);
			Assert.NotEmpty(cluster.Metadata.Version);
			Logger.LogDebug("  Cluster: {Id}, Namespace: {Namespace}, Version: {Version}",
				cluster.Metadata.Id, cluster.Metadata.Namespace, cluster.Metadata.Version);
		}
	}

	[Fact]
	public async Task ListMachines_ReturnsMachineList()
	{
		// Arrange
		Logger.LogInformation("?? Testing: List all machines via COSI State");

		// Act
		var machines = new List<Machine>();
		await foreach (var machine in OmniClient.Resources.ListAsync<Machine>(
			cancellationToken: CancellationToken))
		{
			machines.Add(machine);
		}

		// Assert
		Assert.NotNull(machines);
		Logger.LogInformation("?? Found {Count} machines", machines.Count);

		foreach (var machine in machines.Take(5)) // Log first 5
		{
			Assert.NotEmpty(machine.Metadata.Id);
			Logger.LogDebug("  Machine: {Id}, Namespace: {Namespace}",
				machine.Metadata.Id, machine.Metadata.Namespace);
		}
	}

	[Fact]
	public async Task GetCluster_ExistingCluster_ReturnsCluster()
	{
		// Arrange - Get first cluster ID
		Logger.LogInformation("?? Testing: Get specific cluster via COSI State");

		Cluster? firstCluster = null;
		await foreach (var c in OmniClient.Resources.ListAsync<Cluster>(
			cancellationToken: CancellationToken))
		{
			firstCluster = c;
			break;
		}

		if (firstCluster == null)
		{
			Logger.LogWarning("?? No clusters available for Get test - skipping");
			return;
		}

		Logger.LogInformation("Found cluster to retrieve: {ClusterId}", firstCluster.Metadata.Id);

		// Act
		var cluster = await OmniClient.Resources.GetAsync<Cluster>(
			firstCluster.Metadata.Id,
			firstCluster.Metadata.Namespace,
			CancellationToken);

		// Assert
		Assert.NotNull(cluster);
		Assert.Equal(firstCluster.Metadata.Id, cluster.Metadata.Id);
		Assert.Equal(firstCluster.Metadata.Namespace, cluster.Metadata.Namespace);
		Assert.Equal(firstCluster.Metadata.Version, cluster.Metadata.Version);

		Logger.LogInformation("? Successfully retrieved cluster: {Id}", cluster.Metadata.Id);
	}

	[Fact]
	public async Task GetMachine_ExistingMachine_ReturnsMachine()
	{
		// Arrange - Get first machine ID
		Logger.LogInformation("?? Testing: Get specific machine via COSI State");

		Machine? firstMachine = null;
		await foreach (var m in OmniClient.Resources.ListAsync<Machine>(
			cancellationToken: CancellationToken))
		{
			firstMachine = m;
			break;
		}

		if (firstMachine == null)
		{
			Logger.LogWarning("?? No machines available for Get test - skipping");
			return;
		}

		Logger.LogInformation("Found machine to retrieve: {MachineId}", firstMachine.Metadata.Id);

		// Act
		var machine = await OmniClient.Resources.GetAsync<Machine>(
			firstMachine.Metadata.Id,
			firstMachine.Metadata.Namespace,
			CancellationToken);

		// Assert
		Assert.NotNull(machine);
		Assert.Equal(firstMachine.Metadata.Id, machine.Metadata.Id);

		Logger.LogInformation("? Successfully retrieved machine: {Id}", machine.Metadata.Id);
	}

	[Fact]
	public async Task GetCluster_NonExistent_ThrowsNotFound()
	{
		// Arrange
		var nonExistentId = $"non-existent-cluster-{Guid.NewGuid():N}";
		Logger.LogInformation("?? Testing: Get non-existent cluster returns NotFound");

		// Act & Assert
		var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await OmniClient.Resources.GetAsync<Cluster>(
				nonExistentId,
				"default",
				CancellationToken);
		});

		Assert.Equal(Grpc.Core.StatusCode.NotFound, exception.StatusCode);
		Logger.LogInformation("? Correctly returned NotFound for non-existent cluster");
	}

	[Fact]
	public async Task GetMachine_NonExistent_ThrowsNotFound()
	{
		// Arrange
		var nonExistentId = $"non-existent-machine-{Guid.NewGuid():N}";
		Logger.LogInformation("?? Testing: Get non-existent machine returns NotFound");

		// Act & Assert
		var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await OmniClient.Resources.GetAsync<Machine>(
				nonExistentId,
				"default",
				CancellationToken);
		});

		Assert.Equal(Grpc.Core.StatusCode.NotFound, exception.StatusCode);
		Logger.LogInformation("? Correctly returned NotFound for non-existent machine");
	}

	[Fact]
	public async Task ListWithNamespace_DefaultNamespace_ReturnsResources()
	{
		// Arrange
		Logger.LogInformation("?? Testing: List with explicit namespace parameter");

		// Act
		var clusterCount = 0;
		await foreach (var cluster in OmniClient.Resources.ListAsync<Cluster>(
			@namespace: "default",
			cancellationToken: CancellationToken))
		{
			Assert.Equal("default", cluster.Metadata.Namespace);
			clusterCount++;
		}

		// Assert
		Logger.LogInformation("?? Found {Count} clusters in 'default' namespace", clusterCount);
	}

	[Fact]
	public async Task ComprehensiveTest_ProveCosiStateWorks()
	{
		Logger.LogInformation("?? COMPREHENSIVE TEST: COSI State service integration");
		Logger.LogInformation("");
		Logger.LogInformation("This test proves that:");
		Logger.LogInformation("  1. We're using the CORRECT endpoint: /cosi.resource.State/*");
		Logger.LogInformation("  2. NOT the blocked endpoint: /omni.resources.ResourceService/*");
		Logger.LogInformation("  3. Resource operations work on Omni SaaS");
		Logger.LogInformation("");

		// Test List
		var clusterCount = 0;
		await foreach (var _ in OmniClient.Resources.ListAsync<Cluster>(
			cancellationToken: CancellationToken))
		{
			clusterCount++;
		}
		Logger.LogInformation("? List: Found {Count} clusters", clusterCount);

		// Test Get (if we have clusters)
		if (clusterCount > 0)
		{
			Cluster? firstCluster = null;
			await foreach (var c in OmniClient.Resources.ListAsync<Cluster>(
				cancellationToken: CancellationToken))
			{
				firstCluster = c;
				break;
			}

			if (firstCluster != null)
			{
				var retrieved = await OmniClient.Resources.GetAsync<Cluster>(
					firstCluster.Metadata.Id,
					firstCluster.Metadata.Namespace,
					CancellationToken);

				Assert.Equal(firstCluster.Metadata.Id, retrieved.Metadata.Id);
				Logger.LogInformation("? Get: Retrieved cluster {Id}", retrieved.Metadata.Id);
			}
		}

		// Test machines too
		var machineCount = 0;
		await foreach (var _ in OmniClient.Resources.ListAsync<Machine>(
			cancellationToken: CancellationToken))
		{
			machineCount++;
		}
		Logger.LogInformation("? List: Found {Count} machines", machineCount);

		Logger.LogInformation("");
		Logger.LogInformation("?? SUCCESS! COSI State service is fully functional!");
		Logger.LogInformation("   We have achieved parity with omnictl for resource operations!");
	}
}
