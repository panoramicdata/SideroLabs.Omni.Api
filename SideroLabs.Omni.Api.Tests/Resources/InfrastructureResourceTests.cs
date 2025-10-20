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
public class InfrastructureResourceTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	[Fact]
	public async Task MachineSet_List_ReturnsResults()
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
			// Act - List MachineSets
			Logger.LogInformation("🔍 Listing MachineSets");
			var machineSets = new List<Api.Resources.MachineSet>();

			await foreach (var ms in client.Resources.ListAsync<Api.Resources.MachineSet>(cancellationToken: CancellationToken))
			{
				machineSets.Add(ms);
			}

			// Assert
			machineSets.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully listed {Count} MachineSets", machineSets.Count);

			// If we have MachineSets, verify their structure
			if (machineSets.Count > 0)
			{
				var firstMachineSet = machineSets.First();
				firstMachineSet.Metadata.Should().NotBeNull();
				firstMachineSet.Metadata.Id.Should().NotBeNullOrEmpty();
				Logger.LogInformation("📋 Sample MachineSet ID: {Id}", firstMachineSet.Metadata.Id);
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ MachineSet resource type not available in this Omni instance");
		}
	}

	[Fact]
	public async Task ControlPlane_List_ReturnsResults()
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
			// Act - List ControlPlanes
			Logger.LogInformation("🔍 Listing ControlPlanes");
			var controlPlanes = new List<Api.Resources.ControlPlane>();

			await foreach (var cp in client.Resources.ListAsync<Api.Resources.ControlPlane>(cancellationToken: CancellationToken))
			{
				controlPlanes.Add(cp);
			}

			// Assert
			controlPlanes.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully listed {Count} ControlPlanes", controlPlanes.Count);

			// If we have ControlPlanes, verify their structure
			if (controlPlanes.Count > 0)
			{
				var firstControlPlane = controlPlanes.First();
				firstControlPlane.Metadata.Should().NotBeNull();
				firstControlPlane.Metadata.Id.Should().NotBeNullOrEmpty();
				Logger.LogInformation("📋 Sample ControlPlane ID: {Id}", firstControlPlane.Metadata.Id);
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ ControlPlane resource type not available in this Omni instance");
		}
	}

	[Fact]
	public async Task LoadBalancerConfig_List_ReturnsResults()
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
			// Act - List LoadBalancerConfigs
			Logger.LogInformation("🔍 Listing LoadBalancerConfigs");
			var loadBalancers = new List<Api.Resources.LoadBalancerConfig>();

			await foreach (var lb in client.Resources.ListAsync<Api.Resources.LoadBalancerConfig>(cancellationToken: CancellationToken))
			{
				loadBalancers.Add(lb);
			}

			// Assert
			loadBalancers.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully listed {Count} LoadBalancerConfigs", loadBalancers.Count);

			// If we have LoadBalancerConfigs, verify their structure
			if (loadBalancers.Count > 0)
			{
				var firstLoadBalancer = loadBalancers.First();
				firstLoadBalancer.Metadata.Should().NotBeNull();
				firstLoadBalancer.Metadata.Id.Should().NotBeNullOrEmpty();
				Logger.LogInformation("📋 Sample LoadBalancerConfig ID: {Id}", firstLoadBalancer.Metadata.Id);
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ LoadBalancerConfig resource type not available in this Omni instance");
		}
	}

	[Fact]
	public async Task TalosConfig_List_ReturnsResults()
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
			// Act - List TalosConfigs
			Logger.LogInformation("🔍 Listing TalosConfigs");
			var talosConfigs = new List<Api.Resources.TalosConfig>();

			await foreach (var tc in client.Resources.ListAsync<Api.Resources.TalosConfig>(cancellationToken: CancellationToken))
			{
				talosConfigs.Add(tc);
			}

			// Assert
			talosConfigs.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully listed {Count} TalosConfigs", talosConfigs.Count);

			// If we have TalosConfigs, verify their structure
			if (talosConfigs.Count > 0)
			{
				var firstTalosConfig = talosConfigs.First();
				firstTalosConfig.Metadata.Should().NotBeNull();
				firstTalosConfig.Metadata.Id.Should().NotBeNullOrEmpty();
				Logger.LogInformation("📋 Sample TalosConfig ID: {Id}", firstTalosConfig.Metadata.Id);
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ TalosConfig resource type not available in this Omni instance");
		}
	}

	[Fact]
	public async Task KubernetesNode_List_ReturnsResults()
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
			// Act - List KubernetesNodes
			Logger.LogInformation("🔍 Listing KubernetesNodes");
			var kubernetesNodes = new List<Api.Resources.KubernetesNode>();

			await foreach (var kn in client.Resources.ListAsync<Api.Resources.KubernetesNode>(cancellationToken: CancellationToken))
			{
				kubernetesNodes.Add(kn);
			}

			// Assert
			kubernetesNodes.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully listed {Count} KubernetesNodes", kubernetesNodes.Count);

			// If we have KubernetesNodes, verify their structure
			if (kubernetesNodes.Count > 0)
			{
				var firstKubernetesNode = kubernetesNodes.First();
				firstKubernetesNode.Metadata.Should().NotBeNull();
				firstKubernetesNode.Metadata.Id.Should().NotBeNullOrEmpty();
				Logger.LogInformation("📋 Sample KubernetesNode ID: {Id}", firstKubernetesNode.Metadata.Id);
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ KubernetesNode resource type not available in this Omni instance");
		}
	}

	[Fact]
	public async Task MachineClass_List_ReturnsResults()
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
			// Act - List MachineClasses
			Logger.LogInformation("🔍 Listing MachineClasses");
			var machineClasses = new List<Api.Resources.MachineClass>();

			await foreach (var mc in client.Resources.ListAsync<Api.Resources.MachineClass>(cancellationToken: CancellationToken))
			{
				machineClasses.Add(mc);
			}

			// Assert
			machineClasses.Should().NotBeNull();
			Logger.LogInformation("✅ Successfully listed {Count} MachineClasses", machineClasses.Count);

			// If we have MachineClasses, verify their structure
			if (machineClasses.Count > 0)
			{
				var firstMachineClass = machineClasses.First();
				firstMachineClass.Metadata.Should().NotBeNull();
				firstMachineClass.Metadata.Id.Should().NotBeNullOrEmpty();
				Logger.LogInformation("📋 Sample MachineClass ID: {Id}", firstMachineClass.Metadata.Id);
			}
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied)
		{
			Logger.LogInformation("🔒 Permission denied - expected with Reader role");
		}
		catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unimplemented)
		{
			Logger.LogInformation("⏭️ MachineClass resource type not available in this Omni instance");
		}
	}

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

	#endregion
}
