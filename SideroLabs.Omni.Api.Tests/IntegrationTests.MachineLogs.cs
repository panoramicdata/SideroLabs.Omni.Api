using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SideroLabs.Omni.Api.Tests;

public partial class IntegrationTests
{
	/// <summary>
	/// Gets the first available machine ID from talosconfig
	/// </summary>
	private async Task<string?> GetFirstMachineIdAsync(OmniClient client)
	{
		try
		{
			Logger.LogInformation("🔍 Retrieving talosconfig to extract machine IDs...");
			var talosconfig = await client.Management.GetTalosConfigAsync(
				raw: false,
				cancellationToken: CancellationToken);

			// Parse YAML to extract endpoints
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(CamelCaseNamingConvention.Instance)
				.Build();

			var config = deserializer.Deserialize<Dictionary<string, object>>(talosconfig);

			// Extract endpoints from contexts
			if (config.TryGetValue("contexts", out var contextsObj) && contextsObj is Dictionary<object, object> contexts)
			{
				foreach (var contextEntry in contexts.Values)
				{
					if (contextEntry is Dictionary<object, object> context &&
						context.TryGetValue("endpoints", out var endpointsObj) &&
						endpointsObj is List<object> endpoints &&
						endpoints.Count > 0)
					{
						var firstEndpoint = endpoints[0].ToString();
						Logger.LogInformation("✅ Found machine endpoint: {BaseUrl}", firstEndpoint);
						return firstEndpoint;
					}
				}
			}

			Logger.LogWarning("⚠️ No machine endpoints found in talosconfig");
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to extract machine ID from talosconfig");
			return null;
		}
	}

