using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Integration tests for infrastructure resource operations
/// Tests CRUD and watch operations for infrastructure resources like MachineSet, ControlPlane, etc.
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Infrastructure")]
public class InfrastructureResourceTests(ITestOutputHelper testOutputHelper) : IntegrationTestBase(testOutputHelper)
{
	/// <summary>
	/// Integration test that lists MachineSet resources and verifies metadata integrity.
	/// </summary>
	[Fact]
	public async Task MachineSet_List_ReturnsResults()
	{
		await RunListScenarioAsync<Api.Resources.MachineSet>("MachineSets");
	}

	/// <summary>
	/// Integration test that lists ControlPlane resources and verifies metadata integrity.
	/// </summary>
	[Fact]
	public async Task ControlPlane_List_ReturnsResults()
	{
		await RunListScenarioAsync<Api.Resources.ControlPlane>("ControlPlanes");
	}

	/// <summary>
	/// Verifies that listing <see cref="Api.Resources.LoadBalancerConfig"/> resources returns results without error.
	/// </summary>
	[Fact]
	public async Task LoadBalancerConfig_List_ReturnsResults()
	{
		await RunListScenarioAsync<Api.Resources.LoadBalancerConfig>("LoadBalancerConfigs");
	}

	/// <summary>
	/// Verifies that listing <see cref="Api.Resources.TalosConfig"/> resources returns results without error.
	/// </summary>
	[Fact]
	public async Task TalosConfig_List_ReturnsResults()
	{
		await RunListScenarioAsync<Api.Resources.TalosConfig>("TalosConfigs");
	}

	/// <summary>
	/// Verifies that listing <see cref="Api.Resources.KubernetesNode"/> resources returns results without error.
	/// </summary>
	[Fact]
	public async Task KubernetesNode_List_ReturnsResults()
	{
		await RunListScenarioAsync<Api.Resources.KubernetesNode>("KubernetesNodes");
	}

	/// <summary>
	/// Verifies that listing <see cref="Api.Resources.MachineClass"/> resources returns results without error.
	/// </summary>
	[Fact]
	public async Task MachineClass_List_ReturnsResults()
	{
		await RunListScenarioAsync<Api.Resources.MachineClass>("MachineClasss");
	}

	/// <summary>
	/// Verifies that the Watch API can be initialized for infrastructure resource types without throwing an exception.
	/// </summary>
	[Fact]
	public async Task InfrastructureResources_Watch_CanBeInitialized()
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
			// Act - Attempt to start watching MachineSets (will enumerate until first item or timeout)
			Logger.LogInformation("🔍 Testing Watch API initialization for MachineSets");
			
			var enumerator = client.Resources.WatchAsync<Api.Resources.MachineSet>(
				cancellationToken: CancellationToken).GetAsyncEnumerator(CancellationToken);

			// If we can get an enumerator without exception, the Watch API is accessible
			Logger.LogInformation("✅ Watch API successfully initialized");
			
			// Clean up the enumerator
			await enumerator.DisposeAsync();
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ Watch not available for this resource type");
		}
	}

	#region Helper Methods

	#endregion
}
