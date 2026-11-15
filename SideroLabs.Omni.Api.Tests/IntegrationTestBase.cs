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
}