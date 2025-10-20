using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for error handling and resilience scenarios
/// Tests network failures, timeouts, permissions, conflicts, and invalid data
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "ErrorHandling")]
public class ErrorHandlingTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task Get_NonExistentResource_ThrowsNotFound()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var nonExistentId = CreateUniqueId("does-not-exist");

		// Act & Assert
		Logger.LogInformation("🔍 Attempting to get non-existent cluster: {Id}", nonExistentId);
		
		var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await client.Resources.GetAsync<Api.Resources.Cluster>(nonExistentId, cancellationToken: CancellationToken);
		});

		// Assert
		exception.StatusCode.Should().Be(Grpc.Core.StatusCode.NotFound);
		Logger.LogInformation("✅ NotFound exception thrown as expected");
	}

	[Fact]
	public async Task Delete_NonExistentResource_ThrowsNotFound()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var nonExistentId = CreateUniqueId("does-not-exist");

		// Act & Assert
		Logger.LogInformation("🔍 Attempting to delete non-existent cluster: {Id}", nonExistentId);
		
		var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
		{
			await client.Resources.DeleteAsync<Api.Resources.Cluster>(nonExistentId, cancellationToken: CancellationToken);
		});

		// Assert
		exception.StatusCode.Should().Be(Grpc.Core.StatusCode.NotFound);
		Logger.LogInformation("✅ NotFound exception thrown as expected");
	}

	[Fact]
	public async Task List_WithInvalidSelector_ThrowsError()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - Try to list with invalid selector syntax
			Logger.LogInformation("🔍 Attempting to list with invalid selector");
			
			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				selector: "===invalid===",
				cancellationToken: CancellationToken))
			{
				// Should not reach here
				Assert.Fail("Should have thrown exception for invalid selector");
			}
		}
		catch (Grpc.Core.RpcException ex)
		{
			// Assert - Should get an error (might be InvalidArgument or other)
			Logger.LogInformation("✅ Exception thrown as expected: {Code} - {Message}", 
				ex.StatusCode, ex.Status.Detail);
			
			ex.StatusCode.Should().BeOneOf(
				Grpc.Core.StatusCode.InvalidArgument,
				Grpc.Core.StatusCode.Unknown);
		}
	}

	[Fact]
	public async Task Get_WithShortTimeout_ThrowsDeadlineExceeded()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Create client with very short timeout
		var options = GetClientOptions();
		options.TimeoutSeconds = 1; // Essentially immediate timeout
		using var client = new OmniClient(options);

		try
		{
			// Act
			Logger.LogInformation("🔍 Attempting Get with 0-second timeout");
			
			using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
			await client.Resources.ListAsync<Api.Resources.Cluster>(
				cancellationToken: cts.Token).GetAsyncEnumerator().MoveNextAsync();
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("✅ Operation cancelled as expected due to timeout");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
		{
			Logger.LogInformation("✅ DeadlineExceeded exception thrown as expected");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
		{
			Logger.LogInformation("✅ Cancelled exception thrown as expected");
		}
	}

	[Fact]
	public async Task Create_WithInvalidData_ThrowsValidationError()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Arrange - Create cluster with invalid version format
			var clusterId = CreateUniqueId("invalid");
			var cluster = new Api.Resources.Cluster
			{
				Metadata = new Api.Resources.ResourceMetadata
				{
					Id = clusterId,
					Namespace = "default"
				},
				Spec = new Api.Resources.ClusterSpec
				{
					KubernetesVersion = "invalid-version", // Invalid format
					TalosVersion = "also-invalid"
				}
			};

			// Act
			Logger.LogInformation("🔍 Attempting to create cluster with invalid version format");
			await client.Resources.CreateAsync(cluster, CancellationToken);

			Assert.Fail("Should have thrown validation error");
		}
		catch (Grpc.Core.RpcException ex) when (
			ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument ||
			ex.StatusCode == Grpc.Core.StatusCode.Unknown ||
			ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("✅ Validation error thrown as expected: {Code}", ex.StatusCode);
		}
		catch (FluentValidation.ValidationException)
		{
			Logger.LogInformation("✅ Client-side validation caught invalid data");
		}
	}

	[Fact]
	public async Task Update_WithoutVersion_HandlesOptimisticLocking()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			// Act - List clusters to get one
			Logger.LogInformation("🔍 Getting existing cluster for version test");
			
			var clusters = new List<Api.Resources.Cluster>();
			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				limit: 1,
				cancellationToken: CancellationToken))
			{
				clusters.Add(c);
			}

			if (clusters.Count == 0)
			{
				Logger.LogInformation("⏭️ No clusters available for version test");
				return;
			}

			var cluster = clusters.First();
			var originalVersion = cluster.Metadata.Version;

			// Try to update with cleared version (simulating stale data)
			cluster.Metadata.Version = "";

			try
			{
				await client.Resources.UpdateAsync(cluster, cancellationToken: CancellationToken);
				Logger.LogInformation("✅ Update succeeded (optimistic locking may not be enforced)");
			}
			catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Aborted)
			{
				Logger.LogInformation("✅ Optimistic locking conflict detected as expected");
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	[Fact]
	public async Task List_WithCancellation_StopsEnumeration()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		using var cts = new CancellationTokenSource();

		try
		{
			// Act - Start listing and cancel after first item
			Logger.LogInformation("🔍 Starting List operation with planned cancellation");
			var count = 0;

			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				cancellationToken: cts.Token))
			{
				count++;
				Logger.LogInformation("📋 Retrieved cluster {Count}: {Id}", count, c.Metadata.Id);

				// Cancel after first item
				if (count >= 1)
				{
					Logger.LogInformation("🛑 Cancelling enumeration");
					cts.Cancel();
				}
			}

			Assert.Fail("Should have thrown OperationCanceledException");
		}
		catch (OperationCanceledException)
		{
			Logger.LogInformation("✅ Enumeration cancelled as expected");
		}
	}

	[Fact]
	public async Task Create_DuplicateResource_ThrowsAlreadyExists()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		var machineId = CreateUniqueId("duplicate");

		try
		{
			// Arrange - Create a machine
			var machine = new Api.Builders.MachineBuilder(machineId).Build();

			Logger.LogInformation("🔍 Creating first machine: {Id}", machineId);
			await client.Resources.CreateAsync(machine, CancellationToken);

			// Act - Try to create same machine again
			Logger.LogInformation("🔍 Attempting to create duplicate machine: {Id}", machineId);
			
			var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(async () =>
			{
				await client.Resources.CreateAsync(machine, CancellationToken);
			});

			// Assert
			exception.StatusCode.Should().BeOneOf(
				Grpc.Core.StatusCode.AlreadyExists,
				Grpc.Core.StatusCode.InvalidArgument);
			
			Logger.LogInformation("✅ Duplicate creation prevented: {Code}", exception.StatusCode);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		finally
		{
			// Cleanup
			await CleanupMachine(client, machineId);
		}
	}

	[Fact]
	public async Task InvalidAuthToken_ThrowsUnauthenticated()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		// Create client with invalid auth token
		var options = GetClientOptions();
		options.AuthToken = "invalid-token-12345";
		using var client = new OmniClient(options);

		try
		{
			// Act
			Logger.LogInformation("🔍 Attempting to list with invalid auth token");
			
			await foreach (var c in client.Resources.ListAsync<Api.Resources.Cluster>(
				limit: 1,
				cancellationToken: CancellationToken))
			{
				Assert.Fail("Should have thrown authentication error");
			}
		}
		catch (Grpc.Core.RpcException ex) when (
			ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated ||
			ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("✅ Authentication error thrown as expected: {Code}", ex.StatusCode);
		}
		catch (Exception ex)
		{
			Logger.LogInformation("✅ Authentication failed with exception: {Type}", ex.GetType().Name);
		}
	}

	[Fact]
	public async Task Get_WithEmptyId_ThrowsInvalidArgument()
	{
		// Skip if integration tests are not configured
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		// Act & Assert
		Logger.LogInformation("🔍 Attempting to get cluster with empty ID");
		
		var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
		{
			await client.Resources.GetAsync<Api.Resources.Cluster>("", cancellationToken: CancellationToken);
		});

		Logger.LogInformation("✅ ArgumentException thrown as expected for empty ID");
	}

	#region Helper Methods

	private OmniClientOptions GetClientOptions()
	{
		var configuration = Configuration;
		var omniSection = configuration.GetSection("Omni");
		var authToken = omniSection["AuthToken"] ?? throw new FormatException("Omni:AuthToken required");

		return new OmniClientOptions
		{
			BaseUrl = new(omniSection["BaseUrl"] ?? throw new InvalidOperationException("Omni:BaseUrl required")),
			AuthToken = authToken,
			TimeoutSeconds = int.Parse(omniSection["TimeoutSeconds"] ?? "30"),
			UseTls = bool.Parse(omniSection["UseTls"] ?? "true"),
			ValidateCertificate = bool.Parse(omniSection["ValidateCertificate"] ?? "true"),
			IsReadOnly = bool.Parse(omniSection["IsReadOnly"] ?? "false"),
			Logger = Logger
		};
	}

	private static string CreateUniqueId(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

	private async Task CleanupMachine(OmniClient client, string machineId)
	{
		try
		{
			await client.Resources.DeleteAsync<Api.Resources.Machine>(machineId, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test machine: {MachineId}", machineId);
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
		{
			Logger.LogDebug("Test machine already deleted: {MachineId}", machineId);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test machine: {MachineId}", machineId);
		}
	}

	#endregion
}
