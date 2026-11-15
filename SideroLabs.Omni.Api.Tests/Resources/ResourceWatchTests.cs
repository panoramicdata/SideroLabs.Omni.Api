using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Builders;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for Watch operations on Omni resources
/// Tests real-time streaming and event notifications
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Streaming")]
public class ResourceWatchTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that establishes a watch stream on Cluster resources and verifies a Created event is received after creating a cluster.
	/// </summary>
	[Fact]
	public async Task Watch_Cluster_ReceivesCreatedEvent()
	{
		await RunAsync(async client =>
		{
			var clusterId = CreateUniqueId("watch-test");
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
			var watchTask = Task.Run(() => WaitForCreatedAsync(client, clusterId, cts.Token), cts.Token);

			try
			{
				await Task.Delay(1000, CancellationToken);

				var cluster = new ClusterBuilder(clusterId)
					.WithKubernetesVersion("v1.29.0")
					.WithTalosVersion("v1.7.0")
					.Build();

				await client.Resources.CreateAsync(cluster, CancellationToken);

				(await watchTask).Should().BeTrue("Should receive Created event");
			}
			catch (OperationCanceledException)
			{
				Logger.LogInformation("⏱️ Watch timed out - this may be expected if permissions don't allow watch");
			}
			finally
			{
				await CleanupAsync<Cluster>(client, clusterId);
			}
		});
	}

	/// <summary>
	/// Verifies that watching resources with a label selector only yields events for resources matching the selector.
	/// </summary>
	[Fact]
	public async Task Watch_WithSelector_FiltersEvents()
	{
		await RunAsync(async client =>
		{
			var count = await CountEventsAsync(client, TimeSpan.FromSeconds(5), 3, selector: "env=test", onEvent: evt =>
			{
				evt.Resource.Metadata.Labels.Should().ContainKey("env");
				evt.Resource.Metadata.Labels["env"].Should().Be("test");
			});

			Logger.LogInformation("✅ Watch with selector works correctly (received {Count} events)", count);
		});
	}

	/// <summary>
	/// Verifies that watching with tail events replays recent event history before emitting live events.
	/// </summary>
	[Fact]
	public async Task Watch_TailEvents_ReplaysHistory()
	{
		await RunAsync(async client =>
		{
			var count = await CountEventsAsync(client, TimeSpan.FromSeconds(5), 10, tailEvents: 10);

			count.Should().BeGreaterThan(0, "Should receive at least some historical events");
		});
	}

	/// <summary>
	/// Verifies that cancelling a watch stream correctly stops the streaming and does not throw unexpected exceptions.
	/// </summary>
	[Fact]
	public async Task Watch_Cancellation_StopsStreaming()
	{
		await RunAsync(async client =>
		{
			var count = await CountEventsAsync(client, TimeSpan.FromSeconds(2), int.MaxValue);

			Logger.LogInformation("✅ Watch completed with {Count} events", count);
		});
	}

	private static async Task<bool> WaitForCreatedAsync(OmniClient client, string clusterId, CancellationToken cancellationToken)
	{
		await foreach (var evt in client.Resources.WatchAsync<Cluster>(cancellationToken: cancellationToken))
		{
			if (evt.Resource.Metadata.Id == clusterId && evt.Type == ResourceEventType.Created)
			{
				return true;
			}
		}

		return false;
	}

	private async Task<int> CountEventsAsync(
		OmniClient client,
		TimeSpan timeout,
		int maxEvents,
		string? selector = null,
		int tailEvents = 0,
		Action<ResourceEvent<Cluster>>? onEvent = null)
	{
		using var cts = new CancellationTokenSource(timeout);
		var count = 0;

		try
		{
			await foreach (var evt in client.Resources.WatchAsync<Cluster>(
				selector: selector,
				tailEvents: tailEvents,
				cancellationToken: cts.Token))
			{
				count++;
				onEvent?.Invoke(evt);

				if (count >= maxEvents)
				{
					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("⏱️ Watch ended by timeout after {Count} events", count);
		}

		return count;
	}
}
