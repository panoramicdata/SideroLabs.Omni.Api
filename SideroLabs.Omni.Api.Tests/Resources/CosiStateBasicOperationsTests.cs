using Grpc.Core;
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
	/// <summary>
	/// Integration test that lists all Cluster resources via the COSI State service and verifies each has required metadata.
	/// </summary>
	[Fact]
	public async Task ListClusters_ReturnsClusterList()
	{
		var clusters = await ListAllAsync<Cluster>();

		foreach (var cluster in clusters)
		{
			Assert.NotEmpty(cluster.Metadata.Id);
			Assert.NotEmpty(cluster.Metadata.Namespace);
			Assert.NotEmpty(cluster.Metadata.Version);
		}
	}

	/// <summary>
	/// Integration test that lists all Machine resources via the COSI State service.
	/// </summary>
	[Fact]
	public async Task ListMachines_ReturnsMachineList()
	{
		var machines = await ListAllAsync<Machine>();

		foreach (var machine in machines.Take(5))
		{
			Assert.NotEmpty(machine.Metadata.Id);
		}
	}

	/// <summary>
	/// Integration test that retrieves a specific Cluster by ID from the COSI State service.
	/// </summary>
	[Fact]
	public Task GetCluster_ExistingCluster_ReturnsCluster() => AssertFirstCanBeRetrievedAsync<Cluster>();

	/// <summary>
	/// Verifies that an existing machine can be retrieved by its ID via the COSI State service.
	/// </summary>
	[Fact]
	public Task GetMachine_ExistingMachine_ReturnsMachine() => AssertFirstCanBeRetrievedAsync<Machine>();

	/// <summary>
	/// Verifies that requesting a non-existent cluster throws a <see cref="StatusCode.NotFound"/> gRPC error.
	/// </summary>
	[Fact]
	public Task GetCluster_NonExistent_ThrowsNotFound() => AssertMissingIsNotFoundAsync<Cluster>();

	/// <summary>
	/// Verifies that requesting a non-existent machine throws a <see cref="StatusCode.NotFound"/> gRPC error.
	/// </summary>
	[Fact]
	public Task GetMachine_NonExistent_ThrowsNotFound() => AssertMissingIsNotFoundAsync<Machine>();

	/// <summary>
	/// Verifies that listing resources with an explicit default namespace returns resources correctly namespaced.
	/// </summary>
	[Fact]
	public async Task ListWithNamespace_DefaultNamespace_ReturnsResources()
	{
		await foreach (var cluster in OmniClient.Resources.ListAsync<Cluster>(
			@namespace: "default",
			cancellationToken: CancellationToken))
		{
			Assert.Equal("default", cluster.Metadata.Namespace);
		}
	}

	/// <summary>
	/// Comprehensive integration test that proves the COSI State service endpoint is accessible and functional.
	/// </summary>
	[Fact]
	public async Task ComprehensiveTest_ProveCosiStateWorks()
	{
		var clusters = await ListAllAsync<Cluster>();
		if (clusters.Count > 0)
		{
			var retrieved = await OmniClient.Resources.GetAsync<Cluster>(
				clusters[0].Metadata.Id,
				clusters[0].Metadata.Namespace,
				CancellationToken);

			Assert.Equal(clusters[0].Metadata.Id, retrieved.Metadata.Id);
		}

		var machines = await ListAllAsync<Machine>();
		Logger.LogInformation("✅ COSI State works: {Clusters} clusters, {Machines} machines", clusters.Count, machines.Count);
	}

	private async Task<List<T>> ListAllAsync<T>() where T : class, IOmniResource, new()
	{
		var items = new List<T>();
		await foreach (var item in OmniClient.Resources.ListAsync<T>(cancellationToken: CancellationToken))
		{
			items.Add(item);
		}

		Logger.LogInformation("Found {Count} {Type} resources", items.Count, typeof(T).Name);
		return items;
	}

	private async Task AssertFirstCanBeRetrievedAsync<T>() where T : class, IOmniResource, new()
	{
		var all = await ListAllAsync<T>();
		if (all.Count == 0)
		{
			Logger.LogWarning("No {Type} resources available for Get test - skipping", typeof(T).Name);
			return;
		}

		var first = all[0];
		var retrieved = await OmniClient.Resources.GetAsync<T>(first.Metadata.Id, first.Metadata.Namespace, CancellationToken);

		Assert.Equal(first.Metadata.Id, retrieved.Metadata.Id);
		Assert.Equal(first.Metadata.Namespace, retrieved.Metadata.Namespace);
	}

	private async Task AssertMissingIsNotFoundAsync<T>() where T : class, IOmniResource, new()
	{
		var missingId = $"non-existent-{Guid.NewGuid():N}";

		var exception = await Assert.ThrowsAsync<RpcException>(() =>
			OmniClient.Resources.GetAsync<T>(missingId, "default", CancellationToken));

		Assert.Equal(StatusCode.NotFound, exception.StatusCode);
	}
}
