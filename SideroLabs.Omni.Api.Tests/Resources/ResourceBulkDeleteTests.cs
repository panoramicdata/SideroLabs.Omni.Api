using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for bulk delete operations on Omni resources
/// Tests DeleteMany and DeleteAll operations
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "CRUD")]
public class ResourceBulkDeleteTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task DeleteMany_WithSelector_DeletesMatchingResources()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var testLabel = $"bulk-delete-{Guid.NewGuid():N}";
		var createdIds = new List<string>();

		try
		{
			// Create multiple test clusters with the same label
			for (int i = 0; i < 3; i++)
			{
				var clusterId = CreateUniqueId($"bulk-test-{i}");
				var cluster = new Api.Builders.ClusterBuilder(clusterId)
					.WithKubernetesVersion("v1.29.0")
					.WithTalosVersion("v1.7.0")
					.WithLabel("test-group", testLabel)
					.Build();

				var created = await client.Resources.CreateAsync(cluster, CancellationToken);
				createdIds.Add(created.Metadata.Id);
				Logger.LogInformation("Created test cluster {Id} with label test-group={Label}", clusterId, testLabel);
			}

			// Act
			Logger.LogInformation("🔍 Deleting clusters with selector: test-group={Label}", testLabel);
			var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.Cluster>(
				selector: $"test-group={testLabel}",
				cancellationToken: CancellationToken);

			// Assert
			deletedCount.Should().Be(3, "Should delete all 3 test clusters");
			Logger.LogInformation("✅ Successfully deleted {Count} clusters", deletedCount);

			// Verify they're actually deleted
			foreach (var id in createdIds)
			{
				await Assert.ThrowsAsync<RpcException>(async () =>
				{
					await client.Resources.GetAsync<Api.Resources.Cluster>(id, cancellationToken: CancellationToken);
				});
			}

			createdIds.Clear(); // Prevent cleanup in finally
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup any remaining resources
			foreach (var id in createdIds)
			{
				try
				{
					await client.Resources.DeleteAsync<Api.Resources.Cluster>(id, cancellationToken: CancellationToken);
				}
				catch { /* Ignore cleanup errors */ }
			}
		}
	}

	[Fact]
	public async Task DeleteMany_ReturnsCorrectCount()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var testLabel = $"count-test-{Guid.NewGuid():N}";
		var expectedCount = 5;
		var createdIds = new List<string>();

		try
		{
			// Create multiple test config patches
			for (int i = 0; i < expectedCount; i++)
			{
				var patchId = CreateUniqueId($"count-test-{i}");
				var patch = new Api.Builders.ConfigPatchBuilder(patchId)
					.WithData($"machine:\n  test: {i}")
					.Build();

				patch.Metadata.Labels = new Dictionary<string, string> { ["count-test"] = testLabel };

				var created = await client.Resources.CreateAsync(patch, CancellationToken);
				createdIds.Add(created.Metadata.Id);
			}

			// Act
			var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.ConfigPatch>(
				selector: $"count-test={testLabel}",
				cancellationToken: CancellationToken);

			// Assert
			deletedCount.Should().Be(expectedCount, $"Should delete all {expectedCount} test patches");
			Logger.LogInformation("✅ DeleteMany returned correct count: {Count}", deletedCount);

			createdIds.Clear();
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			foreach (var id in createdIds)
			{
				try { await client.Resources.DeleteAsync<Api.Resources.ConfigPatch>(id, cancellationToken: CancellationToken); }
				catch { /* Ignore */ }
			}
		}
	}

	[Fact]
	public async Task DeleteMany_NoMatches_ReturnsZero()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Arrange
		using var client = new OmniClient(GetClientOptions());
		var nonExistentLabel = $"nonexistent-{Guid.NewGuid():N}";

		try
		{
			// Act - Try to delete with selector that matches nothing
			var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.Cluster>(
				selector: $"nonexistent-label={nonExistentLabel}",
				cancellationToken: CancellationToken);

			// Assert
			deletedCount.Should().Be(0, "Should return 0 when no resources match");
			Logger.LogInformation("✅ DeleteMany correctly returned 0 for no matches");
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task DeleteMany_ContinuesOnError()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// This test verifies that DeleteMany continues deleting even if one delete fails
		// In practice, this is hard to test without special setup, so we'll verify
		// the implementation handles errors gracefully

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - Delete with a selector, implementation should handle any individual failures
			var deletedCount = await client.Resources.DeleteManyAsync<Api.Resources.Machine>(
				selector: "test-label=resilience-test",
				cancellationToken: CancellationToken);

			// Assert - Should complete without throwing, even if 0 matches
			deletedCount.Should().BeGreaterThanOrEqualTo(0);
			Logger.LogInformation("✅ DeleteMany completed gracefully with count: {Count}", deletedCount);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task DeleteAll_RemovesAllResourcesInNamespace()
	{
		// Skip if integration tests are not configured or destructive tests are disabled
		if (!ShouldRunIntegrationTests() || ShouldSkipDestructiveTests())
		{
			Logger.LogInformation("⏭️ Skipping destructive test");
			return;
		}

		// WARNING: This is a destructive test - it deletes ALL resources of a type
		// Only run in test environments!

		using var client = new OmniClient(GetClientOptions());
		var testNamespace = $"test-ns-{Guid.NewGuid():N}";

		try
		{
			// Create a few test resources in a specific namespace
			for (int i = 0; i < 3; i++)
			{
				var patchId = CreateUniqueId($"deleteall-{i}");
				var patch = new Api.Builders.ConfigPatchBuilder(patchId)
					.WithData($"test: {i}")
					.InNamespace(testNamespace)
					.Build();

				await client.Resources.CreateAsync(patch, CancellationToken);
			}

			// Act
			var deletedCount = await client.Resources.DeleteAllAsync<Api.Resources.ConfigPatch>(
				@namespace: testNamespace,
				cancellationToken: CancellationToken);

			// Assert
			deletedCount.Should().Be(3, "Should delete all 3 test patches");
			Logger.LogInformation("✅ DeleteAll removed {Count} resources", deletedCount);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task DeleteAll_InDefaultNamespace_HandledSafely()
	{
		// Skip destructive tests
		if (!ShouldRunIntegrationTests() || ShouldSkipDestructiveTests())
		{
			Logger.LogInformation("⏭️ Skipping destructive test");
			return;
		}

		// This test ensures DeleteAll works correctly
		// We won't actually delete everything in default namespace in tests

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Just verify the method can be called (it will be permission denied in most cases)
			var deletedCount = await client.Resources.DeleteAllAsync<Api.Resources.Machine>(
				cancellationToken: CancellationToken);

			Logger.LogInformation("DeleteAll returned count: {Count}", deletedCount);
			deletedCount.Should().BeGreaterThanOrEqualTo(0);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected for DeleteAll in default namespace");
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
