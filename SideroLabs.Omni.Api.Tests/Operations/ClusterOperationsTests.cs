using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Builders;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Operations;

/// <summary>
/// Integration tests for Cluster Operations API
/// Tests high-level cluster lifecycle and machine management operations
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ClusterOperations")]
public class ClusterOperationsTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that lists all clusters and verifies each has required metadata fields.
	/// </summary>
	[Fact]
	public async Task ListAsync_ReturnsAllClusters()
	{
		await RunSharedAsync(async client =>
		{
			var clusters = new List<Api.Resources.Cluster>();
			await foreach (var cluster in client.Clusters.ListAsync(cancellationToken: CancellationToken))
			{
				clusters.Add(cluster);
			}

			Logger.LogInformation("📊 Found {Count} clusters", clusters.Count);

			foreach (var cluster in clusters)
			{
				cluster.Metadata.Should().NotBeNull();
				cluster.Metadata.Id.Should().NotBeNullOrEmpty();
				cluster.Metadata.Namespace.Should().NotBeNullOrEmpty();
			}
		});
	}

	/// <summary>
	/// Integration test that lists clusters filtered to the 'default' namespace.
	/// </summary>
	[Fact]
	public async Task ListAsync_WithCustomNamespace_ReturnsFilteredClusters()
	{
		await RunSharedAsync(async client =>
		{
			await foreach (var cluster in client.Clusters.ListAsync(@namespace: "default", cancellationToken: CancellationToken))
			{
				cluster.Metadata.Namespace.Should().Be("default");
			}
		});
	}

	/// <summary>
	/// Integration test that retrieves an existing cluster by ID.
	/// </summary>
	[Fact]
	public async Task GetAsync_ExistingCluster_ReturnsCluster()
	{
		await RunSharedAsync(async client =>
		{
			var existing = await FirstOrDefaultResourceAsync<Api.Resources.Cluster>(client);
			if (existing == null)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping GetAsync test");
				return;
			}

			var cluster = await client.Clusters.GetAsync(existing.Metadata.Id, cancellationToken: CancellationToken);

			cluster.Metadata.Id.Should().Be(existing.Metadata.Id);
			cluster.Metadata.Namespace.Should().Be(existing.Metadata.Namespace);
		}, toleratesNotFound: true);
	}

	/// <summary>
	/// Verifies that attempting to get a non-existent cluster throws a <see cref="StatusCode.NotFound"/> gRPC error.
	/// </summary>
	[Fact]
	public async Task GetAsync_NonExistentCluster_ThrowsNotFound()
	{
		await RunSharedAsync(async client =>
		{
			var id = CreateUniqueId("nonexistent");
			var exception = await Assert.ThrowsAsync<RpcException>(() => client.Clusters.GetAsync(id, cancellationToken: CancellationToken));

			exception.StatusCode.Should().Be(StatusCode.NotFound);
		});
	}

	/// <summary>
	/// Verifies that getting the status for an existing cluster returns a non-null status object.
	/// </summary>
	[Fact]
	public async Task GetStatus_ExistingCluster_ReturnsStatus()
	{
		await RunSharedAsync(async client =>
		{
			var existing = await FirstOrDefaultResourceAsync<Api.Resources.Cluster>(client);
			if (existing == null)
			{
				Logger.LogInformation("⏭️ No existing clusters found - skipping status test");
				return;
			}

			var status = await client.Clusters.GetStatusAsync(existing.Metadata.Id, cancellationToken: CancellationToken);

			status.Should().NotBeNull();
		}, toleratesNotFound: true);
	}

	/// <summary>
	/// Verifies that getting the status for a non-existent cluster throws a <see cref="StatusCode.NotFound"/> gRPC error.
	/// </summary>
	[Fact]
	public async Task GetStatus_NonExistentCluster_ThrowsNotFound()
	{
		await RunSharedAsync(async client =>
		{
			var id = CreateUniqueId("nonexistent");
			var exception = await Assert.ThrowsAsync<RpcException>(async () => await client.Clusters.GetStatusAsync(id, cancellationToken: CancellationToken));

			exception.StatusCode.Should().Be(StatusCode.NotFound);
		});
	}

	/// <summary>
	/// Verifies that a cluster can be created successfully via the cluster Operations API.
	/// </summary>
	[Fact]
	public async Task CreateCluster_ViaOperations_Success()
	{
		var clusterId = CreateUniqueId("ops-cluster");

		try
		{
			await RunSharedAsync(async client =>
			{
				var created = await client.Clusters.CreateAsync(BuildCluster(clusterId), CancellationToken);

				created.Metadata.Id.Should().Be(clusterId);
			});
		}
		finally
		{
			await CleanupIfConfiguredAsync(clusterId);
		}
	}

	/// <summary>
	/// Verifies that a cluster can be deleted successfully via the cluster Operations API.
	/// </summary>
	[Fact]
	public async Task DeleteCluster_ViaOperations_Success()
	{
		var clusterId = CreateUniqueId("ops-delete");

		try
		{
			await RunSharedAsync(async client =>
			{
				await client.Clusters.CreateAsync(BuildCluster(clusterId), CancellationToken);
				await client.Clusters.DeleteAsync(clusterId, force: false, cancellationToken: CancellationToken);

				var exception = await Assert.ThrowsAsync<RpcException>(async () =>
					await client.Resources.GetAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken));

				exception.StatusCode.Should().Be(StatusCode.NotFound);
			});
		}
		finally
		{
			await CleanupIfConfiguredAsync(clusterId);
		}
	}

	/// <summary>
	/// Verifies the complete lock and unlock cycle for a machine, ensuring both operations succeed.
	/// </summary>
	[Fact]
	public async Task LockUnlockMachine_FullCycle_Success()
	{
		await RunSharedAsync(async client =>
		{
			var machine = await FirstOrDefaultResourceAsync<Api.Resources.Machine>(client);
			var cluster = await FirstOrDefaultResourceAsync<Api.Resources.Cluster>(client);
			if (machine == null || cluster == null)
			{
				Logger.LogInformation("⏭️ No existing machine or cluster found - skipping lock test");
				return;
			}

			var machineId = machine.Metadata.Id;
			var clusterId = cluster.Metadata.Id;

			await client.Clusters.LockMachineAsync(machineId, clusterId, CancellationToken);
			(await GetMachineAsync(client, machineId)).Status?.Locked.Should().BeTrue();

			await client.Clusters.UnlockMachineAsync(machineId, clusterId, CancellationToken);
			(await GetMachineAsync(client, machineId)).Status?.Locked.Should().BeFalse();
		}, toleratesNotFound: true);
	}

	private static Api.Resources.Cluster BuildCluster(string clusterId) => new ClusterBuilder(clusterId)
		.WithKubernetesVersion("v1.29.0")
		.WithTalosVersion("v1.7.0")
		.Build();

	private Task<Api.Resources.Machine> GetMachineAsync(Interfaces.IOmniClient client, string machineId) =>
		client.Resources.GetAsync<Api.Resources.Machine>(machineId, cancellationToken: CancellationToken);

	private async Task CleanupIfConfiguredAsync(string clusterId)
	{
		if (ShouldRunIntegrationTests())
		{
			await CleanupAsync<Api.Resources.Cluster>(OmniClient, clusterId);
		}
	}
}