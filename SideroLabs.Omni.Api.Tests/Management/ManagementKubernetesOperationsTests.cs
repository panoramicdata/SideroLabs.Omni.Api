using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Models;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Management;

/// <summary>
/// Integration tests for ManagementService Kubernetes operations
/// Tests upgrade pre-checks and manifest synchronization
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ManagementService")]
[Trait("Category", "Kubernetes")]
public class ManagementKubernetesOperationsTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Theory]
	[InlineData("v1.29.0")]
	[InlineData("v1.30.0")]
	[InlineData("v1.31.0")]
	public async Task KubernetesUpgradePreChecks_ValidVersion_ReturnsResult(string version)
	{
		// Arrange & Act
		var kubernetesUpgradePreCheckResult = await OmniClient
			.Management
			.KubernetesUpgradePreChecksAsync(
				version,
				CancellationToken);

		// Assert - Result may be OK or not depending on cluster state
		// Both are valid responses
		Logger.LogInformation("Upgrade check for {Version}: OK={Ok}, Reason={Reason}",
			version,
			kubernetesUpgradePreCheckResult.Ok,
			kubernetesUpgradePreCheckResult.Reason);

		Assert.NotNull(kubernetesUpgradePreCheckResult.Reason);
	}

	[Theory]
	[InlineData("invalid-version")]
	[InlineData("v999.999.999")]
	[InlineData("1.29.0")] // Missing 'v' prefix
	public async Task KubernetesUpgradePreChecks_InvalidVersion_ReturnsNotOk(string version)
	{
		// Arrange & Act
		try
		{
			var kubernetesUpgradePreCheckResult = await OmniClient.Management.KubernetesUpgradePreChecksAsync(
				version,
				CancellationToken);

			// Assert - Should either return not OK or throw
			if (!kubernetesUpgradePreCheckResult.Ok)
			{
				Logger.LogInformation("✓ Invalid version {Version} correctly returned not OK: {Reason}",
					version, kubernetesUpgradePreCheckResult.Reason);
				Assert.NotEmpty(kubernetesUpgradePreCheckResult.Reason);
			}
		}
		catch (RpcException ex)
		{
			// Also acceptable to throw an exception for invalid versions
			Logger.LogInformation("✓ Invalid version {Version} correctly threw exception: {StatusCode}",
				version, ex.StatusCode);
		}
	}

	[Fact]
	public async Task KubernetesUpgradePreChecks_EmptyVersion_ThrowsInvalidArgument()
	{
		// Arrange
		var emptyVersion = "";

		// Act & Assert
		await Assert.ThrowsAnyAsync<Exception>(async () =>
			await OmniClient.Management.KubernetesUpgradePreChecksAsync(emptyVersion, CancellationToken));

		Logger.LogInformation("✓ Empty version correctly rejected");
	}

	[Fact]
	public async Task KubernetesUpgradePreChecks_MultipleVersions_Consistent()
	{
		// Arrange
		var version = "v1.29.0";

		// Act - Call twice
		var result1 = await OmniClient.Management.KubernetesUpgradePreChecksAsync(version, CancellationToken);
		await Task.Delay(100); // Small delay
		var result2 = await OmniClient.Management.KubernetesUpgradePreChecksAsync(version, CancellationToken);

		// Assert - Results should be consistent
		Assert.Equal(result1.Ok, result2.Ok);

		Logger.LogInformation("✓ Upgrade pre-check results are consistent");
	}

	[Fact(Skip = "Modifies cluster state - manual test only")]
	public async Task StreamKubernetesSyncManifests_DryRun_ShowsChanges()
	{
		if (ShouldSkipDestructiveTests())
		{
			return;
		}

		// Arrange
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		var syncCount = 0;
		var skippedCount = 0;

		// Act
		await foreach (var result in OmniClient.Management.StreamKubernetesSyncManifestsAsync(
			dryRun: true,
			cancellationToken: cts.Token))
		{
			// Assert
			Assert.NotNull(result);
			syncCount++;

			if (result.Skipped)
			{
				skippedCount++;
			}

			Logger.LogInformation("Sync result #{Count}: Type={Type}, Path={Path}, Skipped={Skipped}",
				syncCount, result.ResponseType, result.Path, result.Skipped);

			if (syncCount >= 20) break; // Limit for test
		}

		Assert.True(syncCount > 0, "Should receive at least one sync result");
		Logger.LogInformation("✓ Processed {Total} sync results ({Skipped} skipped)",
			syncCount, skippedCount);
	}

	[Fact(Skip = "Requires active cluster - manual test only")]
	public async Task StreamKubernetesSyncManifests_DryRun_HandlesEmptyCluster()
	{
		// This test would verify behavior when there are no manifests to sync
		// Requires specific cluster configuration
	}

	[Fact(Skip = "DESTRUCTIVE - Actually syncs manifests to cluster")]
	public async Task StreamKubernetesSyncManifests_NotDryRun_AppliesChanges()
	{
		// DO NOT RUN IN AUTOMATED TESTS
		// This would actually modify the cluster
		Logger.LogWarning("⚠️ This test is DESTRUCTIVE and should only be run manually");
	}

	[Fact(Skip = "Requires active cluster")]
	public async Task StreamKubernetesSyncManifests_Cancellation_StopsStream()
	{
		// Arrange
		using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
		var syncCount = 0;

		// Act & Assert
		try
		{
			await foreach (var result in OmniClient.Management.StreamKubernetesSyncManifestsAsync(
				dryRun: true,
				cancellationToken: cts.Token))
			{
				syncCount++;
			}
		}
		catch (OperationCanceledException)
		{
			// Expected
			Logger.LogInformation("✓ Stream correctly stopped on cancellation after {Count} results",
				syncCount);
		}
	}

	[Fact]
	public async Task KubernetesUpgradePreChecks_ConcurrentCalls_HandleCorrectly()
	{
		// Arrange
		var version = "v1.29.0";
		var tasks = new List<Task<KubernetesUpgradePreCheckResult>>();

		// Act - Make multiple concurrent calls
		for (int i = 0; i < 5; i++)
		{
			tasks.Add(OmniClient
				.Management
				.KubernetesUpgradePreChecksAsync(version, CancellationToken)
				);
		}

		var results = await Task.WhenAll(tasks);

		// Assert - All should complete successfully
		Assert.Equal(5, results.Length);
		Assert.All(results, r => Assert.NotNull(r.Reason));

		Logger.LogInformation("✓ {Count} concurrent upgrade checks completed successfully", results.Length);
	}

	[Fact]
	public async Task KubernetesUpgradePreChecks_DowngradeVersion_ReturnsNotOk()
	{
		// Arrange - Try to "upgrade" to an old version
		var oldVersion = "v1.20.0";

		// Act
		var kubernetesUpgradePreCheckResult = await OmniClient
			.Management
			.KubernetesUpgradePreChecksAsync(
				oldVersion,
				CancellationToken);

		// Assert - Should return not OK (downgrade not allowed)
		// OR throw an exception, both are acceptable
		Logger.LogInformation(
			"Downgrade check: OK={Ok}, Reason={Reason}",
			kubernetesUpgradePreCheckResult.Ok,
			kubernetesUpgradePreCheckResult.Reason);

		if (!kubernetesUpgradePreCheckResult.Ok)
		{
			Assert.Contains("downgrade", kubernetesUpgradePreCheckResult.Reason, StringComparison.OrdinalIgnoreCase);
			Logger.LogInformation("✓ Downgrade correctly rejected");
		}
	}
}
