using Grpc.Core;
using Microsoft.Extensions.Logging;
using SideroLabs.Omni.Api.Resources;
using SideroLabs.Omni.Api.Utilities;
using Xunit;

namespace SideroLabs.Omni.Api.Tests.Resources;

/// <summary>
/// Diagnostic tests to verify ResourceService endpoint accessibility and capture HTTP response bodies
/// </summary>
[Collection("Integration")]
[Trait("Category", "Integration")]
[Trait("Category", "Diagnostic")]
public class ResourceServiceDiagnosticTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
	/// <summary>
	/// Integration diagnostic test that lists Cluster resources and captures any HTTP response body for debugging.
	/// </summary>
	[Fact]
	public async Task ResourceService_ListClusters_CaptureHttpResponseBody()
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		try
		{
			Logger.LogInformation("🔍 Listing Cluster resources at {BaseUrl} as {Identity}", OmniClient.BaseUrl, OmniClient.Identity ?? "(none)");

			var clusters = new List<Cluster>();
			await foreach (var cluster in OmniClient.Resources.ListAsync<Cluster>(
				@namespace: "default",
				cancellationToken: CancellationToken))
			{
				clusters.Add(cluster);
			}

			Logger.LogInformation("✅ Successfully listed {Count} clusters via ResourceService!", clusters.Count);

			foreach (var cluster in clusters.Take(3))
			{
				Logger.LogInformation("  - Cluster: {Name} (Kubernetes: {Version})", cluster.Metadata.Id, cluster.Spec.KubernetesVersion);
			}
		}
		catch (RpcException ex)
		{
			Logger.LogError(ex, "❌ gRPC Error with HTTP details: {Details}", GrpcErrorParser.FormatDetailedError(ex));
			throw;
		}
	}

	/// <summary>
	/// Baseline integration test that verifies the ManagementService is reachable and returns a valid Omni config.
	/// </summary>
	[Fact]
	public async Task ManagementService_Baseline_Success()
	{
		if (!ShouldRunIntegrationTests())
		{
			Logger.LogInformation("⏭️ Skipping integration test - no valid Omni configuration");
			return;
		}

		try
		{
			var omniconfig = await OmniClient.Management.GetOmniConfigAsync(CancellationToken);

			Logger.LogInformation("✅ ManagementService works: {Length} chars", omniconfig.Length);
			Assert.NotEmpty(omniconfig);
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "❌ Even ManagementService failed: {Message}", ex.Message);
			throw;
		}
	}
}
