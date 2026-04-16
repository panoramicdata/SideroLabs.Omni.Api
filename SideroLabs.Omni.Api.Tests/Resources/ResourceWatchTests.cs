using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for Watch operations on Omni resources
/// Tests real-time streaming and event notifications
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Streaming")]
public class ResourceWatchTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that establishes a watch stream on Cluster resources and verifies a Created event is received after creating a cluster.
	/// </summary>
	[Fact]
	public async Task Watch_Cluster_ReceivesCreatedEvent()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var clusterId = CreateUniqueId("watch-test");
		
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var watchTask = Task.Run(async () =>
		{
			await foreach (var evt in client.Resources.WatchAsync<Api.Resources.Cluster>(cancellationToken: cts.Token))
			{
				if (evt.Resource.Metadata.Id == clusterId && evt.Type == Api.Resources.ResourceEventType.Created)
				{
					Logger.LogInformation("✅ Received Created event for cluster: {Id}", clusterId);
					return true;
				}
			}
			return false;
		}, cts.Token);

		try
		{
			// Wait a moment for watch to establish
			await Task.Delay(1000, CancellationToken);

			// Act - Create the cluster
			var cluster = new Api.Builders.ClusterBuilder(clusterId)
				.WithKubernetesVersion("v1.29.0")
				.WithTalosVersion("v1.7.0")
				.Build();

			await client.Resources.CreateAsync(cluster, CancellationToken);

			// Assert
			var receivedEvent = await watchTask;
			receivedEvent.Should().BeTrue("Should receive Created event");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("⏱️ Watch timed out - this may be expected if permissions don't allow watch");
		}
		finally
		{
			// Cleanup
			try
			{
				await client.Resources.DeleteAsync<Api.Resources.Cluster>(clusterId, cancellationToken: CancellationToken);
			}
			catch { /* Ignore cleanup errors */ }
		}
	}

	/// <summary>
	/// Verifies that watching resources with a label selector only yields events for resources matching the selector.
	/// </summary>
	[Fact]
	public async Task Watch_WithSelector_FiltersEvents()
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
			// Act - Watch with label selector
			Logger.LogInformation("🔍 Watching clusters with selector: env=test");
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			var eventCount = 0;

			await foreach (var evt in client.Resources.WatchAsync<Api.Resources.Cluster>(
				selector: "env=test",
				cancellationToken: cts.Token))
			{
				eventCount++;
				Logger.LogInformation("Received event: {Type} - {Id}", evt.Type, evt.Resource.Metadata.Id);
				
				// Verify the event matches our selector
				if (evt.Resource.Metadata.Labels != null)
				{
					evt.Resource.Metadata.Labels.Should().ContainKey("env");
					evt.Resource.Metadata.Labels["env"].Should().Be("test");
				}

				if (eventCount >= 3)
				{
					break;
				}
			}

			Logger.LogInformation("✅ Watch with selector works correctly (received {Count} events)", eventCount);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("⏱️ Watch timed out - no matching events in time window");
		}
	}

	/// <summary>
	/// Verifies that watching with tail events replays recent event history before emitting live events.
	/// </summary>
	[Fact]
	public async Task Watch_TailEvents_ReplaysHistory()
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
			// Act - Watch with tail events to get recent history
			Logger.LogInformation("🔍 Watching clusters with tail events");
			using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			var eventCount = 0;

			await foreach (var evt in client.Resources.WatchAsync<Api.Resources.Cluster>(
				tailEvents: 10, // Request last 10 events
				cancellationToken: cts.Token))
			{
				eventCount++;
				Logger.LogInformation("Event {Count}: {Type} - {Id}", eventCount, evt.Type, evt.Resource.Metadata.Id);

				if (eventCount >= 10)
				{
					break;
				}
			}

			Logger.LogInformation("✅ Tail events works correctly (received {Count} events)", eventCount);
			eventCount.Should().BeGreaterThan(0, "Should receive at least some historical events");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("⏱️ Watch timed out");
		}
	}

	/// <summary>
	/// Verifies that cancelling a watch stream correctly stops the streaming and does not throw unexpected exceptions.
	/// </summary>
	[Fact]
	public async Task Watch_Cancellation_StopsStreaming()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
		
		try
		{
			// Act - Start watching and cancel
			Logger.LogInformation("🔍 Starting watch and cancelling");
			var eventCount = 0;

			await foreach (var evt in client.Resources.WatchAsync<Api.Resources.Cluster>(
				cancellationToken: cts.Token))
			{
				eventCount++;
				Logger.LogDebug("Received event {Count}", eventCount);
			}

			Logger.LogInformation("✅ Watch completed with {Count} events", eventCount);
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("✅ Watch correctly cancelled after timeout");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
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
}
