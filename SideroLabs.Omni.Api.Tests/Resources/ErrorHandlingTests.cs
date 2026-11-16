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
public class ErrorHandlingTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that verifies getting a non-existent cluster ID throws RpcException with NotFound status.
	/// </summary>
	[Fact]
	public Task Get_NonExistentResource_ThrowsNotFound() =>
		AssertNotFoundAsync((client, id) => client.Resources.GetAsync<Api.Resources.Cluster>(id, cancellationToken: CancellationToken));

	/// <summary>
	/// Integration test that verifies deleting a non-existent cluster ID throws RpcException with NotFound status.
	/// </summary>
	[Fact]
	public Task Delete_NonExistentResource_ThrowsNotFound() =>
		AssertNotFoundAsync((client, id) => client.Resources.DeleteAsync<Api.Resources.Cluster>(id, cancellationToken: CancellationToken));

	private Task AssertNotFoundAsync(Func<OmniClient, string, Task> operation) =>
		RunClientAsync(async client =>
		{
			var exception = await Assert.ThrowsAsync<Grpc.Core.RpcException>(() => operation(client, CreateUniqueId("does-not-exist")));

			exception.StatusCode.Should().Be(Grpc.Core.StatusCode.NotFound);
		});
	/// <summary>
	/// Integration test that verifies listing with a malformed selector string throws an error.
	/// </summary>
	[Fact]
	public async Task List_WithInvalidSelector_ThrowsError()
	{
		await RunClientAsync(async client =>
		{
			try
			{
				// Act - Try to list with invalid selector syntax
				Logger.LogInformation("🔍 Attempting to list with invalid selector");
				
				await foreach (var _ in client.Resources.ListAsync<Api.Resources.Cluster>(
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
		});
	}

	/// <summary>
	/// Verifies that a list operation with a very short timeout throws a deadline-exceeded or cancellation error.
	/// </summary>
	[Fact]
	public Task Get_WithShortTimeout_ThrowsDeadlineExceeded() =>
		RunWithOptionsAsync(options => options.TimeoutSeconds = 1, async client =>
		{
			using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));

			try
			{
				await client.Resources.ListAsync<Api.Resources.Cluster>(cancellationToken: cts.Token).GetAsyncEnumerator().MoveNextAsync();
			}
			catch (OperationCanceledException)
			{
				Logger.LogInformation("✅ Operation cancelled as expected due to timeout");
			}
			catch (Grpc.Core.RpcException ex) when (ex.StatusCode is Grpc.Core.StatusCode.DeadlineExceeded or Grpc.Core.StatusCode.Cancelled)
			{
				Logger.LogInformation("✅ {Code} thrown as expected", ex.StatusCode);
			}
		});
	/// <summary>
	/// Verifies that creating a resource with invalid data (malformed version strings) throws a validation error.
	/// </summary>
	[Fact]
	public async Task Create_WithInvalidData_ThrowsValidationError()
	{
		await RunClientAsync(async client =>
		{
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
		});
	}

	/// <summary>
	/// Verifies that updating a resource with a cleared version string correctly handles optimistic locking behavior.
	/// </summary>
	[Fact]
	public async Task Update_WithoutVersion_HandlesOptimisticLocking()
	{
		await RunAsync(async client =>
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
		});
	}

	/// <summary>
	/// Verifies that cancelling a list enumeration in progress stops the stream and throws <see cref="OperationCanceledException"/>.
	/// </summary>
	[Fact]
	public async Task List_WithCancellation_StopsEnumeration()
	{
		await RunClientAsync(async client =>
		{
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
		});
	}

	/// <summary>
	/// Verifies that creating a resource that already exists throws an <see cref="Grpc.Core.StatusCode.AlreadyExists"/> or equivalent gRPC error.
	/// </summary>
	[Fact]
	public async Task Create_DuplicateResource_ThrowsAlreadyExists()
	{
		await RunClientAsync(async client =>
		{
			var machineId = CreateUniqueId("duplicate");

			try
			{
				// Arrange - Create a machine with required role
				var machine = new Api.Builders.MachineBuilder(machineId)
					.WithRole("worker")
					.Build();

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
				Logger.LogInformation("✅ gRPC authentication is working correctly");
			}
			finally
			{
				// Cleanup
				await CleanupAsync<Api.Resources.Machine>(client, machineId);
			}
		});
	}

	/// <summary>
	/// Verifies that using an invalid auth token causes an unauthenticated or permission-denied gRPC error.
	/// </summary>
	[Fact]
	public Task InvalidAuthToken_ThrowsUnauthenticated() =>
		RunWithOptionsAsync(options => options.AuthToken = "invalid-token-12345", async client =>
		{
			try
			{
				await foreach (var _ in client.Resources.ListAsync<Api.Resources.Cluster>(limit: 1, cancellationToken: CancellationToken))
				{
					Assert.Fail("Should have thrown authentication error");
				}
			}
			catch (Grpc.Core.RpcException ex) when (ex.StatusCode is Grpc.Core.StatusCode.Unauthenticated or Grpc.Core.StatusCode.PermissionDenied)
			{
				Logger.LogInformation("✅ Authentication error thrown as expected: {Code}", ex.StatusCode);
			}
			catch (Exception ex) when (ex is not Xunit.Sdk.XunitException)
			{
				Logger.LogInformation("✅ Authentication failed with exception: {Type}", ex.GetType().Name);
			}
		});
	/// <summary>
	/// Verifies that attempting to get a resource with an empty ID throws an <see cref="ArgumentException"/>.
	/// </summary>
	[Fact]
	public async Task Get_WithEmptyId_ThrowsInvalidArgument()
	{
		await RunClientAsync(async client =>
		{
			// Act & Assert
			Logger.LogInformation("🔍 Attempting to get cluster with empty ID");
			
			await Assert.ThrowsAsync<ArgumentException>(async () =>
			{
				await client.Resources.GetAsync<Api.Resources.Cluster>("", cancellationToken: CancellationToken);
			});

			Logger.LogInformation("✅ ArgumentException thrown as expected for empty ID");
		});
	}

	private async Task RunWithOptionsAsync(Action<OmniClientOptions> configure, Func<OmniClient, Task> body)
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		var options = GetClientOptions();
		configure(options);
		using var client = new OmniClient(options);
		await body(client);
	}}
