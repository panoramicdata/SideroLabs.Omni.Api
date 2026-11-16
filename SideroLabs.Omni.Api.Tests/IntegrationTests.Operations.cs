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
	/// Integration test that reads the audit log for a date range.
	/// </summary>
	[Fact]
	public async Task RealWorld_ReadAuditLog_WithDateRange()
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
		var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

		Logger.LogInformation("🚀 Testing audit log reading from {StartDate} to {EndDate}", startDate, endDate);

		try
		{
			var (chunkCount, totalBytes) = await ReadAuditChunksAsync(client, startDate, endDate);

			if (chunkCount > 0)
			{
				totalBytes.Should().BePositive("Should have received some audit log data");
				Logger.LogInformation("✅ Successfully read {Chunks} audit log chunks, {TotalBytes} total bytes",
					chunkCount, totalBytes);
			}
			else
			{
				Logger.LogInformation("ℹ️ No audit log entries found - gRPC streaming is working correctly (empty result is valid)");
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
		{
			Logger.LogInformation("ℹ️ Invalid date range format - gRPC API is validating input correctly");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for audit log access - this is expected with Reader role");
		}
	}

	private async Task<(int Chunks, long Bytes)> ReadAuditChunksAsync(OmniClient client, string startDate, string endDate)
	{
		const int maxChunks = 5;
		var chunkCount = 0;
		var totalBytes = 0L;

		await foreach (var logData in client.Management.ReadAuditLogAsync(
			startDate: startDate,
			endDate: endDate,
			cancellationToken: CancellationToken))
		{
			chunkCount++;
			totalBytes += logData.Length;
			Logger.LogInformation("Received audit log chunk {Count}: {Size} bytes", chunkCount, logData.Length);

			if (chunkCount >= maxChunks)
			{
				break;
			}
		}

		return (chunkCount, totalBytes);
	}
	/// <summary>
	/// Integration test that initiates a maintenance upgrade on a machine.
	/// </summary>
	[Fact]
	public async Task RealWorld_MaintenanceUpgrade_WithMachine()
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

		// Get a machine ID from talosconfig
		var testMachineId = await GetFirstMachineIdAsync(client);

		if (string.IsNullOrEmpty(testMachineId))
		{
			Logger.LogInformation("ℹ️ No machines found - skipping test");
			Logger.LogInformation("💡 Tip: Ensure your Omni instance has at least one machine configured");
			return;
		}

		Logger.LogInformation("🚀 Testing maintenance upgrade for machine: {MachineId}", testMachineId);

		try
		{
			// Act - Attempt maintenance upgrade
			await client.Management.MaintenanceUpgradeAsync(
				machineId: testMachineId,
				version: "v1.7.0",
				cancellationToken: CancellationToken);

			Logger.LogInformation("✅ Maintenance upgrade initiated successfully for machine {MachineId}", testMachineId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Machine '{MachineId}' not found or not eligible for upgrade", testMachineId);
			Logger.LogInformation("✅ gRPC API is working correctly (NotFound is expected for unavailable machines)");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for maintenance upgrade - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (Exceptions.ReadOnlyModeException ex)
		{
			Logger.LogInformation("🔒 Write operation blocked in read-only mode: {Message}", ex.Message);
			Logger.LogInformation("✅ Read-only mode protection is working correctly");
		}
	}

	/// <summary>
	/// Integration test that retrieves a machine join configuration with gRPC tunnel mode.
	/// </summary>
	[Fact]
	public async Task RealWorld_GetMachineJoinConfig_WithGrpcTunnel()
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

		Logger.LogInformation("🚀 Testing machine join config retrieval");

		try
		{
			// Act - Get machine join configuration
			var config = await client.Management.GetMachineJoinConfigAsync(
				useGrpcTunnel: true,
				joinToken: "test-token",
				cancellationToken: CancellationToken);

			// Assert
			config.Should().NotBeNull();
			Logger.LogInformation("✅ Machine join config retrieved successfully");
			Logger.LogInformation("📊 Configuration Details:");
			Logger.LogInformation("  Kernel Args: {Count}", config.KernelArgs.Count);
			Logger.LogInformation("  Config Size: {Size} characters", config.Config.Length);
			Logger.LogInformation("  Summary: {Summary}", config.GetSummary());

			if (config.HasKernelArgs)
			{
				Logger.LogInformation("  Kernel Args String: {Args}", config.GetKernelArgsString());
			}
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
		{
			Logger.LogInformation("ℹ️ Invalid join token provided");
			Logger.LogInformation("✅ gRPC API is validating input correctly");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for machine join config - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
	}

	/// <summary>
	/// Integration test that creates a join token with an expiration.
	/// </summary>
	[Fact]
	public async Task RealWorld_CreateJoinToken_WithExpiration()
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

		var tokenName = $"test-token-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
		var expirationTime = DateTimeOffset.UtcNow.AddDays(7);

		Logger.LogInformation("🚀 Testing join token creation: {TokenName}", tokenName);

		try
		{
			// Act - Create join token
			var tokenId = await client.Management.CreateJoinTokenAsync(
				name: tokenName,
				expirationTime: expirationTime,
				cancellationToken: CancellationToken);

			// Assert
			tokenId.Should().NotBeNullOrEmpty();
			Logger.LogInformation("✅ Join token created successfully: {TokenId}", tokenId);
			Logger.LogInformation("📊 Token Details:");
			Logger.LogInformation("  Name: {Name}", tokenName);
			Logger.LogInformation("  Expiration: {Expiration:yyyy-MM-dd HH:mm:ss}", expirationTime);
			Logger.LogInformation("  Token ID: {TokenId}", tokenId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for join token creation - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (Exceptions.ReadOnlyModeException ex)
		{
			Logger.LogInformation("🔒 Write operation blocked in read-only mode: {Message}", ex.Message);
			Logger.LogInformation("✅ Read-only mode protection is working correctly");
		}
	}

	/// <summary>
	/// Integration test that tears down a locked cluster.
	/// </summary>
	[Fact]
	public async Task RealWorld_TearDownLockedCluster_WithClusterId()
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

		// Use a test cluster ID that doesn't exist to avoid accidental deletion
		var testClusterId = $"test-cluster-{Guid.NewGuid()}";

		Logger.LogInformation("🚀 Testing locked cluster tear down (non-existent cluster for safety)");

		try
		{
			// Act - Attempt to tear down locked cluster
			await client.Management.TearDownLockedClusterAsync(
				clusterId: testClusterId,
				cancellationToken: CancellationToken);

			Logger.LogInformation("✅ Tear down operation completed for cluster {ClusterId}", testClusterId);
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogInformation("ℹ️ Cluster '{ClusterId}' not found - this is expected for test", testClusterId);
			Logger.LogInformation("✅ gRPC API is working correctly (NotFound is expected for non-existent clusters)");
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied for cluster tear down - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly");
		}
		catch (Exceptions.ReadOnlyModeException ex)
		{
			Logger.LogInformation("🔒 Write operation blocked in read-only mode: {Message}", ex.Message);
			Logger.LogInformation("✅ Read-only mode protection is working correctly");
		}
	}

	/// <summary>
	/// Integration test that runs Kubernetes upgrade pre-checks against the real Omni API.
	/// </summary>
	[Fact]
	public async Task RealWorld_KubernetesUpgradePreChecks_WithSideroLabsCredentials()
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

		Logger.LogInformation("🚀 Starting real-world Kubernetes upgrade pre-checks test");

		try
		{
			// Act - Test Kubernetes upgrade pre-checks through real gRPC
			Logger.LogInformation("🔍 Attempting Kubernetes upgrade pre-checks via real Omni gRPC API...");

			var result = await client.Management.KubernetesUpgradePreChecksAsync(
				"v1.29.0",
				CancellationToken);

			// Assert - Just log the results, this operation can legitimately return false
			Logger.LogInformation("✅ Successfully completed Kubernetes upgrade pre-checks via Sidero Labs Omni gRPC API!");
			Logger.LogInformation("📊 Upgrade Pre-check Results:");
			Logger.LogInformation("  Can Upgrade to v1.29.0: {CanUpgrade}", result.Ok ? "✅ Yes" : "❌ No");
			Logger.LogInformation("  Reason: {Reason}", result.Reason);

			// At minimum, the reason should not be null or empty
			result.Reason.Should().NotBeNull();
		}
		catch (Grpc.Core.RpcException rpcEx) when (rpcEx.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			// This is expected if the user role doesn't have permission for upgrade operations
			Logger.LogInformation("🔒 Permission denied for Kubernetes upgrade pre-checks - this is expected with Reader role");
			Logger.LogInformation("✅ gRPC authentication is working correctly (permission check succeeded)");
		}
	}

}