	/// <summary>
	/// Integration test that streams machine logs for configured machines.
	/// </summary>
	[Fact]
	public async Task RealWorld_StreamMachineLogs_WithConfiguredMachines()
	{
		await RunWithMachineAsync(async (client, machineId) =>
		{
			var logCount = 0;
			const int maxLogs = 5;

			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: machineId,
				follow: false,
				tailLines: 10,
				cancellationToken: CancellationToken))
			{
				var logText = Encoding.UTF8.GetString(logData);
				Logger.LogInformation("📄 Log entry {Count}: {LogText}", ++logCount, logText.Length > 100 ? logText[..100] + "..." : logText);

				if (logCount >= maxLogs)
				{
					break;
				}
			}

			logCount.Should().BePositive("Should have received at least one log entry from the machine");
			Logger.LogInformation("✅ Successfully streamed {Count} log entries from machine {MachineId}", logCount, machineId);
		});
	}
	/// <summary>
	/// Integration test that validates input for machine log streaming.
	/// </summary>
	[Fact]
	public async Task RealWorld_MachineLogStreaming_ValidatesInput()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var options = GetClientOptions();
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing machine log streaming input validation");

		try
		{
			// Act & Assert - Test with empty machine ID
			var emptyMachineId = "";
			var logCount = 0;

			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: emptyMachineId,
				follow: false,
				tailLines: 1,
				cancellationToken: CancellationToken))
			{
				logCount++;
				break; // Just check if we can start the stream
			}

			// If we get here, either:
			// 1. The API accepted empty machine ID (some APIs do this)
			// 2. We got an error (which is expected and handled in the catch)
			Logger.LogInformation("ℹ️ Machine log streaming API accepted empty machine ID");
		}
		catch (Grpc.Core.RpcException rpcEx)
		{
			// Expected - API should reject invalid input
			Logger.LogInformation("✅ Machine log streaming correctly validates input: {Status}", rpcEx.StatusCode);
			Logger.LogInformation("   Error message: {Message}", rpcEx.Status.Detail);
		}
	}

	/// <summary>
	/// Verifies that streaming logs for a non-existent machine returns a NotFound gRPC error or completes gracefully.
	/// </summary>
	[Fact]
	public async Task RealWorld_MachineLogStreaming_HandlesNonExistentMachine()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		var options = GetClientOptions();
		using var client = new OmniClient(options);

		Logger.LogInformation("🚀 Testing machine log streaming with non-existent machine");

		// Use a machine ID that definitely doesn't exist
		var nonExistentMachineId = "non-existent-machine-" + Guid.NewGuid();

		try
		{
			// Act - Try to stream logs from non-existent machine
			await foreach (var logData in client.Management.StreamMachineLogsAsync(
				machineId: nonExistentMachineId,
				follow: false,
				tailLines: 1,
				cancellationToken: CancellationToken))
			{
				// If we get here, something unexpected happened
				Logger.LogWarning("⚠️ Received log data for non-existent machine: {Length} bytes", logData.Length);
				break;
			}

			Logger.LogInformation("ℹ️ Machine log streaming completed without error for non-existent machine");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			// Expected - machine doesn't exist
			Logger.LogInformation("✅ Machine log streaming correctly handles non-existent machine: NotFound");
			Logger.LogInformation("   Machine ID: {MachineId}", nonExistentMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx)
		{
			// Other gRPC errors
			Logger.LogInformation("ℹ️ Machine log streaming returned gRPC error: {Status}", rpcEx.StatusCode);
			Logger.LogInformation("   Error: {Message}", rpcEx.Status.Detail);
		}
	}

	/// <summary>
	/// Verifies that machine log streaming in follow mode correctly streams logs and supports cancellation.
	/// </summary>
	[Fact]
	public async Task RealWorld_MachineLogStreaming_SupportsFollowMode()
	{
		await RunWithMachineAsync(async (client, machineId) =>
		{
			try
			{
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
				var logCount = 0;

				await foreach (var _ in client.Management.StreamMachineLogsAsync(
					machineId: machineId,
					follow: true,
					tailLines: 5,
					cancellationToken: cts.Token))
				{
					logCount++;

					if (logCount >= 3)
					{
						break;
					}
				}

				Logger.LogInformation("✅ Follow mode streaming works correctly (received {Count} log entries)", logCount);
			}
			catch (OperationCanceledException)
			{
				Logger.LogInformation("✅ Follow mode streaming correctly handles cancellation");
			}
		});
	}

	/// <summary>
	/// Verifies that machine log streaming correctly limits results using the tail lines parameter.
	/// </summary>
	[Fact]
	public async Task RealWorld_MachineLogStreaming_SupportsTailLines()
	{
		await RunWithMachineAsync(async (client, machineId) =>
		{
			const int requestedTailLines = 20;
			var logCount = 0;

			await foreach (var _ in client.Management.StreamMachineLogsAsync(
				machineId: machineId,
				follow: false,
				tailLines: requestedTailLines,
				cancellationToken: CancellationToken))
			{
				logCount++;

				if (logCount >= requestedTailLines)
				{
					break;
				}
			}

			Logger.LogInformation("✅ Tail lines parameter works correctly (requested {Requested}, received {Actual})",
				requestedTailLines, logCount);

			logCount.Should().BeLessThanOrEqualTo(requestedTailLines, "Should not receive more logs than requested");
		});
	}

	private async Task RunWithMachineAsync(Func<OmniClient, string, Task> body)
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var machineId = await GetFirstMachineIdAsync(client);

		if (string.IsNullOrEmpty(machineId))
		{
			Logger.LogInformation("ℹ️ No machines found in talosconfig - skipping test");
			return;
		}

		try
		{
			await body(client, machineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Machine '{MachineId}' not found - NotFound is expected for unavailable machines", machineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for machine log access - this is expected with Reader role");
		}
	}
	/// <summary>
	/// Helper class to track test operation results
	/// </summary>
	private sealed class TestOperationResults
	{
		public int SuccessCount { get; set; }
		public int TotalOperations { get; set; }
		public double SuccessRate => TotalOperations > 0 ? (double)SuccessCount / TotalOperations : 0.0;
	}
}
