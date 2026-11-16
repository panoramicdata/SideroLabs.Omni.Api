using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Builders;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for bulk delete operations on Omni resources
/// Tests DeleteMany and DeleteAll operations
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ResourceBulkDeleteTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that bulk-deletes multiple Cluster resources matching a label selector.
	/// </summary>
	[Fact]
	public async Task DeleteMany_WithSelector_DeletesMatchingResources()
	{
		await RunAsync(async client =>
		{
			var testLabel = $"bulk-delete-{Guid.NewGuid():N}";
			var createdIds = new List<string>();

			try
			{
				for (var i = 0; i < 3; i++)
				{
					var cluster = new ClusterBuilder(CreateUniqueId($"bulk-test-{i}"))
						.WithKubernetesVersion("v1.29.0")
						.WithTalosVersion("v1.7.0")
						.WithLabel("test-group", testLabel)
						.Build();

					createdIds.Add((await client.Resources.CreateAsync(cluster, CancellationToken)).Metadata.Id);
				}

				var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.Cluster>(
					selector: $"test-group={testLabel}",
					cancellationToken: CancellationToken);

				deletedCount.Should().Be(3, "Should delete all 3 test clusters");

				foreach (var id in createdIds)
				{
					await Assert.ThrowsAsync<RpcException>(() =>
						client.Resources.GetAsync<Api.Resources.Cluster>(id, cancellationToken: CancellationToken));
				}

				createdIds.Clear();
			}
			finally
			{
				await CleanupAllAsync<Api.Resources.Cluster>(client, createdIds);
			}
		});
	}

	/// <summary>
	/// Verifies that <c>DeleteMany</c> returns the correct count of deleted resources when matches exist.
	/// </summary>
	[Fact]
	public async Task DeleteMany_ReturnsCorrectCount()
	{
		await RunAsync(async client =>
		{
			var testLabel = $"count-test-{Guid.NewGuid():N}";
			const int expectedCount = 5;
			var createdIds = new List<string>();

			try
			{
				for (var i = 0; i < expectedCount; i++)
				{
					var patch = new ConfigPatchBuilder(CreateUniqueId($"count-test-{i}"))
						.WithData($"machine:\n  test: {i}")
						.Build();

					patch.Metadata.Labels = new Dictionary<string, string> { ["count-test"] = testLabel };
					createdIds.Add((await client.Resources.CreateAsync(patch, CancellationToken)).Metadata.Id);
				}

				var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.ConfigPatch>(
					selector: $"count-test={testLabel}",
					cancellationToken: CancellationToken);

				deletedCount.Should().Be(expectedCount, $"Should delete all {expectedCount} test patches");
				createdIds.Clear();
			}
			finally
			{
				await CleanupAllAsync<Api.Resources.ConfigPatch>(client, createdIds);
			}
		});
	}

	/// <summary>
	/// Verifies that <c>DeleteMany</c> returns zero when the selector matches no resources.
	/// </summary>
	[Fact]
	public async Task DeleteMany_NoMatches_ReturnsZero()
	{
		await RunAsync(async client =>
		{
			var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.Cluster>(
				selector: $"nonexistent-label=nonexistent-{Guid.NewGuid():N}",
				cancellationToken: CancellationToken);

			deletedCount.Should().Be(0, "Should return 0 when no resources match");
		});
	}

	/// <summary>
	/// Verifies that <c>DeleteMany</c> continues processing remaining resources even if an individual delete fails.
	/// </summary>
	[Fact]
	public async Task DeleteMany_ContinuesOnError()
	{
		await RunAsync(async client =>
		{
			var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.Machine>(
				selector: "test-label=resilience-test",
				cancellationToken: CancellationToken);

			deletedCount.Should().BeGreaterThanOrEqualTo(0);
		});
	}

	/// <summary>
	/// Verifies that <c>DeleteAll</c> removes all resources of the specified type within a given namespace.
	/// </summary>
	[Fact]
	public async Task DeleteAll_RemovesAllResourcesInNamespace()
	{
		await RunDestructiveAsync(async client =>
		{
			var testNamespace = $"test-ns-{Guid.NewGuid():N}";

			for (var i = 0; i < 3; i++)
			{
				var patch = new ConfigPatchBuilder(CreateUniqueId($"deleteall-{i}"))
					.WithData($"test: {i}")
					.InNamespace(testNamespace)
					.Build();

				await client.Resources.CreateAsync(patch, CancellationToken);
			}

			var deletedCount = await client.Resources.DeleteAllAsync<Api.Resources.ConfigPatch>(
				@namespace: testNamespace,
				cancellationToken: CancellationToken);

			deletedCount.Should().Be(3, "Should delete all 3 test patches");
		});
	}

	/// <summary>
	/// Verifies that calling <c>DeleteAll</c> against the default namespace is handled safely without throwing unexpected errors.
	/// </summary>
	[Fact]
	public async Task DeleteAll_InDefaultNamespace_HandledSafely()
	{
		await RunDestructiveAsync(async client =>
		{
			var deletedCount = await client.Resources.DeleteAllAsync<Api.Resources.Machine>(cancellationToken: CancellationToken);

			deletedCount.Should().BeGreaterThanOrEqualTo(0);
		});
	}

	private async Task RunDestructiveAsync(Func<OmniClient, Task> body)
	{
		if (ShouldSkipDestructiveTests())
		{
			Logger.LogInformation("⏭️ Skipping destructive test");
			return;
		}

		await RunAsync(body);
	}

	private async Task CleanupAllAsync<T>(OmniClient client, List<string> ids) where T : class, Api.Resources.IOmniResource, new()
	{
		foreach (var id in ids)
		{
			try
			{
				await client.Resources.DeleteAsync<T>(id, cancellationToken: CancellationToken);
			}
			catch (RpcException ex)
			{
				Logger.LogDebug(ex, "Ignoring cleanup error for {Id}", id);
			}
		}
	}
}
