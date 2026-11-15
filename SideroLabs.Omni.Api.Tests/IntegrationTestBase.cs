using AwesomeAssertions;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Resources;
using Xunit;

namespace SideroLabs.Omni.Api.Tests;

/// <summary>
/// Base class for integration tests that need a configured, live Omni client
/// </summary>
public abstract class IntegrationTestBase(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Runs the body with a client when integration configuration exists, treating permission denied as expected
	/// </summary>
	protected async Task RunAsync(Func<OmniClient, Task> body)
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());

		try
		{
			await body(client);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
	}

	/// <summary>
	/// Lists resources of one kind and checks the first item has usable metadata
	/// </summary>
	protected Task RunListScenarioAsync<T>(string label) where T : class, IOmniResource, new() =>
		RunAsync(async client =>
		{
			try
			{
				Logger.LogInformation("🔍 Listing {Label}", label);
				var items = new List<T>();

				await foreach (var item in client.Resources.ListAsync<T>(cancellationToken: CancellationToken))
				{
					items.Add(item);
				}

				Logger.LogInformation("✅ Successfully listed {Count} {Label}", items.Count, label);

				if (items.Count > 0)
				{
					items[0].Metadata.Should().NotBeNull();
					items[0].Metadata.Id.Should().NotBeNullOrEmpty();
					Logger.LogInformation("📋 Sample {Label} ID: {Id}", label, items[0].Metadata.Id);
				}
			}
			catch (RpcException ex) when (ex.StatusCode == StatusCode.Unimplemented)
			{
				Logger.LogInformation("⏭️ {Label} resource type not available in this Omni instance", label);
			}
		});

	/// <summary>
	/// Runs the body against the shared injected client, treating permission denied (and optionally not found) as expected
	/// </summary>
	protected async Task RunSharedAsync(Func<Interfaces.IOmniClient, Task> body, bool toleratesNotFound = false)
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		try
		{
			await body(OmniClient);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (RpcException ex) when (toleratesNotFound && ex.StatusCode == StatusCode.NotFound)
		{
			Logger.LogInformation("⏭️ Resource not found - may have been deleted");
		}
	}

	/// <summary>
	/// Gets the first existing resource of a kind, or null when none exist
	/// </summary>
	protected async Task<T?> FirstOrDefaultResourceAsync<T>(Interfaces.IOmniClient client) where T : class, IOmniResource, new()
	{
		await foreach (var item in client.Resources.ListAsync<T>(limit: 1, cancellationToken: CancellationToken))
		{
			return item;
		}

		return null;
	}

	/// <summary>
	/// Deletes a test resource, ignoring failures so cleanup never masks the test result
	/// </summary>
	protected async Task CleanupAsync<T>(Interfaces.IOmniClient client, string id) where T : class, IOmniResource, new()
	{
		try
		{
			await client.Resources.DeleteAsync<T>(id, cancellationToken: CancellationToken);
			Logger.LogDebug("Cleaned up test resource: {Id}", id);
		}
		catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
		{
			Logger.LogDebug("Test resource already deleted: {Id}", id);
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to cleanup test resource: {Id}", id);
		}
	}

	/// <summary>
	/// Runs the body with a client when integration configuration exists
	/// </summary>
	protected async Task RunClientAsync(Func<OmniClient, Task> body)
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		using var client = new OmniClient(GetClientOptions());
		await body(client);
	}
}